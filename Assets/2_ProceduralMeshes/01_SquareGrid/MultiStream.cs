using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;


namespace ProceduralMeshes.Streams {
    public struct MultiStream : IMeshStreams {


        [NativeDisableContainerSafetyRestriction]
        NativeArray<float3> stream0, stream1;
        [NativeDisableContainerSafetyRestriction]
        NativeArray<float4> stream2;
        [NativeDisableContainerSafetyRestriction]
        NativeArray<float2> stream3;

        [NativeDisableContainerSafetyRestriction]
        NativeArray<TriangleUInt16> triangles;

        public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;

        public void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount) {

            NativeArray<VertexAttributeDescriptor> attributes =
                new(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            attributes[0] = new VertexAttributeDescriptor(dimension: 3, stream: 0);
            attributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3, stream: 1);
            attributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, dimension: 4, stream: 2);
            attributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: 3);

            meshData.SetVertexBufferParams(vertexCount, attributes);
            attributes.Dispose();

            meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount) {
                vertexCount = vertexCount,
                bounds = bounds
            }, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

            stream0 = meshData.GetVertexData<float3>(0);
            stream1 = meshData.GetVertexData<float3>(1);
            stream2 = meshData.GetVertexData<float4>(2);
            stream3 = meshData.GetVertexData<float2>(3);

            triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);


        }

        public void SetVertex(int index, Vertex data) {
            stream0[index] = data.position;
            stream1[index] = data.normal;
            stream2[index] = data.tangent;
            stream3[index] = data.texCoord0;
        }
    }
}

