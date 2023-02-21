Shader "LIBII/URP/Lit/2D/Sprite-Lit-HSLpure2"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}

        //_ColorAngle ("Color angle", float) = 45
        _ResultColor ("SkinColor",color) = (1,1,1,0)
        _MixColor("MixColor",color) = (1,1,1,1)
        _HSVRangeMax("HSV Affect RangeMax",Range(0,1)) = 1
        _HSVRangeMin("HSV Affect RangeMin",Range(0,1)) = 0.45

        _OffsetY("OffsetY",Range(0 , 1)) = 1

        //_Boost ("Lighting Up Ratio", Range(-3,3)) = 0.15
        //_Hue("Hue", Range(-360,360)) = 0.0
        _SaturationParm("SaturationParm",Range(0,30)) = 0
        //HideInInspector]_Saturation("Saturation", Range(-1,1)) = 1.0
        //_Brightness("Brightness", Range(-2,2)) = 1.0
        //_GradientA("Hue Graident A",Range(0,360)) = 0.0
        //_GradientB("Hue Graident B",Range(0,360)) = 0.0
        //_AlphaA("Alpha A",Range(0,1)) = 0.0
        //_AlphaB("Alpha B",Range(0,1)) = 0.0
        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.

        [Toggle(IS_LIGHT)] _LightFlag("Light Flag", Int) = 0
        _LightDensity("Light Density", Range(0,1)) = .8
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2.0

        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("LightColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0

        _StencilRef("Stencil Reference", Float) = 1.0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline"
        }

        //Blend 0 SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        //Blend 1 SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
        Blend SrcAlpha OneMinusSrcAlpha

        Cull[_Cull]
        ZWrite Off
        ZTest Off

        Stencil
        {
            Ref[_StencilRef]
            Comp[_StencilComp]
            Pass Keep
        }

        Pass
        {
            Tags
            {
                "LightMode" = "Universal2D"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment

            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_0 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_2 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_3 __
            #pragma multi_compile _ DEBUG_DISPLAY

            float _ColorAngle;
            float _Boost;
            float _GradientA;
            float _GradientB;
            float _Hue;
            float _SaturationParm;
            float _Saturation;
            float _Brightness;
            float _AlphaA, _AlphaB;
            float4 _ResultColor;
            float4 _MixColor;
            float _HSVRangeMax;
            float _HSVRangeMin;
            float _OffsetY;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                half2 lightingUV : TEXCOORD1;
                #if defined(DEBUG_DISPLAY)
                float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            half4 _MainTex_ST;

            #if USE_SHAPE_LIGHT_TYPE_0
            SHAPE_LIGHT(0)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_1
            SHAPE_LIGHT(1)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_2
            SHAPE_LIGHT(2)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_3
            SHAPE_LIGHT(3)
            #endif

            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(v.positionOS);
                #if defined(DEBUG_DISPLAY)
                o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);

                o.color = v.color;
                return o;
            }

            #include "../../../ShaderLibrary/Core/LibiiHSL.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            float3 hsv2rgb(float3 c)
            {
                c = float3(c.x, clamp(c.yz, 0.0, 1.0));
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float3 hsv = rgb2hsv(color.rgb);

                const half4 main = i.color * color;
                const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(main.rgb, main.a, mask, surfaceData);
                InitializeInputData(i.uv, i.lightingUV, inputData);

                //MixInfo info;
                //info.boost = _Boost;
                //info.hue = _Hue;
                //info.lighting = _Brightness;
                //info.saturation = _Saturation;
                //info.pointA = _GradientA;
                //info.pointB = _GradientB;
                //info.alphaA = _AlphaA;
                //info.alphaB = _AlphaB;

                float inRange = step(hsv.z, _HSVRangeMax) * step(_HSVRangeMin, hsv.z);

                //surfaceData.albedo = inRange * (mixed.rgb * mixed.a + (1 - mixed.a) * surfaceData.albedo) + (1 - inRange) * main.rgb * main.a;
                surfaceData.albedo = inRange * (_ResultColor.rgb * _ResultColor.a + (1 - _ResultColor.a) * surfaceData.
                    albedo) + (1 - inRange) * main.rgb * main.a;

                float lp = clamp((_OffsetY / (i.uv.y)), 0, 1);

                if (_SaturationParm != 0)
                {
                    float3 hsv1 = rgb2hsv(surfaceData.albedo);
                    //hsv1.y = _SaturationParm * (sin(hsv1.y - UNITY_PI / 2) + 1);
                    surfaceData.albedo = hsv2rgb(hsv1);
                }

                surfaceData.albedo = inRange * step(_OffsetY, i.uv.y) * lerp(_MixColor.rgb, surfaceData.albedo, lp) + (1
                    - step(_OffsetY, i.uv.y)) * surfaceData.albedo;

                return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }

        Pass
        {
            Tags
            {
                "LightMode" = "NormalsRendering"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half3 tangentWS : TEXCOORD2;
                half3 bitangentWS : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            half4 _NormalMap_ST; // Is this the right way to do this?

            Varyings NormalsRenderingVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                o.uv = TRANSFORM_TEX(attributes.uv, _NormalMap);
                o.color = attributes.color;
                o.normalWS = -GetViewForwardDir();
                o.tangentWS = TransformObjectToWorldDir(attributes.tangent.xyz);
                o.bitangentWS = cross(o.normalWS, o.tangentWS) * attributes.tangent.w;
                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            half4 NormalsRenderingFragment(Varyings i) : SV_Target
            {
                const half4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                const half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));

                return NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
            }
            ENDHLSL
        }

        Pass
        {
            Tags
            {
                "LightMode" = "LibiiLightObject" "Queue"="Transparent" "RenderType"="Transparent"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment


            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                #if defined(DEBUG_DISPLAY)
                float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float _LightFlag;
            float _LightDensity;

            float _ColorAngle;
            float _Boost;
            float _GradientA;
            float _GradientB;
            float _Hue;
            float _SaturationParm;
            float _Saturation;
            float _Brightness;
            float _AlphaA, _AlphaB;
            float4 _ResultColor;
            float4 _MixColor;
            float _HSVRangeMax;
            float _HSVRangeMin;
            float _OffsetY;

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                #if defined(DEBUG_DISPLAY)
                o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = TRANSFORM_TEX(attributes.uv, _MainTex);
                o.color = attributes.color;
                return o;
            }

            struct fout
            {
                half4 color : SV_Target;
                half4 depth : SV_Target1;
            };

            #include "../../../ShaderLibrary/Core/LibiiHSL.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            float3 hsv2rgb(float3 c)
            {
                c = float3(c.x, clamp(c.yz, 0.0, 1.0));
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }

            fout UnlitFragment(Varyings i)
            {
                float4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                if (mainTex.a == 0)
                {
                    discard;
                }
                #if defined(DEBUG_DISPLAY)
                SurfaceData2D surfaceData;
                InputData2D inputData;
                half4 debugColor = 0;

                InitializeSurfaceData(mainTex.rgb, mainTex.a, surfaceData);
                InitializeInputData(i.uv, inputData);
                SETUP_DEBUG_DATA_2D(inputData, i.positionWS);

                if(CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
                {
                    return debugColor;
                }
                #endif

                float3 hsv = rgb2hsv(mainTex.rgb);
                float inRange = step(hsv.z, _HSVRangeMax) * step(_HSVRangeMin, hsv.z);

                //surfaceData.albedo = inRange * (mixed.rgb * mixed.a + (1 - mixed.a) * surfaceData.albedo) + (1 - inRange) * main.rgb * main.a;
                mainTex.rgb = inRange * (_ResultColor.rgb * _ResultColor.a + (1 - _ResultColor.a) * mainTex.rgb) + (1 -
                    inRange) * mainTex.rgb * mainTex.a;

                float lp = clamp((_OffsetY / (i.uv.y)), 0, 1);

                if (_SaturationParm != 0)
                {
                    float3 hsv1 = rgb2hsv(mainTex.rgb);
                    //hsv1.y = _SaturationParm * (sin(hsv1.y - UNITY_PI / 2) + 1);
                    mainTex.rgb = hsv2rgb(hsv1);
                }

                mainTex.rgb = inRange * step(_OffsetY, i.uv.y) * lerp(_MixColor.rgb, mainTex.rgb, lp) + (1
                    - step(_OffsetY, i.uv.y)) * mainTex.rgb;

                fout fo;
                fo.color = mainTex;
                // fo.depth = half4(_LightFlag , _LightDensity , 1, mainTex.a +_LightDensity*_LightFlag);
                mainTex.rgb *= _LightDensity * _LightFlag;
                fo.depth = mainTex;
                return fo;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}