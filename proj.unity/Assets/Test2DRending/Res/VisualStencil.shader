Shader "Unlit/VisualStencil"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle]IsR("IsR",int) = 1
        [Toggle]IsG("IsG",int) = 1
        [Toggle]IsB("IsB",int) = 1
        [Toggle]IsA("IsA",int) = 1
    }
    SubShader
    {
        Tags
        {
            "LightMode" = "LibiiLightObject" "Queue"="Transparent" "RenderType"="Transparent"
        }
        LOD 100

        Pass
        {

            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int IsR;
            int IsG;
            int IsB;
            int IsA;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            struct fout
            {
                half4 color : SV_Target;
                half4 depth : SV_Target1;
            };


            fout frag(v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float colR = col.r * IsR;
                float colG = col.g * IsG;
                float colB = col.b * IsB;
                float colA = col.a * IsA;
                col = float4(colR, colG, colB, colR);

                fout fo;
                fo.color = col;
                half4 finalColor = fo.color;
                finalColor.a = fo.color.a;
                fo.depth = finalColor;

                return fo;
            }
            ENDCG
        }
    }
}