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
    };

    public enum MeshType {
        SquareGrid,
        SharedSQuareGrid,
        SharedTriangleGrid,
        PointyHexagonGrid,
        FlatHexagonGrid,
    }



    [Range(1, 50)]
    [SerializeField] private int resolution;

    [SerializeField] private MeshType meshType;


    private Mesh mesh;

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
    }

}
