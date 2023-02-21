Shader "LIBII/Internal/MeshModeAdditive"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Invert("Invert", Float) = 0
        _Point ("Free Form Point", Float) = 0
        _Strength ("Strength", Float) = 0
    }

    Category
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha One
        Cull Off
        Lighting Off
        ZWrite Off

        SubShader
        {
            Pass
            {
                Tags
                {
                    "LightMode" = "LibiiLightObject" "Queue"="Transparent" "RenderType"="Transparent"
                }
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                float4 _Color;
                float _Invert;
                float _Strength;
                float _Point;

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                v2f vert(appdata_t v)
                {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = _Color;
                    o.texcoord = v.texcoord;

                    return o;
                }

                struct fout
                {
                    half4 color : SV_Target;
                    half4 depth : SV_Target1;
                };

                fout frag(v2f i)
                {
                    float4 sprite = tex2D(_MainTex, i.texcoord);;

                    float4 color = float4(1, 1, 1, 1);
                    float2 xy = i.texcoord - 0.5;
                    float distance = lerp(1, 1 - sqrt(xy.x * xy.x + xy.y * xy.y) * 2, _Point);

                    color.rgb *= sprite.rgb * i.color.rgb;
                    color.a *= sprite.a * i.color.a * distance * (1 - _Strength);
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
}