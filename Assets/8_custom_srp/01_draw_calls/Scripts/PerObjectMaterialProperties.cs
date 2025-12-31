using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace custom.render.pipeline {

    [DisallowMultipleComponent]
    public class PerObjectMaterialProperties : MonoBehaviour {
        static int baseColorId = Shader.PropertyToID("_BaseColor");
        static int cutoffId = Shader.PropertyToID("_Cutoff");

        static MaterialPropertyBlock block;

        [SerializeField]
        Color baseColor = Color.white;

        [Range(0,1)]
        [SerializeField] float cutoff = 0.5f;


        private void Awake() {
            OnValidate();
        }


        private void OnValidate() {
            if (block == null) {
                block = new MaterialPropertyBlock();
            }
            
            block.SetFloat(cutoffId,cutoff);
            block.SetColor(baseColorId, baseColor);
            GetComponent<Renderer>().SetPropertyBlock(block);
        }

    }
}