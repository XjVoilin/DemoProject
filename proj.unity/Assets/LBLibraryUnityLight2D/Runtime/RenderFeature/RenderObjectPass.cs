using System;
using System.Collections.Generic;
using LIBII.Light2D;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Custom2DLightRenderFeature
{
    public class RenderObjectPass : ScriptableRenderPass
    {
        private const string kLightDepthTexture = "_LightDepthTexture";
        private const string kCameraTargetAttachmentA = "_CameraColorAttachmentA";
        private static readonly int s_DrawObjectPassDataPropID = Shader.PropertyToID("_DrawObjectPassData");
        private static readonly int scaleBiasRt = Shader.PropertyToID("_ScaleBiasRt");
        private static readonly int LightIntensity = Shader.PropertyToID("_LightIntensity");
        private static readonly int BloomIntensity = Shader.PropertyToID("_BloomIntensity");
        private static readonly int Filter = Shader.PropertyToID("_Filter");
        private static readonly int GlobalColor = Shader.PropertyToID("_GlobalColor");
        private ProfilingSampler m_DownSampler = new ProfilingSampler("Downsample");
        private ProfilingSampler m_UpSampler = new ProfilingSampler("Upsample");


        private RenderTargetHandle m_LightDepth, m_CameraTargetAttachmentA;
        private RenderTextureDescriptor cameraTextureDesc;
        ProfilingSampler m_ProfilingSampler;
        private Material m_BloomMaterial, m_CombineLightMat, m_ApplyBoomMaterial;
        private PassParameters m_Paramters;
        private int w, h;

        RenderTargetHandle[] textures = new RenderTargetHandle[16];
        const int BoxDownPrefilterPass = 0;
        const int BoxDownPass = 1;
        const int BoxUpPass = 2;
        FilteringSettings m_FilteringSettings;
        RenderStateBlock m_RenderStateBlock;
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

        private bool isBloomEnabled;

        public RenderObjectPass(PassParameters parameters)
        {
            m_Paramters = parameters;
            base.profilingSampler = new ProfilingSampler(nameof(RenderObjectPass));
            m_ProfilingSampler = new ProfilingSampler(parameters.profilerTag);
            foreach (ShaderTagId sid in parameters.shaderTagIds)
            {
                m_ShaderTagIdList.Add(sid);
            }


            renderPassEvent = parameters.renderEvent;
            m_FilteringSettings = new FilteringSettings(parameters.renderQueueRange, parameters.layerMask);
            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

            if (parameters.stencilState.enabled)
            {
                m_RenderStateBlock.stencilReference = parameters.stencilReference;
                m_RenderStateBlock.mask = RenderStateMask.Stencil;
                m_RenderStateBlock.stencilState = parameters.stencilState;
            }
        }

        public void SetUp(PassParameters passParameters)
        {
            m_Paramters = passParameters;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (m_RenderStateBlock.depthState.compareFunction == CompareFunction.Equal)
            {
                m_RenderStateBlock.depthState = new DepthState(true, CompareFunction.LessEqual);
                // m_RenderStateBlock.mask |= RenderStateMask.Depth;
            }
        }


        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var rtd = cameraTextureDescriptor;
            cameraTextureDesc = rtd;
            m_LightDepth.Init(kLightDepthTexture);
            m_CameraTargetAttachmentA.Init(kCameraTargetAttachmentA);

            cmd.GetTemporaryRT(m_CameraTargetAttachmentA.id, rtd, FilterMode.Bilinear);
            cmd.GetTemporaryRT(m_LightDepth.id, rtd, FilterMode.Bilinear);

            w = cameraTextureDescriptor.width;
            h = cameraTextureDescriptor.height;

            if (!m_CombineLightMat)
                m_CombineLightMat = new Material(Shader.Find("LIBII/Internal/CombineLight"));


            if (!m_BloomMaterial)
            {
                m_BloomMaterial = new Material(Shader.Find("LIBII/Internal/Bloom"));
                m_BloomMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            if (!m_ApplyBoomMaterial)
                m_ApplyBoomMaterial = new Material(Shader.Find("LIBII/Internal/ApplyBloom"));

            m_CombineLightMat.SetColor(GlobalColor, m_Paramters.globalLightColor);
            m_CombineLightMat.SetFloat(LightIntensity, m_Paramters.lightDensity);

            m_BloomMaterial.SetVector(Filter, m_Paramters.Filter);
            m_BloomMaterial.SetFloat(BloomIntensity, m_Paramters.GammaToLinearSpaceIntensity);

            m_ApplyBoomMaterial.SetFloat(BloomIntensity, m_Paramters.intensity);
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // NOTE: Do NOT mix ProfilingScope with named CommandBuffers i.e. CommandBufferPool.Get("name").
            // Currently there's an issue which results in mismatched markers.
            ref CameraData cameraData = ref renderingData.cameraData;
            var camera = cameraData.camera;

            CommandBuffer cmd = CommandBufferPool.Get("Light2D");

#if UNITY_EDITOR
            m_Paramters.enableBloom &=
                m_Paramters.debugMode == DebugMode.none;
#endif
            isBloomEnabled = m_Paramters.enableBloom && !cameraData.isPreviewCamera && !cameraData.isSceneViewCamera;


            var albedo = new AttachmentDescriptor(cameraTextureDesc.colorFormat);
            var albedo1 = new AttachmentDescriptor(cameraTextureDesc.colorFormat);
            var finalAlbedo = new AttachmentDescriptor(cameraTextureDesc.colorFormat);
#if UNITY_EDITOR
            var depth = new AttachmentDescriptor(RenderTextureFormat.Depth);
#endif

            albedo.ConfigureTarget(m_CameraTargetAttachmentA.id, false, true);
            albedo1.ConfigureTarget(m_LightDepth.id, false, true);
            // albedo.ConfigureResolveTarget(m_CameraTargetAttachmentA.id);
            // albedo1.ConfigureResolveTarget(m_LightDepth.id);

            albedo.ConfigureClear(Color.clear, 1, 0);
            albedo1.ConfigureClear(Color.clear, 1, 0);
#if UNITY_EDITOR
            depth.ConfigureClear(Color.clear, 1.0f, 0);
#endif
            const int indexAlbedo = 0, indexAlbedo1 = 1, indexFinal = 2, indexDepth = 3;


            var attachments = new NativeArray<AttachmentDescriptor>(
#if UNITY_EDITOR
                4,
#else
               2,
#endif
                Allocator.Temp);
            attachments[indexAlbedo] = albedo;
            attachments[indexAlbedo1] = albedo1;
            attachments[indexFinal] = finalAlbedo;

#if UNITY_EDITOR
            attachments[indexDepth] = depth;
#endif

            using (context.BeginScopedRenderPass(camera.pixelWidth, camera.pixelHeight, 1, attachments,
#if UNITY_EDITOR
                       indexDepth
#else
                -1
#endif
                   ))
            {
                attachments.Dispose();

                var bufferColors = new NativeArray<int>(2, Allocator.Temp);
                bufferColors[0] = indexAlbedo;
                bufferColors[1] = indexAlbedo1;
                using (context.BeginScopedSubPass(bufferColors))
                {
                    bufferColors.Dispose();

                    cmd.Clear();
                    /*using (new ProfilingScope(cmd, m_ProfilingSampler))*/
                    {
                        // Global render pass data containing various settings.
                        // x,y,z are currently unused
                        // w is used for knowing whether the object is opaque(1) or alpha blended(0)
                        Vector4 drawObjectPassData = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
                        cmd.SetGlobalVector(s_DrawObjectPassDataPropID, drawObjectPassData);


                        // scaleBias.x = flipSign
                        // scaleBias.y = scale
                        // scaleBias.z = bias
                        // scaleBias.w = unused
                        float flipSign = (renderingData.cameraData.IsCameraProjectionMatrixFlipped()) ? -1.0f : 1.0f;
                        Vector4 scaleBias = (flipSign < 0.0f)
                            ? new Vector4(flipSign, 1.0f, -1.0f, 1.0f)
                            : new Vector4(flipSign, 0.0f, 1.0f, 1.0f);
                        cmd.SetGlobalVector(scaleBiasRt, scaleBias);

                        context.ExecuteCommandBuffer(cmd);
                        cmd.Clear();

                        var sortFlags = SortingCriteria.CommonTransparent;
                        // if ((renderingData.cameraData.renderType == CameraRenderType.Base ||
                        //      renderingData.cameraData.clearDepth))
                        //     sortFlags = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue |
                        //                 SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;

                        var filterSettings = m_FilteringSettings;

#if UNITY_EDITOR
                        // When rendering the preview camera, we want the layer mask to be forced to Everything
                        if (renderingData.cameraData.isPreviewCamera)
                        {
                            filterSettings.layerMask = -1;
                        }
#endif
                        DrawingSettings drawSettings =
                            CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortFlags);


                        context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings,
                            ref m_RenderStateBlock);

                        // Render objects that did not match any shader pass with error shader
                        // TODO: When importing project, AssetPreviewUpdater::CreatePreviewForAsset will be called multiple times.
                        // This might be in a point that some resources required for the pipeline are not finished importing yet.
                        // Proper fix is to add a fence on asset import.
                        if (errorMaterial == null)
                            return;

                        SortingSettings sortingSettings = new SortingSettings(camera) { criteria = sortFlags };
                        DrawingSettings errorSettings = new DrawingSettings(m_LegacyShaderPassNames[0], sortingSettings)
                        {
                            perObjectData = PerObjectData.None,
                            overrideMaterial = errorMaterial,
                            overrideMaterialPassIndex = 0
                        };
                        for (int i = 1; i < m_LegacyShaderPassNames.Count; ++i)
                            errorSettings.SetShaderPassName(i, m_LegacyShaderPassNames[i]);

                        context.DrawRenderers(renderingData.cullResults, ref errorSettings, ref filterSettings);
                    }

                    context.ExecuteCommandBuffer(cmd);
                    CommandBufferPool.Release(cmd);
                }
            }

            RenderTargetHandle currentSource = m_LightDepth;


            if (isBloomEnabled)
            {
                cmd = CommandBufferPool.Get("Bloom");
                int width = w / 2;
                int height = h / 2;

                RenderTargetHandle currentDestination = new RenderTargetHandle();
                currentDestination.Init("_Temp");
                textures[0] = currentDestination;
                cmd.GetTemporaryRT(currentDestination.id, width, height, 0, FilterMode.Bilinear,
                    cameraTextureDesc.colorFormat);

                cmd.Blit(currentSource.id,
                    currentDestination.id,
                    m_BloomMaterial, BoxDownPrefilterPass);

                currentSource = currentDestination;
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                int i = 1;
                // Debug.Log($"=====> start");

                using (new ProfilingScope(cmd, m_DownSampler))
                {
                    for (; i < m_Paramters.iterations; i++)
                    {
                        width /= 2;
                        height /= 2;
                        if (height < 2)
                        {
                            break;
                        }

                        currentDestination = new RenderTargetHandle();
                        currentDestination.Init("_TempDown" + i);
                        // Debug.Log($"=====> set {i}");
                        cmd.GetTemporaryRT(currentDestination.id, width, height, 0, FilterMode.Bilinear,
                            cameraTextureDesc.colorFormat);
                        textures[i] = currentDestination;
                        cmd.Blit(currentSource.id, currentDestination.id, m_BloomMaterial, BoxDownPass);
                        currentSource = currentDestination;
                    }
                }

                using (new ProfilingScope(cmd, m_UpSampler))
                {
                    for (i -= 2; i >= 0; i--)
                    {
                        currentDestination = textures[i];
                        // Debug.Log($"=====> get {i}");

                        cmd.Blit(currentSource.id, currentDestination.id, m_BloomMaterial, BoxUpPass);
                        currentSource = currentDestination;
                    }
                }

                cmd.Blit(m_LightDepth.id, currentSource.id, m_ApplyBoomMaterial);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }


            cmd = CommandBufferPool.Get("BlitToScreen");
            cmd.Clear();
            if (!cameraData.isPreviewCamera)
            {
                cmd.SetRenderTarget(m_CameraTargetAttachmentA.id, RenderBufferLoadAction.Load,
                    RenderBufferStoreAction.Store);


                cmd.Blit(currentSource.id, m_CameraTargetAttachmentA.id, m_CombineLightMat);
            }
            else
            {
                cmd.Blit(m_CameraTargetAttachmentA.id, BuiltinRenderTextureType.CameraTarget);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Release();
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");
            for (int j = 0; j < textures.Length; j++)
            {
                if (textures[j] != default)
                {
                    cmd.ReleaseTemporaryRT(textures[j].id);
                    textures[j] = default;
                }
            }

            cmd.ReleaseTemporaryRT(m_LightDepth.id);
            cmd.ReleaseTemporaryRT(m_CameraTargetAttachmentA.id);
        }

        static List<ShaderTagId> m_LegacyShaderPassNames = new List<ShaderTagId>
        {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM"),
        };

        static Material s_ErrorMaterial;

        static Material errorMaterial
        {
            get
            {
                if (s_ErrorMaterial == null)
                {
                    // TODO: When importing project, AssetPreviewUpdater::CreatePreviewForAsset will be called multiple times.
                    // This might be in a point that some resources required for the pipeline are not finished importing yet.
                    // Proper fix is to add a fence on asset import.
                    try
                    {
                        s_ErrorMaterial = new Material(Shader.Find("Hidden/Universal Render Pipeline/FallbackError"));
                    }
                    catch
                    {
                    }
                }

                return s_ErrorMaterial;
            }
        }
    }
}