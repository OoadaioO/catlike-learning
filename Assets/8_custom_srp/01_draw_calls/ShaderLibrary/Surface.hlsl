#ifndef CUSTOM_SURFACE_INCLUDE
#define CUSTOM_SURFACE_INCLUDE

struct Surface{
    float3 position;
    float3 normal;
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