#ifndef COLOR_SHADER_EFFECT
#define COLOR_SHADER_EFFECT

#define  Epsilon  1e-10


float3 rgb2hcv(in float3 RGB)
{
    // Based on work by Sam Hocevar and Emil Persson
    float4 P = lerp(float4(RGB.bg, -1.0, 2.0 / 3.0), float4(RGB.gb, 0.0, -1.0 / 3.0), step(RGB.b, RGB.g));
    float4 Q = lerp(float4(P.xyw, RGB.r), float4(RGB.r, P.yzx), step(P.x, RGB.r));
    float C = Q.x - min(Q.w, Q.y);
    float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
    return float3(H, C, Q.x);
}

float3 rgb2hsl(in float3 RGB)
{
    float3 HCV = rgb2hcv(RGB);
    float L = HCV.z - HCV.y * 0.5;
    float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
    return float3(HCV.x, S, L);
}

float3 hsl2rgb(float3 c)
{
    c = float3(frac(c.x), clamp(c.yz, 0.0, 1.0));
    float3 rgb = clamp(abs(fmod(c.x * 6.0 + float3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    return c.z + c.y * (rgb - 0.5) * (1.0 - abs(2.0 * c.z - 1.0));
}

inline float4 adjustColorHorizontal(float4 color, float2 uv)
{
    float3 hsl = rgb2hsl(color.rgb);
    float3 adjust = float3(_Hue / 360, _Saturation, _Brightness);

    hsl.x = _Hue / 360 + ((uv.x) * (_GradientA - _GradientB) + _GradientB) / 360;

    hsl.y *= adjust.y;
    hsl.z += adjust.z;
    hsl.z *= _Boost;

    color.rgb = hsl2rgb(hsl);
    return color;
}

inline float4 adjustColorRotate(float4 color, float2 uv)
{
    float3 hsl = rgb2hsl(color.rgb);
    float3 adjust = float3(_Hue / 360, _Saturation, _Brightness);


    float2 dir = uv - float2(0.5, .65);
    float r = length(float2(dir.x, dir.y));
    float angle = atan(dir.y / dir.x) - sin(_Time.y) * r + 1.0 * _Time.y;
    //float angle = atan(dir.y / dir.x) +  sin(r * UNITY_PI * 40) * 0.15;
    hsl.x = _Hue / 360 + (angle * (_GradientA - _GradientB) + _GradientB) / 360;

    hsl.y *= adjust.y;
    hsl.z += adjust.z;
    hsl.z *= _Boost;

    color.rgb = hsl2rgb(hsl);
    return color;
}

inline float4 adjustColorVertical(float4 color, float2 uv)
{
    float3 hsl = rgb2hsl(color.rgb);
    float3 adjust = float3(_Hue / 360, _Saturation, _Brightness);

    hsl.x = adjust.x + ((uv.y) * (_GradientA - _GradientB) + _GradientB) / 360;

    hsl.y *= adjust.y;
    hsl.z += adjust.z;
    hsl.z *= _Boost;

    color.rgb = hsl2rgb(hsl);
    return color;
}


inline float4 adjustColorLT2RB(float4 color, float2 uv)
{
    float3 hsl = rgb2hsl(color.rgb);
    float3 adjust = float3(_Hue / 360, _Saturation, _Brightness);

    hsl.x = _Hue / 360 + ((uv.x - uv.y) * (_GradientA - _GradientB) + _GradientB) / 360;

    hsl.y *= adjust.y;
    hsl.z += adjust.z;
    hsl.z *= _Boost;

    color.rgb = hsl2rgb(hsl);
    return color;
}

inline float4 adjustColorLB2RT(float4 color, float2 uv)
{
    float3 hsl = rgb2hsl(color.rgb);
    float3 adjust = float3(_Hue / 360, _Saturation, _Brightness);

    hsl.x = _Hue / 360 + ((uv.x + uv.y) * (_GradientA - _GradientB) + _GradientB) / 360;

    hsl.y *= adjust.y;
    hsl.z += adjust.z;
    hsl.z *= _Boost;

    color.rgb = hsl2rgb(hsl);
    return color;
}

//inline float4 adjustColorDistortion(float4 color, float2 uv)
//{
//    float3 hsl = rgb2hsl(color.rgb);
//    float3 adjust = float3(_Hue / 360, _Saturation, _Brightness);

//    hsl.x = _Hue / 360 + ((sin(uv.y * UNITY_PI - uv.x) + uv.x) * (_GradientA - _GradientB) + _GradientB) /360;
//    hsl.y *= adjust.y;
//    hsl.z += adjust.z;
//    hsl.z *= _Boost;

//    color.rgb = hsl2rgb(hsl);
//    return color;
//}


inline float4 adjustColorCenterWave(float4 color, float2 uv)
{
    float3 hsl = rgb2hsl(color.rgb);
    float3 adjust = float3(_Hue / 360, _Saturation, _Brightness);
    hsl.x = _Hue / 360 + (distance(uv, 0.5) * (_GradientA - _GradientB) + _GradientB) / 360;

    hsl.y *= adjust.y;
    hsl.z += adjust.z;
    hsl.z *= _Boost;

    color.rgb = hsl2rgb(hsl);
    return color;
}
#endif
