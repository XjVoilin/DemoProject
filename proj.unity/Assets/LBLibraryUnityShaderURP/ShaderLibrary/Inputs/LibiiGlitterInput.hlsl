#ifndef UNIVERSAL_PIPELINE_LIBII_GLITTER_INPUT_INCLUDED
#define UNIVERSAL_PIPELINE_LIBII_GLITTER_INPUT_INCLUDED


#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "../../ShaderLibrary/Core/LibiiCore.hlsl"

CBUFFER_START (UnityPerMaterial)
float4 _BumpMap_ST;
float4 _BaseMap_ST;
half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;
half _Smoothness;
half _Metallic;
half _BumpScale;
half _OcclusionStrength;
half _Surface;
half shininessVar;
half fakeLight;
half reflectionCubeMapPower;
half cubeMapBlur;
half specularPower;
half specularContrast;
half4 glitterColor;
half glitterPower;
half glitterContrast;
half glitterySpeed;
half glitteryDotScale;
half flowSpeed;
half maskAdjust;
CBUFFER_END

TEXTURECUBE(reflectionCubeMap);
SAMPLER(sampler_reflectionCubeMap);

TEXTURE2D(maskMap);
SAMPLER (sampler_maskMap);

TEXTURE2D(specularGlitterMap);
SAMPLER (sampler_specularGlitterMap);

TEXTURE2D(glitterMap);
SAMPLER (sampler_glitterMap);

TEXTURE2D(_OcclusionMap);
SAMPLER (sampler_OcclusionMap);
TEXTURE2D (_MetallicGlossMap);
SAMPLER (sampler_MetallicGlossMap);
TEXTURE2D (_SpecGlossMap);
SAMPLER (sampler_SpecGlossMap);

#ifdef _SPECULAR_SETUP
#define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
#else
    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)
#endif


half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
{
    half4 specGloss;

    #ifdef _METALLICSPECGLOSSMAP
    specGloss = SAMPLE_METALLICSPECULAR(uv);
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    specGloss.a = albedoAlpha * _Smoothness;
    #else
    specGloss.a *= _Smoothness;
    #endif
    #else // _METALLICSPECGLOSSMAP
    #if _SPECULAR_SETUP
    specGloss.rgb = _SpecColor.rgb;
    #else
    specGloss.rgb = _Metallic.rrr;
    #endif

    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    specGloss.a = albedoAlpha * _Smoothness;
    #else
    specGloss.a = _Smoothness;
    #endif
    #endif

    return specGloss;
}

half SampleOcclusion(float2 uv)
{
    #ifdef _OCCLUSIONMAP
    // TODO: Controls things like these by exposing SHADER_QUALITY levels (low, medium, high)
    #if defined(SHADER_API_GLES)
    return SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
    #else
    half occ = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
    return LerpWhiteTo(occ, _OcclusionStrength);
    #endif
    #else
    return 1.0;
    #endif
}

 

inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData, float4 normal_ST)
{
    ZERO_INITIALIZE(SurfaceData, outSurfaceData);

    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

    half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

    #if _SPECULAR_SETUP
    outSurfaceData.metallic = 1.0h;
    outSurfaceData.specular = specGloss.rgb;
    #else
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0h, 0.0h, 0.0h);
    #endif

    outSurfaceData.smoothness = specGloss.a;
    outSurfaceData.normalTS = SampleNormal(uv * normal_ST.xy + normal_ST.zw, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap),
                                           _BumpScale);
    outSurfaceData.occlusion = SampleOcclusion(uv);
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
}


#endif
