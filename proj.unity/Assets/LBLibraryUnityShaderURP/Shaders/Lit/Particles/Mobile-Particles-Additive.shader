// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Additive Particle shader. Differences from regular Additive Particle one:
// - no Tint color
// - no Smooth particle support
// - no AlphaTest
// - no ColorMask

Shader "LIBII/Mobile/Particles/Additive"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
        [Toggle(IS_LIGHT)] _LightFlag("Light Flag", Int) = 0
        _LightDensity("Light Density", Range(0,1)) = .8
    }

    Category
    {
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"
        }
        //Blend 0 SrcAlpha One
        //Blend 1 SrcAlpha One, OneMinusSrcAlpha SrcAlpha
        Blend SrcAlpha One
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Off

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

                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 uv : TEXCOORD0;
                };

                sampler2D _MainTex;
                float _LightFlag;
                float _LightDensity;

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = v.color;
                    o.uv = v.uv;
                    return o;
                }

                struct fout
                {
                    half4 color : SV_Target;
                    half4 depth : SV_Target1;
                };

                fout frag(v2f i)
                {
                    half4 main = tex2D(_MainTex, i.uv);
                    main *= i.color;

                    fout fo;
                    fo.color = main;
                    // fo.depth = half4(_LightFlag , _LightDensity , 1, mainTex.a +_LightDensity*_LightFlag);
                    main.rgb *= _LightDensity;
                    fo.depth = main;
                    return fo;
                }
                ENDCG

            }
        }
    }
}