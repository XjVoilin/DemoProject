#ifndef UNIVERSAL_PIPELINE_LIBII_CORE_INCLUDED
#define UNIVERSAL_PIPELINE_LIBII_CORE_INCLUDED

#define UNITY_PI            3.14159265359f
#define UNITY_TWO_PI        6.28318530718f
#define UNITY_FOUR_PI       12.56637061436f
#define UNITY_INV_PI        0.31830988618f
#define DEGREE_2_RADIAN     0.01745329252
#define ONE_DIVIDED_360     0.0027777777778
#define Epsilon             1e-10

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

inline float2 rotateUV(half2 uv, float rotation)
{
    float sinX = sin(rotation);
    float cosX = cos(rotation);
    float2x2 rotationMatrix = float2x2(cosX, -sinX, sinX, cosX);
    return mul(uv - half2(0.5, 0.5), rotationMatrix) + half2(0.5, 0.5);
}


float calculate(float t, float p, float pstart, float pend)
{
    float r = step(0, t - p);
    float end = 1 / (pend - p + step(0, p - pend));
    float start = 1 / (p - pstart + step(0, pstart - p));
    return clamp(0, 1, r * step(0, pend - t) * (1 - (t - p) * end) +
                 (1 - r) * step(0, t - pstart) * (1 - (p - t) * start));
}

inline half4 sampleGradientColor(half4 c1, half4 c2, half4 c3, half4 c4, half4 c5, half4 c6,
                                 float p1, float p2, float p3, float p4, float p5, float p6,
                                 int count, float t)
{
    half4 result = calculate(t, p1, 0, p2) * c1 +
        calculate(t, p2, p1, p3) * c2 * step(2, count) +
        calculate(t, p3, p2, p4) * c3 * step(3, count) +
        calculate(t, p4, p3, p5) * c4 * step(4, count) +
        calculate(t, p5, p4, p6) * c5 * step(5, count) +
        calculate(t, p6, p5, 1) * c6 * step(6, count);

    return result;
}


float random(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
}

float4 hash4(float2 p)
{
    return frac(sin(float4(1.0 + dot(p, float2(37.0, 17.0)),
                           2.0 + dot(p, float2(11.0, 47.0)),
                           3.0 + dot(p, float2(41.0, 29.0)),
                           4.0 + dot(p, float2(23.0, 31.0)))) * 103.0);
}

///  3 out, 3 in...
// from https://www.shadertoy.com/view/4djSRW
#define HASHSCALE3 float3(443.897, 441.423, 437.195)

float3 hash33(float3 p3)
{
    p3 = frac(p3 * HASHSCALE3);
    p3 += dot(p3, p3.yxz + 19.19);
    return frac((p3.xxy + p3.yxx) * p3.zyx);
}

half4 SampleTextureNoRepeatTech1(float2 uv, TEXTURE2D_PARAM(repeatMap, mapSampler)
)
{
    float blendRatio = 0.49f;
    float2 iuv = floor(uv);
    float2 fuv = frac(uv);

    float4 ofa = hash4(iuv + float2(0, 0));
    float4 ofb = hash4(iuv + float2(1, 0));
    float4 ofc = hash4(iuv + float2(0, 1));
    float4 ofd = hash4(iuv + float2(1, 1));

    // Compute the correct derivatives
    float2 dx = ddx(uv);
    float2 dy = ddy(uv);

    // Mirror per-tile uvs
    ofa.zw = sign(ofa.zw - 0.5);
    ofb.zw = sign(ofb.zw - 0.5);
    ofc.zw = sign(ofc.zw - 0.5);
    ofd.zw = sign(ofd.zw - 0.5);

    float2 uva = uv * ofa.zw + ofa.xy, dxa = dx * ofa.zw, dya = dy * ofa.zw;
    float2 uvb = uv * ofb.zw + ofb.xy, dxb = dx * ofb.zw, dyb = dy * ofb.zw;
    float2 uvc = uv * ofc.zw + ofc.xy, dxc = dx * ofc.zw, dyc = dy * ofc.zw;
    float2 uvd = uv * ofd.zw + ofd.xy, dxd = dx * ofd.zw, dyd = dy * ofd.zw;

    // Fetch and blend
    float2 b = smoothstep(blendRatio, 1.0 - blendRatio, fuv);

    return lerp(
        lerp(SAMPLE_TEXTURE2D_GRAD(repeatMap, mapSampler, uva, dxa, dya),
             SAMPLE_TEXTURE2D_GRAD(repeatMap, mapSampler, uvb, dxb, dyb), b.x),
        lerp(SAMPLE_TEXTURE2D_GRAD(repeatMap, mapSampler, uvc, dxc, dyc),
             SAMPLE_TEXTURE2D_GRAD(repeatMap, mapSampler, uvd, dxd, dyd), b.x), b.y);
}

// https://zhuanlan.zhihu.com/p/432090394
// TODO: 出图不能有明显边缘
half4 SampleTextureNoRepeatTech2(float2 uv, TEXTURE2D_PARAM(repeatMap, mapSampler)
)
{
    float blendRatio = 0.49;
    float2 iuv = floor(uv);
    float2 fuv = frac(uv);

    float4 ofa = hash4(iuv + float2(0, 0));
    float4 ofb = hash4(iuv + float2(1, 0));
    float4 ofc = hash4(iuv + float2(0, 1));
    float4 ofd = hash4(iuv + float2(1, 1));

    // Compute the correct derivatives
    float2 dx = ddx(uv);
    float2 dy = ddy(uv);

    // Mirror per-tile uvs
    ofa.zw = sign(ofa.zw - 0.5);
    ofb.zw = sign(ofb.zw - 0.5);
    ofc.zw = sign(ofc.zw - 0.5);
    ofd.zw = sign(ofd.zw - 0.5);

    // float DITHER_THRESHOLDS[16] =
    // {
    //     1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
    //     13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
    //     4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
    //     16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    // };
    float2 b = smoothstep(blendRatio, 1.0 - blendRatio, fuv);
    b = saturate(sign(b - 0.5));
    // uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    // b =  saturate(sign(b -  DITHER_THRESHOLDS[index]));
    float4 ofDither = lerp(lerp(ofa, ofb, b.x), lerp(ofc, ofd, b.x), b.y);
    // return ofDither;

    uv = uv * ofDither.zw + ofDither.xy;

    return SAMPLE_TEXTURE2D_GRAD(repeatMap, mapSampler, uv, dx * ofDither.zw, dy * ofDither.zw);
}


#endif
