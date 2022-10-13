#ifndef UNIVERSAL_PIPELINE_LIBII_GLITTER_INCLUDED
#define UNIVERSAL_PIPELINE_LIBII_GLITTER_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "../../ShaderLibrary/Core/LibiiCore.hlsl"

struct GlitterInfo
{
    float glitteryDotScale;
    float glitterySpeed;
    float glitterPower;
    float glitterContrast;
    float3 glitterColor;
    float maskAdjust;

    float specularPower;
    float specularContrast;

    float flowSpeed;
};


half4 SampleGlitterMap(float2 uv, TEXTURE2D_PARAM(glitterMap, sampler_glitterMap))
{
    return SAMPLE_TEXTURE2D(glitterMap, sampler_glitterMap, uv);
}

half4 SampleMaskMap(float2 uv, TEXTURE2D_PARAM(maskMap, sampler_maskMap))
{
    return SAMPLE_TEXTURE2D(maskMap, sampler_maskMap, uv);
}

half4 SampleSpecularGlitterMap(float2 uv, TEXTURE2D_PARAM(specularGlitterMap, sampler_specularGlitterMap))
{
    return SAMPLE_TEXTURE2D(specularGlitterMap, sampler_specularGlitterMap, uv);
}

half4 SampleGlitterMap_LOD(float2 uv, float lod, TEXTURE2D_PARAM(glitterMap, sampler_glitterMap))
{
    return SAMPLE_TEXTURE2D_LOD(glitterMap, sampler_glitterMap, uv, lod);
}

float mipmapLevel(float2 uv, float2 textureSize)
{
    float dx = ddx(uv.x * textureSize.x);
    float dy = ddy(uv.y * textureSize.y);
    float d = max(dot(dx, dx), dot(dy, dy));
    return 0.5 * log2(d); //0.5是技巧，本来是d的平方。
}

float rand(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
}

float noise(in float2 st)
{
    float2 i = floor(st);
    float2 f = frac(st);

    // Four corners in 2D of a tile
    float a = rand(i);
    float b = rand(i + float2(1.0, 0.0));
    float c = rand(i + float2(0.0, 1.0));
    float d = rand(i + float2(1.0, 1.0));

    float2 u = f * f * (3.0 - 2.0 * f);

    return lerp(a, b, u.x) +
        (c - a) * u.y * (1.0 - u.x) +
        (d - b) * u.x * u.y;
}

#define OCTAVES 6

float fbm(in float2 st)
{
    // Initial values
    float value = 0.0;
    float amplitude = .5;
    float frequency = 0.;
    //
    // Loop of octaves
    for (int i = 0; i < OCTAVES; i++)
    {
        value += amplitude * noise(st);
        st *= 2.;
        amplitude *= .5;
    }
    return value;
}


#define INTENSIVE 1

float intensiveNoise(in float2 st)
{
    // Initial values
    float value = 0.0;
    float amplitude = .5;
    float frequency = 0.;
    //
    // Loop of octaves
    for (int i = 0; i < INTENSIVE; i++)
    {
        value += amplitude * rand(st);
        st *= 2.;
        amplitude *= .5;
    }
    return value;
}

half3 DoIntensiveGlitterColor(GlitterInfo i, float2 uv, half3 normalWS, half3 lightDirectionWS)
{
    float attendant = saturate(dot(normalWS, lightDirectionWS));
    return intensiveNoise(uv) * pow(attendant, i.glitterPower);
}

half3 DoWaveGlitterColor(GlitterInfo i, float2 uv, half textureSize, half3 viewDirectionWS, half3 normalWS,
                         half3 lightDirectionWS,
                         TEXTURE2D_PARAM(glitterMap, sampler_glitterMap),
                         TEXTURE2D_PARAM(maskMap, sampler_maskMap),
                         TEXTURE2D_PARAM(specularGlitterMap, sampler_specularGlitterMap))
{
    float attendant = saturate(dot(normalWS, lightDirectionWS));
    float xNum = 0.0;
    float2 glitterUV1 = (0.05 * (i.glitterySpeed - xNum) * viewDirectionWS.xy + uv).rg
        * (i.glitterySpeed * 0.5 + 1.0) * i.glitteryDotScale;

    float4 glitterColor1 = SampleGlitterMap_LOD(glitterUV1, 1, //mipmapLevel(glitterUV1, textureSize),
                                                TEXTURE2D_ARGS(glitterMap, sampler_glitterMap));
    float pNum = 0.0;
    float radian = 1.0;
    float radianAngle = UNITY_PI * radian;

    float cosValue = cos(radianAngle);
    float sinValue = sin(radianAngle);
    float2 piv = float2(0.5, 0.5);
    float2 rotatedUV = mul((0.05 * (-1 * i.glitterySpeed - xNum) * viewDirectionWS.xy + uv).rg - piv,
                           float2x2(cosValue, -sinValue, sinValue, cosValue)) + piv;
    float2 glitterUV2 = rotatedUV * i.glitteryDotScale * (1.0 - i.glitterySpeed * UNITY_INV_PI) * i.maskAdjust;
    float4 glitterColor2 = SampleGlitterMap_LOD(glitterUV2, 1, //mipmapLevel(glitterUV2, textureSize),
                                                TEXTURE2D_ARGS(glitterMap, sampler_glitterMap));
    float4 maskMap_var = SampleMaskMap(uv,TEXTURE2D_ARGS(maskMap, sampler_maskMap));
    float4 specularGlitterMap_var = SampleSpecularGlitterMap(
        uv,TEXTURE2D_ARGS(specularGlitterMap, sampler_specularGlitterMap));
    float3 specularColor = (lerp(pow(i.glitterColor.rgb * glitterColor1.rgb * i.glitterPower, i.glitterContrast),
                                 float3(pNum, pNum, pNum),
                                 max((1.0 - glitterColor2.rgb), maskMap_var.rgb)) + lerp(
        pow((specularGlitterMap_var.rgb * i.specularPower), i.specularContrast), float3(pNum, pNum, pNum),
        maskMap_var.rgb));
    return specularColor * pow((abs(sin((attendant + _Time.y * 0.05) * PI * 10)) + .1) * attendant, i.glitterPower);;
}


half3 DoGlitterColor(GlitterInfo i, float2 uv, half textureSize, half3 viewDirectionWS, half3 normalWS,
                     half3 lightDirectionWS,
                     TEXTURE2D_PARAM(glitterMap, sampler_glitterMap),
                     TEXTURE2D_PARAM(maskMap, sampler_maskMap),
                     TEXTURE2D_PARAM(specularGlitterMap, sampler_specularGlitterMap))
{
    float attendant = saturate(dot(normalWS, lightDirectionWS));
    float xNum = 0.0;
    float2 glitterUV1 = (0.05 * (i.glitterySpeed - xNum) * viewDirectionWS.xy + uv).rg
        * (i.glitterySpeed * 0.5 + 1.0) * i.glitteryDotScale;

    float4 glitterColor1 = SampleGlitterMap_LOD(glitterUV1, 1, //mipmapLevel(glitterUV1, textureSize),
                                                TEXTURE2D_ARGS(glitterMap, sampler_glitterMap));
    float pNum = 0.0;
    float radian = 1.0;
    float radianAngle = UNITY_PI * radian * (1 + sin(_Time.y * i.flowSpeed));

    float cosValue = cos(radianAngle);
    float sinValue = sin(radianAngle);
    float2 piv = float2(0.5, 0.5);
    float2 rotatedUV = mul((0.05 * (-1 * i.glitterySpeed - xNum) * viewDirectionWS.xy + uv).rg - piv,
                           float2x2(cosValue, -sinValue, sinValue, cosValue)) + piv;
    float2 glitterUV2 = rotatedUV * i.glitteryDotScale * (1.0 - i.glitterySpeed * UNITY_INV_PI) * i.maskAdjust;
    float4 glitterColor2 = SampleGlitterMap_LOD(glitterUV2, 1, //mipmapLevel(glitterUV2, textureSize),
                                                TEXTURE2D_ARGS(glitterMap, sampler_glitterMap));

    float4 maskMap_var = SampleMaskMap(uv,TEXTURE2D_ARGS(maskMap, sampler_maskMap));
    float4 specularGlitterMap_var = SampleSpecularGlitterMap(
        uv,TEXTURE2D_ARGS(specularGlitterMap, sampler_specularGlitterMap));
    float3 specularColor = (lerp(pow(abs(i.glitterColor.rgb * glitterColor1.rgb * i.glitterPower), i.glitterContrast),
                                 float3(pNum, pNum, pNum),
                                 max((1.0 - glitterColor2.rgb), maskMap_var.rgb)) + lerp(
        pow(abs(specularGlitterMap_var.rgb * i.specularPower), i.specularContrast), float3(pNum, pNum, pNum),
        maskMap_var.rgb));
    return specularColor * pow(attendant, i.glitterPower);;
}


#endif
