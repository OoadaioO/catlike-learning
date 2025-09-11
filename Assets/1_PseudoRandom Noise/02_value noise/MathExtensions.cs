
using Unity.Mathematics;
using static Unity.Mathematics.math;
public static class MathExtensions {
    /// <summary>
    /// 同时对 4 个向量进行线性变换
    /// </summary>
    /// <param name="trs">只保留旋转、缩放、平移的变换矩阵</param>
    /// <param name="p"> 将顶点转置到4个分量，方便线性变换一次性处理4个顶点的3个分量 </param>
    /// <param name="w">w分量</param>
    /// <returns>处理完的4个顶点的3个分量，需要转置后才是实际的顶点列向量</returns>
    public static float4x3 TransformVectors(this float3x4 trs, float4x3 p, float w = 1f) => float4x3(
            trs.c0.x * p.c0 + trs.c1.x * p.c1 + trs.c2.x * p.c2 + trs.c3.x * w,
            trs.c0.y * p.c0 + trs.c1.y * p.c1 + trs.c2.y * p.c2 + trs.c3.y * w,
            trs.c0.z * p.c0 + trs.c1.z * p.c1 + trs.c2.z * p.c2 + trs.c3.z * w
    );

    public static float3x4 Get3x4(this float4x4 m) => float3x4(m.c0.xyz, m.c1.xyz, m.c2.xyz, m.c3.xyz);

}
