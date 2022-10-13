Shader "LIBII/Internal/DepthLightBlend"
{
    Properties
    {
        _SrcBlend("__src", Float) = 1.0
        _DstBlend("__dst", Float) = 0.0
        _GlobalColor("Global Light Color", COLOR) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        //        Blend [_SrcBlend][_DstBlend]
        Blend One Zero
        //        Blend Zero SrcColor
        Cull Back
        Lighting Off
        ZWrite Off
        ZTest Off
        ZClip False

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ DEBUG_DEPTH DEBUG_LIGHT DEBUG_DEPTH_GAP DEBUG_GAP_LIGHT DEBUG_GAP_LIGHT_GLOBAL

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/CopyDepthPass.hlsl"


            sampler2D _LightDepthTexture;
            half4 _GlobalColor;
            half _Intensity;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }


            float4 frag(v2f i) : SV_Target
            {
                // return tex2D(_MainTex, i.uv) ;
                float4 depth = tex2D(_LightDepthTexture, i.uv);
                float3 lightDensity = depth.rgb;

          

                half4 lightResult = 0;
                lightResult.rgb = lightDensity * _Intensity;
              
                lightResult.rgb += _GlobalColor.rgb * (1 - depth.a * max(depth.r,max(depth.g,depth.b)));
                return lightResult;
            }
            ENDHLSL
        }
    }
}