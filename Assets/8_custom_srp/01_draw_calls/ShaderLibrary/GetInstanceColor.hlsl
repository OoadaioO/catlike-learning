

#ifndef CUSTOM_INSTANCING
#define CUSTOM_INSTANCING

UNITY_INSTANCING_BUFFER_START(Props)
  UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
UNITY_INSTANCING_BUFFER_END(Props)

void GetInstancedColor_float(out float4 Out){
    Out = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor);
}

#endif