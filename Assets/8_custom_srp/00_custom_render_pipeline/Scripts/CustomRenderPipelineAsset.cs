using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace custom.render.pipeline {
    [CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
    public class CustomRenderPipelineAsset : RenderPipelineAsset {

        protected override RenderPipeline CreatePipeline() {
            return new CustomRenderPipeline();
        }
    }
}