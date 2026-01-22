#ifndef CUSTOM_SURFACE_INCLUDE
#define CUSTOM_SURFACE_INCLUDE

struct Surface{
    float3 position;
    // 世界空间法线
    float3 normal;
    // 基于插值的法线，可能是世界空间法线的偏移(未标准化)
    float3 interpolatedNormal;
    float3 viewDirection;
    float3 color;
    float alpha;
    float metallic;
    float occlusion;
    float smoothness;
    float depth;
    float fresnelStrength;
    float dither;
};

#endif