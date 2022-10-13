Shader "LIBII/Internal/Light/PointLight_Mask"
{
    Properties
    {
        _MainTex ("Lightmap Texture", 2D) = "white" {}
        _Strength ("Strength", Float) = 0
        _Outer("Outer", Float) = 0
        _Inner("Inner", Float) = 0
        _Rotation("Rotation", Float) = 0
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Power ("Power", Range(1,10)) = 5

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

        //Blend 0 SrcAlpha OneMinusSrcAlpha
        //Blend 1 SrcAlpha OneMinusSrcAlpha
        Blend SrcAlpha OneMinusSrcAlpha

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

                sampler2D _MainTex;

                float _Strength;
                float _Outer;
                float _Inner;
                float _Rotation;
                float4 _Color;
                float _Power;

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float4 texcoord : TEXCOORD0;
                    float3 xy : TEXCOORD1;
                };

                v2f vert(appdata_t v)
                {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = v.color;
                    o.texcoord = UNITY_PROJ_COORD(float4(v.texcoord.x, v.texcoord.y, 1, 1));

                    o.xy.xy = float2(v.texcoord.x - 0.5, v.texcoord.y - 0.5);
                    o.xy.z = _Inner >= 359;

                    return o;
                }

                struct fout
                {
                    half4 color : SV_Target;
                    half4 depth : SV_Target1;
                };

                fout frag(v2f i)
                {
                    float distance = sqrt(i.xy.x * i.xy.x + i.xy.y * i.xy.y);

                    float dir = ((atan2(i.xy.y, i.xy.x) - _Rotation) * 57.2958 + 810) % 360;

                    float pointValue = max(0, (0.5 - pow(distance + pow(.5, 1 / _Power) - .5, _Power))) * 2;


                    pointValue *= lerp(max(0, min(1, (_Inner * 0.5 - abs(dir - 180) + _Outer) / _Outer)), 1, i.xy.z);

                    fixed4 output = half4(1, 1, 1, 1);

                    output.rgb = 1;

                    float decay = lerp(pointValue, pointValue * pointValue * pointValue, _Strength);
                    // output.rgb *= decay;

                    output.rgb *= i.color * _Color.rgb;
                    output.a *= i.color.a * _Color.a * decay;
                    output.rgb *= output.a;
                    fout fo;

                    fo.color = 0; //float4(1,0,0,output.a);
                    // fo.depth = half4(_LightFlag , _LightDensity , 1, mainTex.a +_LightDensity*_LightFlag);
                    fo.depth = output;
                    return fo;
                }
                ENDCG
            }
        }
    }
}