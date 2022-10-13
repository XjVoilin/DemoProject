Shader "LIBII/Internal/CombineLight"
{
    Properties
    {
        //        _SrcBlend("__src", Float) = 1.0
        //        _DstBlend("__dst", Float) = 0.0
        //        _GlobalColor("Global Light Color", COLOR) = (1,1,1,1)
        _MainTex("MainTex",2D) = "white"{}
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
        //        Blend One Zero
        Blend Zero SrcColor
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "HLSLSupport.cginc"


            half4 _GlobalColor;
            half _LightIntensity;
            sampler2D _MainTex;

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


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }


            float4 frag(v2f IN) : SV_Target
            {
                half4 depth = tex2D(_MainTex, IN.uv);
                half4 lightResult = 0;
                lightResult.rgb = depth.rgb * _LightIntensity;

                lightResult.rgb += _GlobalColor.rgb * (1 - depth.a * max(depth.r, max(depth.g, depth.b)));
                lightResult.rgb *= _GlobalColor.a;

                return lightResult;
            }
            ENDHLSL
        }
    }
}