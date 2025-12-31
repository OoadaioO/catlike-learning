using UnityEngine;

namespace custom.render.pipeline {
    public class MeshBall : MonoBehaviour {
        static int baseColorID = Shader.PropertyToID("_BaseColor");
        static int cutoffId = Shader.PropertyToID("_Cutoff");


        [SerializeField] Mesh mesh = default;
        [SerializeField] Material material = default;


        [Range(0, 1)]
        [SerializeField] float cutoff = 0.5f;

        Matrix4x4[] matrices = new Matrix4x4[1023];
        Vector4[] baseColors = new Vector4[1023];
        float[] cutoffs = new float[1023];


        MaterialPropertyBlock block;

        float lastCutoff;

        private void Awake() {
            for (int i = 0; i < matrices.Length; i++) {
                matrices[i] = Matrix4x4.TRS(
                    Random.insideUnitSphere * 10f,
                    Quaternion.Euler(
                        Random.value * 360f, Random.value * 360f, Random.value * 360f
                    )
                    , Vector3.one
                );
                baseColors[i] = new Vector4(Random.value, Random.value, Random.value, Random.Range(0.5f, 1f));

                cutoffs[i] = cutoff;
            }

            lastCutoff = cutoff;

        }


        private void Update() {
            if (block == null) {
                block = new MaterialPropertyBlock();

                block.SetVectorArray(baseColorID, baseColors);
                block.SetFloatArray(cutoffId, cutoffs);
            }

            if (lastCutoff != cutoff) {
                for (int i = 0; i < cutoffs.Length; i++) {
                    cutoffs[i] = cutoff;
                }
                lastCutoff = cutoff;

                block.SetFloatArray(cutoffId, cutoffs);
            }


            Graphics.DrawMeshInstanced(mesh, 0, material, matrices, 1023, block);
        }


    }
}