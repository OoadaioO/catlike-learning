using System;
using ProceduralMeshes;
using ProceduralMeshes.Generators;
using ProceduralMeshes.Streams;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour {

    static MeshJobScheduleDelegate[] jobs = {
        MeshJob<SquareGrid,SingleStream>.ScheduleParallel,
        MeshJob<SharedSQuareGrid,SingleStream>.ScheduleParallel,
        MeshJob<SharedTriangleGrid,SingleStream>.ScheduleParallel,
        MeshJob<PointyHexagonGrid,SingleStream>.ScheduleParallel,
        MeshJob<FlatHexagonGrid,SingleStream>.ScheduleParallel,
        MeshJob<UVSphere,SingleStream>.ScheduleParallel,
        MeshJob<CubeSphere,SingleStream>.ScheduleParallel,
    };

    public enum MeshType {
        SquareGrid,
        SharedSQuareGrid,
        SharedTriangleGrid,
        PointyHexagonGrid,
        FlatHexagonGrid,
        UVSphere,
        CubeSphere
    }

    public enum MaterialMode { Flat, Ripple, LatLonMap, CubeMap };

    [System.Flags]
    public enum GizmoMode { Nothing = 0, Vertices = 1, Normals = 0b10, Tangents = 0b100 }

    [SerializeField] private GizmoMode gizmos;


    [Range(1, 50)]
    [SerializeField] private int resolution;

    [SerializeField] private MeshType meshType;
    [SerializeField] private MaterialMode material;
    [SerializeField] private Material[] materials;


    private Mesh mesh;


    private Vector3[] vertices, normals;
    private Vector4[] tangents;

    private void Awake() {
        mesh = new Mesh {
            name = "Procedural Mesh"
        };

        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void GenerateMesh() {
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        jobs[(int)meshType](
            mesh,
            meshData,
            resolution,
            default
        )
        .Complete();

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
    }

    private void OnValidate() {
        enabled = true;
    }

    private void Update() {
        GenerateMesh();
        enabled = false;
        vertices = null;
        normals = null;
        tangents = null;

        GetComponent<MeshRenderer>().material = materials[(int)material];

    }

    private void OnDrawGizmos() {
        if (gizmos == GizmoMode.Nothing || mesh == null) return;

        bool drawVertices = (gizmos & GizmoMode.Vertices) != 0;
        bool drawNormals = (gizmos & GizmoMode.Normals) != 0;
        bool drawTangents = (gizmos & GizmoMode.Tangents) != 0;

        if (vertices == null) {
            vertices = mesh.vertices;
        }
        if (drawNormals && normals == null) {
            normals = mesh.normals;
        }
        if (drawTangents && tangents == null) {
            tangents = mesh.tangents;
        }



        Transform t = transform;

        for (int i = 0; i < vertices.Length; i++) {
            Vector3 position = t.TransformPoint(vertices[i]);
            if (drawVertices) {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(position, 0.02f);
            }
            if (drawNormals) {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(position, t.TransformPoint(normals[i]) * 0.2f);
            }
            if (drawTangents) {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(position, t.TransformPoint(tangents[i]) * 0.2f);
            }

        }
    }

}
