Shader "LIBII/Internal/BlitColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

        //        Blend SrcColor SrcColor
        //        Blend One SrcColor
        //        Blend Zero SrcColor
        Blend [_SrcBlend][_DstBlend]
        //        Blend One Zero
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

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Intensity;
            sampler2D _SourceTex;

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/CopyDepthPass.hlsl"


            half4 Sample(float2 uv)
            {
                return tex2D(_MainTex, uv);
            }

            half4 SampleBox(float2 uv, float delta)
            {
                float4 o = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
                half4 s =
                    Sample(uv + o.xy) + Sample(uv + o.zy) +
                    Sample(uv + o.xw) + Sample(uv + o.zw);
                return s * 0.25f;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }


            float4 frag(v2f i) : SV_Target
            {
                float4 c = _Intensity * SampleBox(i.uv, 0.5);
                return c;
            }
            ENDHLSL
        }
    }
}