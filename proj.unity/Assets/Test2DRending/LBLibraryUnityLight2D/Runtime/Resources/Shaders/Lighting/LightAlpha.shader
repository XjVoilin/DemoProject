Shader "LIBII/Internal/MeshModeAlpha"
{
    Properties
    {
        _Sprite ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Invert("Invert", Float) = 0
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

        Blend One OneMinusSrcAlpha

        Cull Off Lighting Off ZWrite Off

        Pass
        {
            Tags
            {
                "LightMode" = "LibiiLightObject" "Queue"="Transparent" "RenderType"="Transparent"
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _Sprite;
            float4 _Color;
            float _Invert;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = _Color;
                return o;
            }

            struct fout
            {
                half4 color : SV_Target;
                half4 depth : SV_Target1;
            };

            fout frag(v2f i)
            {
                float4 sprite = tex2D(_Sprite, i.uv);;

                float4 color = float4(1, 1, 1, 1);

                color.rgb *= sprite.rgb * sprite.a * i.color.a * i.color.rgb;

                color.a = sprite.a * sprite.r * i.color.a * i.color.rgb;

                fout fo;
                fo.color = color;
                // fo.depth = half4(_LightFlag , _LightDensity , 1, mainTex.a +_LightDensity*_LightFlag);
                fo.depth = color;
                return fo;
            }
            ENDCG
        }
    }
}