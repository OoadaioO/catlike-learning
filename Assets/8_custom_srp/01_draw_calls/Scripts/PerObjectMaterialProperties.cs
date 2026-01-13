using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace custom.render.pipeline {

    [DisallowMultipleComponent]
    public class PerObjectMaterialProperties : MonoBehaviour {
        static int baseColorId = Shader.PropertyToID("_BaseColor");
        static int cutoffId = Shader.PropertyToID("_Cutoff");
        static int metallicId = Shader.PropertyToID("_Metallic");
        static int smoothnessId = Shader.PropertyToID("_Smoothness");
        static int emissionColorId = Shader.PropertyToID("_EmissionColor");

        static MaterialPropertyBlock block;

        [SerializeField]
        Color baseColor = Color.white;

        [Range(0, 1)]
        [SerializeField] float cutoff = 0.5f, metallic = 0f, smoothness = 0.5f;

        [SerializeField, ColorUsage(false, true)]
        Color emissionColor = Color.black;



        private void Awake() {
            OnValidate();
        }


        private void OnValidate() {
            if (block == null) {
                block = new MaterialPropertyBlock();
            }

            block.SetFloat(cutoffId, cutoff);
            block.SetColor(baseColorId, baseColor);
            block.SetFloat(metallicId, metallic);
            block.SetFloat(smoothnessId, smoothness);
            block.SetColor(emissionColorId, emissionColor);
            GetComponent<Renderer>().SetPropertyBlock(block);
        }

    }
}