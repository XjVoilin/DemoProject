using System.Collections.Generic;
using Custom2DLightRenderFeature;
using LIBII.Light2D;
using LIBII;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LIBII.Light2D
{
    public class Light2DRenderFeature : ScriptableRendererFeature
    {
        public static Light2DRenderFeature Instance;

        public Light2DSettings light2DSettings;

        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingTransparents;
        public int renderOrderOffset = -1;
        public Color globalLightColor = new Color(.7f,.7f,.7f,1);

        public string[] shaderTagIds = new string[]
        {
            "LibiiLightObject"
        };

        public LayerMask transparentLayerMask = -1;

        [SerializeField] StencilStateData
            defaultStencilState = new StencilStateData()
                {passOperation = StencilOp.Replace}; // This default state is compatible with deferred renderer.

        [SerializeField] StencilState stencilState; // This default state is compatible with deferred renderer.


        public bool enableBloom = false;
        [Range(1, 16)] public int iterations = 4;
        [Range(0, 10)] public float intensity = 4;
        [Range(0, 2)] public float threshold = 1;
        [Range(0, 1)] public float softThreshold = 0.5f;
        [Range(0, 1f)] public float lightDensity = 0.8f;

        [HideInInspector] public BlendMode srcOpt = BlendMode.Zero;
        [HideInInspector] public BlendMode dstOpt = BlendMode.SrcColor;
#if UNITY_EDITOR
        [HideInInspector] public DebugMode debugMode;
#endif
        private MarkLightAreaPass m_LightAreaPass;
        private RenderObjectPass m_RenderObjectPass;

        private PassParameters m_PassParameters;
        public override void Create()
        {
            Instance = this;
            m_LightAreaPass = new MarkLightAreaPass
            {
                renderPassEvent = renderEvent + renderOrderOffset
            };

            Light2DSettings.Instance = light2DSettings;

            StencilStateData stencilData = defaultStencilState;
            stencilState = StencilState.defaultValue;
            stencilState.enabled = stencilData.overrideStencilState;
            stencilState.SetCompareFunction(stencilData.stencilCompareFunction);
            stencilState.SetPassOperation(stencilData.passOperation);
            stencilState.SetFailOperation(stencilData.failOperation);
            stencilState.SetZFailOperation(stencilData.zFailOperation);

            m_RenderObjectPass = new RenderObjectPass(new PassParameters()
            {
                profilerTag = "RenderLightingObject",
                shaderTagIds = shaderTagIds,
                renderEvent = RenderPassEvent.BeforeRenderingTransparents - 10,
                renderQueueRange = RenderQueueRange.all,
                layerMask = transparentLayerMask,
                stencilState = stencilState,
                stencilReference = stencilData.stencilReference,
#if UNITY_EDITOR
                debugMode = debugMode,
#endif
                globalLightColor = globalLightColor,
                enableBloom = enableBloom, iterations = iterations,
                intensity = intensity,
                threshold = threshold,
                softThreshold = softThreshold,
                srcOpt = srcOpt,
                dstOpt = dstOpt,
                lightDensity = lightDensity
            });
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_RenderObjectPass != null)
            {
                renderer.EnqueuePass(m_RenderObjectPass);
            }

            if (m_RenderObjectPass!=null)
            {
                if (m_PassParameters==null)
                {
                    m_PassParameters = new PassParameters();
                }
                m_PassParameters.profilerTag = "RenderLightingObject";
                m_PassParameters.shaderTagIds = shaderTagIds;
                m_PassParameters.renderEvent = RenderPassEvent.BeforeRenderingTransparents - 10;
                m_PassParameters.renderQueueRange = RenderQueueRange.all;
                m_PassParameters.layerMask = transparentLayerMask;
                m_PassParameters.stencilState = stencilState;
#if UNITY_EDITOR
                m_PassParameters.debugMode = debugMode;
#endif
                m_PassParameters.globalLightColor = globalLightColor;
                m_PassParameters.enableBloom = enableBloom; 
                m_PassParameters.iterations = iterations;
                m_PassParameters.intensity = intensity;
                m_PassParameters.threshold = threshold;
                m_PassParameters.softThreshold = softThreshold;
                m_PassParameters.srcOpt = srcOpt;
                m_PassParameters.dstOpt = dstOpt;
                m_PassParameters.lightDensity = lightDensity;
                m_RenderObjectPass.SetUp(m_PassParameters);
            }
//             if (m_LightAreaPass != null)
//             {
//                 m_LightAreaPass.Setup(
//                     new PassParameters()
//                     {
// #if UNITY_EDITOR
//                         debugMode = debugMode,
// #endif
//                         globalLightColor = globalLightColor,
//                         enableBloom = enableBloom, iterations = iterations,
//                         intensity = intensity,
//                         threshold = threshold,
//                         softThreshold = softThreshold,
//                         srcOpt = srcOpt,
//                         dstOpt = dstOpt,
//                         lightDensity = lightDensity
//                     }
//                 );
//
//                 renderer.EnqueuePass(m_LightAreaPass);
//             }
        }
    }

    public class PassParameters
    {
        public string profilerTag;
        public string[] shaderTagIds;
        public RenderPassEvent renderEvent;
        public RenderQueueRange renderQueueRange;
        public LayerMask layerMask;
        public StencilState stencilState;
        public int stencilReference;
#if UNITY_EDITOR
        public DebugMode debugMode;
#endif

        public BlendMode srcOpt;
        public BlendMode dstOpt;

        public Color globalLightColor;
        public float lightDensity;
        public bool enableBloom;
        public int iterations;
        public float intensity;
        public float threshold;
        public float softThreshold;

        public float GammaToLinearSpaceIntensity
        {
            get { return Mathf.GammaToLinearSpace(intensity); }
        }

        private Vector4 _filter;

        public Vector4 Filter
        {
            get
            {
                _filter.x = threshold;
                _filter.y = _filter.x - Knee;
                _filter.z = 2f * Knee;
                _filter.w = 0.25f / (Knee + 0.00001f);
                return _filter;
            }
        }

        public float Knee
        {
            get { return threshold * softThreshold; }
        }
    }

#if UNITY_EDITOR
    public enum DebugMode
    {
        none,
        // depth,
        // light,
        // depthGap,
        // depthGapWithLight,
        // depthGapLightWithGlobal,
        // bloom
    }
#endif
}