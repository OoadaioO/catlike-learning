using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace custom.render.pipeline {
    public partial class CameraRenderer {

        static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");


        ScriptableRenderContext context;
        Camera camera;

        CullingResults cullingResults;

        const string bufferName = "Render Camera";


        CommandBuffer buffer = new CommandBuffer {
            name = bufferName
        };

        public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing) {
            this.context = context;
            this.camera = camera;

            PrepareBuffer();
            PrepareForSceneWindow();

            if (!Cull()) {
                return;
            }

            Setup();
            DrawVisibleGemetry(useDynamicBatching, useGPUInstancing);
            DrawUnsupportedShaders();
            DrawGizmos();
            Submit();
        }


        bool Cull() {
            if (camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {
                cullingResults = context.Cull(ref p);
                return true;
            }
            return false;
        }

        void Setup() {

            context.SetupCameraProperties(camera);

            CameraClearFlags flags = camera.clearFlags;
            buffer.ClearRenderTarget(
                flags <= CameraClearFlags.Depth,
                flags <= CameraClearFlags.Color,
                flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear
            );

            buffer.BeginSample(SampleName);
            ExecuteBuffer();

        }

        void DrawVisibleGemetry(bool useDynamicBatching, bool useGPUInstancing) {
            var sortingSettings = new SortingSettings(camera) {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstancing,
            };
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

            // 命令挂载到context上
            context.DrawSkybox(camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;

            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        }


        void Submit() {
            buffer.EndSample(SampleName);
            ExecuteBuffer();
            context.Submit();
        }

        void ExecuteBuffer() {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }


    }
}