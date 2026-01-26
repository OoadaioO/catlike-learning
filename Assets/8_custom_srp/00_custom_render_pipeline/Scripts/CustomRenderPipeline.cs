using System.Collections;
using System.Collections.Generic;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine;
using UnityEngine.Rendering;

namespace custom.render.pipeline {
    public partial class CustomRenderPipeline : RenderPipeline {

        CameraRenderer renderer = new CameraRenderer();

        bool useDynamicBatching, useGPUInstancing, useLightsPerObject;

        ShadowSettings shadowSettings;

        PostFXSettings postFXSettings;

        bool allowHDR;

        public CustomRenderPipeline(bool allowHDR,bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher, bool useLightsPerObject,
            ShadowSettings shadowSettings,
            PostFXSettings postFXSettings
        ) {
            this.allowHDR = allowHDR;
            this.postFXSettings = postFXSettings;
            this.shadowSettings = shadowSettings;
            this.useDynamicBatching = useDynamicBatching;
            this.useGPUInstancing = useGPUInstancing;
            this.useLightsPerObject = useLightsPerObject;
            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
            GraphicsSettings.lightsUseLinearIntensity = true;

            InitializeForEditor();
        }

#if UNITY_EDITOR
        partial void InitializeForEditor() {
            Lightmapping.SetDelegate(lightsDelegate);
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            Lightmapping.ResetDelegate();
        }
#endif



        protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        }

        protected override void Render(ScriptableRenderContext context, List<Camera> cameras) {

            for (int i = 0; i < cameras.Count; i++) {
                renderer.Render(context, cameras[i],allowHDR, useDynamicBatching, useGPUInstancing, useLightsPerObject, shadowSettings,postFXSettings);
            }
        }
    }
}