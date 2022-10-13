using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LIBII.Light2D
{
    [Serializable]
    public class MarkLightAreaPass : ScriptableRenderPass
    {
        const string m_ProfilerTag = "LightArea";
        [SerializeField] private Material m_DepthLightMaterial, m_BlitMaterial, m_FinalBlitMaterial, m_BloomMaterial;

        private PassParameters m_Paramters;

        const int BoxDownPrefilterPass = 0;
        const int BoxDownPass = 1;
        const int BoxUpPass = 2;
        const string kLightResultTexture = "_LightResultTexture";
        private ProfilingSampler m_DownSampler = new ProfilingSampler("Downsample");
        private ProfilingSampler m_UpSampler = new ProfilingSampler("Upsample");

        void CreateMaterial()
        {
            if (!m_BlitMaterial)
            {
                m_BlitMaterial = new Material(Shader.Find("LIBII/Internal/BlitColor"));
                m_FinalBlitMaterial = new Material(Shader.Find("LIBII/Internal/ApplyBloom"));

                m_DepthLightMaterial = new Material(Shader.Find("LIBII/Internal/DepthLightBlend"));
            }
        }

        public void Setup(PassParameters parameters)
        {
            m_Paramters = parameters;

            CreateMaterial();
            m_DepthLightMaterial.SetColor(GlobalColor, m_Paramters.globalLightColor);
            m_DepthLightMaterial.SetFloat(Intensity, m_Paramters.lightDensity);


            m_FinalBlitMaterial.SetFloat(GlobalIntensity, m_Paramters.globalLightColor.a * 2);


            m_BlitMaterial.SetFloat(Intensity, m_Paramters.GammaToLinearSpaceIntensity);

#if UNITY_EDITOR
            if (m_Paramters.debugMode == DebugMode.none)
            {
                m_BlitMaterial.SetInt(SrcBlend, (int) m_Paramters.srcOpt);
                m_BlitMaterial.SetInt(DstBlend, (int) m_Paramters.dstOpt);
            }
            else
            {
                m_BlitMaterial.SetInt(SrcBlend, (int) BlendMode.One);
                m_BlitMaterial.SetInt(DstBlend, (int) BlendMode.Zero);
            }

            SetMaterialMode(m_Paramters.debugMode);
#else
            m_BlitMaterial.SetInt(SrcBlend, (int) m_Paramters.srcOpt);
            m_BlitMaterial.SetInt(DstBlend, (int) m_Paramters.dstOpt);
#endif
        }

#if UNITY_EDITOR
        void SetMaterialMode(DebugMode debugMode)
        {
            // SetKeyword(m_DepthLightMaterial, "DEBUG_DEPTH", debugMode == DebugMode.depth);
            // SetKeyword(m_DepthLightMaterial, "DEBUG_LIGHT", debugMode == DebugMode.light);
            // SetKeyword(m_DepthLightMaterial, "DEBUG_DEPTH_GAP", debugMode == DebugMode.depthGap);
            // SetKeyword(m_DepthLightMaterial, "DEBUG_GAP_LIGHT", debugMode == DebugMode.depthGapWithLight);
            // SetKeyword(m_DepthLightMaterial, "DEBUG_GAP_LIGHT_GLOBAL", debugMode == DebugMode.depthGapLightWithGlobal);
        }
#endif
        static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }

        private int w, h;
        private RenderTargetHandle m_LightResult;
        private RenderTextureFormat format;
        RenderTargetHandle[] textures = new RenderTargetHandle[16];
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int Filter = Shader.PropertyToID("_Filter");
        private static readonly int GlobalColor = Shader.PropertyToID("_GlobalColor");
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int GlobalIntensity = Shader.PropertyToID("_GlobalIntensity");

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var rtd = cameraTextureDescriptor;
            format = rtd.colorFormat;
            m_LightResult.Init(kLightResultTexture);
            w = cameraTextureDescriptor.width;
            h = cameraTextureDescriptor.height;
            cmd.GetTemporaryRT(m_LightResult.id, rtd, FilterMode.Bilinear);
            ConfigureTarget(m_LightResult.Identifier(),
                m_LightResult.Identifier());

            ConfigureClear(ClearFlag.All, Color.clear);
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            RenderTargetIdentifier cameraTarget = (cameraData.targetTexture != null)
                ? new RenderTargetIdentifier(cameraData.targetTexture)
                : BuiltinRenderTextureType.CameraTarget;


            if (m_BlitMaterial == null)
            {
                Debug.LogWarningFormat(
                    "Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.",
                    m_BlitMaterial, GetType().Name);
                return;
            }


            CommandBuffer cmd =
                CommandBufferPool.Get(m_ProfilerTag);
            if (!cameraData.isPreviewCamera)
            {
                cmd.Blit(null, m_LightResult.Identifier(), m_DepthLightMaterial);
#if UNITY_EDITOR
                if (m_Paramters.debugMode == DebugMode.none)
#endif
                    cmd.Blit(cameraTarget, m_LightResult.Identifier(), m_BlitMaterial);

                context.ExecuteCommandBuffer(cmd);
            }


            RenderTargetHandle currentSource = m_LightResult;

#if UNITY_EDITOR
            m_Paramters.enableBloom &=
                m_Paramters.debugMode == DebugMode.none;
#endif

            if (m_Paramters.enableBloom && !cameraData.isPreviewCamera)
            {
                if (m_BloomMaterial == null)
                {
                    m_BloomMaterial = new Material(Shader.Find("LIBII/Internal/Bloom"));
                    m_BloomMaterial.hideFlags = HideFlags.HideAndDontSave;
                }


                m_BloomMaterial.SetVector(Filter, m_Paramters.Filter);
                m_BloomMaterial.SetFloat(Intensity, m_Paramters.GammaToLinearSpaceIntensity);
                int width = w / 2;
                int height = h / 2;

                RenderTargetHandle currentDestination = textures[0] = new RenderTargetHandle();
                currentDestination.Init("_Temp");
                cmd.GetTemporaryRT(currentDestination.id, width, height, 0, FilterMode.Bilinear, format);

                cmd.Blit(currentSource.id,
                    currentDestination.id, m_BloomMaterial, BoxDownPrefilterPass);

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
                        cmd.GetTemporaryRT(currentDestination.id, width, height, 0, FilterMode.Bilinear, format);
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

                context.ExecuteCommandBuffer(cmd);
            }


            cmd.Clear();
            CoreUtils.SetRenderTarget(
                cmd,
                cameraTarget,
                RenderBufferLoadAction.Load,
                RenderBufferStoreAction.Store,
                ClearFlag.None,
                Color.black);


            if (!cameraData.isPreviewCamera)
            {
                cmd.SetGlobalTexture("_SourceTex", m_LightResult.id);
                cmd.Blit(currentSource.id, cameraTarget, m_FinalBlitMaterial);
            }

            context.ExecuteCommandBuffer(cmd);

            CommandBufferPool.Release(cmd);
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

            cmd.ReleaseTemporaryRT(m_LightResult.id);
        }
    }
}