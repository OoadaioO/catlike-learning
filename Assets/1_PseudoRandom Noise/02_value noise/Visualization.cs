using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

public abstract class Visualization : MonoBehaviour {

    public enum Shape {
        Plane,
        SphereUV,
        Sphere,
        Torus,
    }
    

    static Shapes.ScheduleDelegate[] shapeJobs = {
        Shapes.Job<Shapes.Plane>.ScheduleParallel,
        Shapes.Job<Shapes.SphereUV>.ScheduleParallel,
        Shapes.Job<Shapes.Sphere>.ScheduleParallel,
        Shapes.Job<Shapes.Torus>.ScheduleParallel,
    };


    [Range(1, 512)]
    [SerializeField] private int resolution = 16;
    [SerializeField] private Material material;
    [SerializeField] private Mesh instanceMesh;

    [Range(-0.5f, 0.5f)]
    [SerializeField] private float displacement = 0.1f;

    [Range(0.1f, 10f)]
    [SerializeField] private float instanceScale = 2f;

    [SerializeField] private Shape shape;

    private bool isDirty = true;
    private Bounds bounds;

    private NativeArray<float3x4> positions, normals;
    private ComputeBuffer positionsBuffer, normalsBuffer;

    static MaterialPropertyBlock propertyBlock;
    static int
        configId = Shader.PropertyToID("_Config"),
        positionsId = Shader.PropertyToID("_Positions"),
        normalId = Shader.PropertyToID("_Normals");



    protected abstract void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock);
    protected abstract void DisableVisualization();
    protected abstract void UpdateVisualization(NativeArray<float3x4> positions, int resolution, JobHandle handle);

    private void OnEnable() {
        isDirty = true;

        int length = resolution * resolution;
        length = length / 4 + (length & 1);


        positions = new NativeArray<float3x4>(length, Allocator.Persistent);
        normals = new NativeArray<float3x4>(length, Allocator.Persistent);


        positionsBuffer = new ComputeBuffer(length * 4, 3 * 4);
        normalsBuffer = new ComputeBuffer(length * 4, 3 * 4);

        propertyBlock ??= new MaterialPropertyBlock();
        EnableVisualization(length, propertyBlock);
        propertyBlock.SetBuffer(positionsId, positionsBuffer);
        propertyBlock.SetBuffer(normalId, normalsBuffer);
        propertyBlock.SetVector(configId, new Vector4(resolution, instanceScale / resolution, displacement));
    }

    private void OnDisable() {
        positions.Dispose();
        normals.Dispose();

        positionsBuffer.Release();
        normalsBuffer.Release();

        positionsBuffer = null;
        normalsBuffer = null;
        DisableVisualization();

    }

    private void OnValidate() {
        if (positionsBuffer != null && enabled) {
            OnDisable();
            OnEnable();
        }
    }


    private void Update() {
        if (isDirty || transform.hasChanged) {
            isDirty = false;
            transform.hasChanged = false;

            JobHandle handle = shapeJobs[(int)shape](
                positions, normals, resolution, transform.localToWorldMatrix, default
            );
            UpdateVisualization(positions, resolution, handle);

            positionsBuffer.SetData(positions.Reinterpret<float3>(3 * 4 * 4));
            normalsBuffer.SetData(normals.Reinterpret<float3>(3 * 4 * 4));
        }


        bounds = new Bounds(transform.position, float3(2f * cmax(abs(transform.lossyScale)) + displacement));

        Graphics.DrawMeshInstancedProcedural(
                instanceMesh,
                0,
                material,
                bounds,
                resolution * resolution,
                propertyBlock
            );

    }


}

