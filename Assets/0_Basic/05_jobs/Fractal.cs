
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using float3x4 = Unity.Mathematics.float3x4;
using quaternion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;

public class Fractal : MonoBehaviour {

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct UpdateFractalLevelJob : IJobFor {


        public float deltaTime;
        public float scale;

        [ReadOnly]
        public NativeArray<FractalPart> parents;
        public NativeArray<FractalPart> parts;

        [WriteOnly]
        public NativeArray<float3x4> matrices;

        public void Execute(int i) {
            FractalPart parent = parents[i / 5];
            FractalPart part = parts[i];
            part.spinAngle += part.spinVelocity * deltaTime;

            float3 upAxis = mul(mul(parent.worldRotation, part.rotation), up());
            float3 sagAxis = cross(up(), upAxis);
            float sagMagnitude = length(sagAxis);
            quaternion baseRotation;
            if (sagMagnitude > 0f) {

                sagAxis /= sagMagnitude;
                quaternion sagRotation = quaternion.AxisAngle(sagAxis, part.maxSagAngle * sagMagnitude);
                baseRotation = mul(sagRotation, parent.worldRotation);

            } else {
                baseRotation = parent.worldRotation;
            }

            part.worldRotation = mul(baseRotation, mul(part.rotation, quaternion.RotateY(part.spinAngle)));
            part.worldPosition = parent.worldPosition + mul(part.worldRotation, float3(0f, 1.5f * scale, 0f));

            parts[i] = part;
            float3x3 r = float3x3(part.worldRotation) * scale;
            matrices[i] = float3x4(r.c0, r.c1, r.c2, part.worldPosition);
        }
    }

    struct FractalPart {
        public float3 worldPosition;
        public quaternion rotation, worldRotation;
        public float spinAngle, maxSagAngle, spinVelocity;
    }

    static int
        matricesId = Shader.PropertyToID("_Matrices"),
        colorAId = Shader.PropertyToID("_ColorA"),
        colorBId = Shader.PropertyToID("_ColorB"),
        sequenceNumberId = Shader.PropertyToID("_SequenceNumbers")
        ;

    static MaterialPropertyBlock propertyBlock;



    static quaternion[] rotaitons = {
        quaternion.identity,
        quaternion.RotateZ(-0.5f * PI),quaternion.RotateZ(0.5f * PI),
        quaternion.RotateX(0.5f * PI),quaternion.RotateX(-0.5f * PI),
    };


    [Range(3, 8)]
    [SerializeField] protected int depth = 4;
    [SerializeField] private Material material;
    [SerializeField] private Mesh mesh, leafMesh;
    [Range(1, 5)]
    [SerializeField] private int jobCount = 1;
    [SerializeField] private Gradient gradientA, gradientB;
    [SerializeField] private Color leafColorA, leafColorB;

    [Range(0f, 90f)]
    [SerializeField] private float maxSagAngleA = 15f, maxSagAngleB = 25f;

    [Range(0f, 90f)]
    [SerializeField] private float spinSpeedA = 20f, spinSpeedB = 25f;
    [Range(0f, 1f)]
    [SerializeField] private float reverseSpinChance = 0.25f;


    private NativeArray<FractalPart>[] parts;
    private NativeArray<float3x4>[] matrices;
    private ComputeBuffer[] matricesBuffers;

    private Vector4[] sequenceNumbers;

    private void OnEnable() {
        parts = new NativeArray<FractalPart>[depth];
        matrices = new NativeArray<float3x4>[depth];
        matricesBuffers = new ComputeBuffer[depth];
        sequenceNumbers = new Vector4[depth];
        int stride = 3 * 4 * 4;
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5) {
            parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
            matricesBuffers[i] = new ComputeBuffer(length, stride);
            sequenceNumbers[i] = new Vector4(Random.value, Random.value, Random.value, Random.value);
        }


        parts[0][0] = CreatePart(0);
        for (int li = 1; li < parts.Length; li++) {
            NativeArray<FractalPart> levelParts = parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5) {
                for (int ci = 0; ci < 5; ci++) {
                    levelParts[fpi + ci] = CreatePart(ci);
                }
            }
        }

        propertyBlock ??= new MaterialPropertyBlock();
    }

    private void OnDisable() {
        for (int i = 0; i < matricesBuffers.Length; i++) {
            matricesBuffers[i].Release();
            parts[i].Dispose();
            matrices[i].Dispose();
        }
        parts = null;
        matrices = null;
        matricesBuffers = null;
        sequenceNumbers = null;
    }

    private void OnValidate() {
        if (parts != null && enabled) {
            OnDisable();
            OnEnable();
        }
    }

    private FractalPart CreatePart(int childIndex) => new FractalPart {
        rotation = rotaitons[childIndex],
        maxSagAngle = radians(Random.Range(maxSagAngleA, maxSagAngleB)),
        spinVelocity = (Random.value < reverseSpinChance ? -1f : 1f) * radians(Random.Range(spinSpeedA, spinSpeedB)),
    };


    private void Update() {



        FractalPart rootPart = parts[0][0];

        rootPart.spinAngle += rootPart.spinVelocity * Time.deltaTime;
        rootPart.worldRotation = mul(transform.rotation, mul(rootPart.rotation, quaternion.RotateY(rootPart.spinAngle)));
        rootPart.worldPosition = transform.position;
        parts[0][0] = rootPart;
        float objectScale = transform.lossyScale.x;

        float3x3 r = float3x3(rootPart.worldRotation) * objectScale;
        matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.worldPosition);

        float scale = objectScale;
        JobHandle jobHandle = default;
        for (int li = 1; li < parts.Length; li++) {
            scale *= 0.5f;

            jobHandle = new UpdateFractalLevelJob {
                deltaTime = Time.deltaTime,
                scale = scale,
                parents = parts[li - 1],
                parts = parts[li],
                matrices = matrices[li]
            }.ScheduleParallel(parts[li].Length, jobCount, jobHandle);
        }

        jobHandle.Complete();

        Bounds bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * float3(1f, 1f, 1f));

        int leafIndex = matricesBuffers.Length - 1;
        for (int i = 0; i < matricesBuffers.Length; i++) {

            ComputeBuffer buffer = matricesBuffers[i];
            buffer.SetData(matrices[i]);

            Color colorA, colorB;
            Mesh instanceMesh;
            if (i == leafIndex) {
                colorA = leafColorA;
                colorB = leafColorB;
                instanceMesh = leafMesh;
            } else {
                float gradientInteporator = i / (matricesBuffers.Length - 2.0f);
                colorA = gradientA.Evaluate(gradientInteporator);
                colorB = gradientB.Evaluate(gradientInteporator);
                instanceMesh = mesh;
            }
            propertyBlock.SetColor(colorAId, colorA);
            propertyBlock.SetColor(colorBId, colorB);

            propertyBlock.SetVector(sequenceNumberId, sequenceNumbers[i]);
            propertyBlock.SetBuffer(matricesId, buffer);
            Graphics.DrawMeshInstancedProcedural(instanceMesh, 0, material, bounds, buffer.count, propertyBlock);

        }


    }





}
