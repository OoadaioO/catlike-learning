using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static Unity.Mathematics.math;

public class HashVisualization : MonoBehaviour {

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct HashJob : IJobFor {
        [WriteOnly]
        public NativeArray<uint> hashes;
        public int resolution;
        public float inResolution;
        public SmallXXHash hash;

        public void Execute(int i) {
            int v = (int)floor(i * inResolution + 0.0001f);
            int u = i - v * resolution - resolution / 2;
            v -= resolution / 2;
            hashes[i] = hash.Eat(u).Eat(v);
        }
    }

    [Range(1, 512)]
    [SerializeField] private int resolution = 16;
    [SerializeField] private Material material;
    [SerializeField] private Mesh instanceMesh;
    [SerializeField] private int seed;
    [Range(-2f, 2f)]
    [SerializeField] private float verticalOffset = 1f;

    private ComputeBuffer computeBuffer;
    private NativeArray<uint> hashes;


    static MaterialPropertyBlock propertyBlock;
    static int
        configId = Shader.PropertyToID("_Config"),
        hashesId = Shader.PropertyToID("_Hashes");

    private void OnEnable() {
        int length = resolution * resolution;
        hashes = new NativeArray<uint>(length, Allocator.Persistent);
        computeBuffer = new ComputeBuffer(length, 4);

        new HashJob {
            hashes = hashes,
            resolution = resolution,
            inResolution = 1f / resolution,
            hash = SmallXXHash.Seed(seed),
        }
       .ScheduleParallel(hashes.Length, resolution, default)
       .Complete();

        computeBuffer.SetData(hashes);

        propertyBlock ??= new MaterialPropertyBlock();
        propertyBlock.SetBuffer(hashesId, computeBuffer);
        propertyBlock.SetVector(configId, new Vector4(resolution, 1f / resolution, verticalOffset / resolution));
    }

    private void OnDisable() {
        hashes.Dispose();
        computeBuffer.Release();
        computeBuffer = null;
    }

    private void OnValidate() {
        if (computeBuffer != null) {
            OnDisable();
            OnEnable();
        }
    }


    private void Update() {

        Graphics.DrawMeshInstancedProcedural(
                instanceMesh,
                0,
                material,
                new Bounds(Vector3.zero, Vector3.one),
                computeBuffer.count,
                propertyBlock
            );

    }


}

