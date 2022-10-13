Shader "LIBII/Internal/NativeFinalBlit"
{
    Properties
    {
        _SrcBlend("__src", Float) = 1.0
        _DstBlend("__dst", Float) = 0.0
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
        //        Blend [_SrcBlend][_DstBlend]
        Blend One Zero
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


            float _Intensity;

            #include "HLSLSupport.cginc"
            UNITY_DECLARE_FRAMEBUFFER_INPUT_FLOAT(0);


            float4 vert(float4 vertexPosition : POSITION) : SV_POSITION
            {
                return vertexPosition;
            }


            float4 frag(float4 pos : SV_POSITION) : SV_Target
            {
                float4 c = _Intensity * UNITY_READ_FRAMEBUFFER_INPUT(0, pos);
                return c;
            }
            ENDHLSL
        }
    }
}