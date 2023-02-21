Shader "LIBII/Internal/NativeCombineLight"
{
    Properties
    {
//        _SrcBlend("__src", Float) = 1.0
//        _DstBlend("__dst", Float) = 0.0
//        _GlobalColor("Global Light Color", COLOR) = (1,1,1,1)
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

            #include "HLSLSupport.cginc"

            
            UNITY_DECLARE_FRAMEBUFFER_INPUT_FLOAT(0); // color
            UNITY_DECLARE_FRAMEBUFFER_INPUT_FLOAT(1); // lightmap

            half4 _GlobalColor;
            half _LightIntensity;

            float4 vert(float4 vertexPosition : POSITION) : SV_POSITION
            {
                return vertexPosition;
            }


            float4 frag(float4 pos : SV_POSITION) : SV_Target
            {
                float4 color = UNITY_READ_FRAMEBUFFER_INPUT(0, pos);
                float4 depth = UNITY_READ_FRAMEBUFFER_INPUT(1, pos);

                half4 lightResult = 0;
                lightResult.rgb =  depth.rgb * _LightIntensity;

                lightResult.rgb += _GlobalColor.rgb * (1 - depth.a * max(depth.r, max(depth.g, depth.b)));

                lightResult.rgb *= color.rgb;

                return lightResult;
            }
            ENDHLSL
        }
    }
}