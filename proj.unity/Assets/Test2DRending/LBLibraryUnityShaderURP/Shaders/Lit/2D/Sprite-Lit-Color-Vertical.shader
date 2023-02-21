Shader "LIBII/URP/Lit/2D/Sprite-Lit-Color-Vertical"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}

        _HSVRange("HSV Affect Range",Range(0,1.01)) = 0.45

        _Boost ("Lighting Up Ratio", Range(-3,3)) = 0.15
        _Hue("Hue", Range(-360,360)) = 0.0
        _Saturation("Saturation", Range(-2,2)) = 1.0
        _Brightness("Brightness", Range(-2,2)) = 1.0
        _GradientA("Hue Graident A",Range(0,360)) = 0.0
        _GradientB("Hue Graident B",Range(0,360)) = 0.0
        [HideInInspector]_UV2("UV2",vector) = (0,0,0,0)
        [HideInInspector]_UV22("UV22",vector) = (0,0,0,0)
        _Offset("Offset",float) = 0.0
        _Width("Width",Range(-5,5)) = 1.0
        _Slope("Slope",Range(-5,5)) = 1.0
        [Toggle(IS_LIGHT)] _LightFlag("Light Flag", Int) = 0
        _LightDensity("Light Density", Range(0,1)) = .8

        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2.0

        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0

        [HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default
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

            float4 _ClipRect;
            float _Boost, _GradientA, _GradientB;
            uniform float _Hue;
            uniform float _Saturation;
            uniform float _Brightness;
            uniform float4 _UV2;
            float Epsilon = 1e-10;

            float _HSVRange;
            float4 _UV22;
            float _Offset, _Width, _Slope;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 uv8 : TEXCOORD7;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 uv8 : TEXCOORD7;

                half2 lightingUV : TEXCOORD1;
                #if defined(DEBUG_DISPLAY)
                float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
            #include "./ColorShaderEffect.cginc"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            half4 _MainTex_ST;
            float _LightFlag;
            float _LightDensity;
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
                o.uv8 = v.uv8;
                o.color = v.color;
                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"
            #include "../../../ShaderLibrary/Core/LibiiCore.hlsl"

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
                const half4 main = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(main.rgb, main.a, mask, surfaceData);
                InitializeInputData(i.uv, i.lightingUV, inputData);
                //float2 scale = float2(unity_ObjectToWorld[0][0], unity_ObjectToWorld[1][1]);
                //float adjustedAngle = atan(tan(_ColorAngle * DEGREE_2_RADIAN) * scale.x / scale.y);

                //float4 mixed = sampleGradientColor(_C1, _C2, _C3, _C4, _C5, _C6,
                //                                         _P1, _P2, _P3, _P4, _P5, _P6, _ColorCount,
                //                                         rotateUV(i.uv, adjustedAngle).y);
                //surfaceData.albedo = mixed.rgb * mixed.a + (1 - mixed.a) * surfaceData.albedo;
                float4 mixed;
                float3 hsv = clamp((rgb2hsl(main.rgb) - 0.5) * 2, 0, 1);
                hsv.z += _Boost;
                float3 grey = hsv.xyz;
                mixed.rgb = grey;
                mixed.a = main.a;
                // mixed.rgb = adjustColorVertical(mixed.rgba, i.uv * float2(_UV2.z+_UV22.z,_UV2.w+_UV22.w) + _UV2.xy+_UV22.xy).rgb;
                mixed.rgb = adjustColorVertical(mixed.rgba, i.uv8 * float2(_Slope, _Width) + float2(0, _Offset)).rgb;

                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float3 hsvOri = rgb2hsv(color.rgb);

                //surfaceData.albedo = step(_HSVRange,hsvOri.z) * (mixed.rgb * mixed.a + (1 - mixed.a) * surfaceData.albedo) + (1 - step(_HSVRange,hsvOri.z)) * main.rgb * main.a;
                surfaceData.albedo = step(_HSVRange, hsvOri.z) * mixed.rgb + (1 - step(_HSVRange, hsvOri.z)) * main.rgb;

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
                float2 uv8 : TEXCOORD7;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 uv8 : TEXCOORD7;
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

            float4 _ClipRect;
            float _Boost, _GradientA, _GradientB;
            uniform float _Hue;
            uniform float _Saturation;
            uniform float _Brightness;
            uniform float4 _UV2;
            float Epsilon = 1e-10;

            float _HSVRange;
            float4 _UV22;
            float _Offset, _Width, _Slope;

            #include "../../../ShaderLibrary/Core/LibiiCore.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
            #include "./ColorShaderEffect.cginc"

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
                o.uv8 = attributes.uv8;
                return o;
            }

            struct fout
            {
                half4 color : SV_Target;
                half4 depth : SV_Target1;
            };

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


                half4 mixed;
                float3 hsv = clamp((rgb2hsl(mainTex.rgb) - 0.5) * 2, 0, 1);
                hsv.z += _Boost;
                float3 grey = hsv.xyz;
                mixed.rgb = grey;
                mixed.a = mainTex.a;
                // mixed.rgb = adjustColorVertical(mixed.rgba, i.uv * float2(_UV2.z+_UV22.z,_UV2.w+_UV22.w) + _UV2.xy+_UV22.xy).rgb;
                mixed.rgb = adjustColorVertical(mixed.rgba, i.uv8 * float2(_Slope, _Width) + float2(0, _Offset)).rgb;

                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float3 hsvOri = rgb2hsv(color.rgb);

                //surfaceData.albedo = step(_HSVRange,hsvOri.z) * (mixed.rgb * mixed.a + (1 - mixed.a) * surfaceData.albedo) + (1 - step(_HSVRange,hsvOri.z)) * main.rgb * main.a;
                half3 albedo = step(_HSVRange, hsvOri.z) * mixed.rgb + (1 - step(_HSVRange, hsvOri.z)) * mainTex.rgb;


                fout fo;
                fo.color = float4(albedo, mainTex.a);
                // fo.depth = half4(_LightFlag , _LightDensity , 1, mainTex.a +_LightDensity*_LightFlag);
                half4 finalColor = fo.color * _LightDensity * _LightFlag;
                finalColor.a = fo.color.a;
                fo.depth = finalColor;
                return fo;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}