using UnityEngine;

public class GPUGraph : MonoBehaviour {
    public enum TransitionMode { Cycle, Random }


    const int maxResolution = 1000;

    [Range(10, maxResolution)]
    [SerializeField] private int resolution = 10;

    [SerializeField] private FunctionLibrary.FunctionName function;
    [SerializeField] private TransitionMode transitionMode;

    [Min(1f)]
    [SerializeField] private float functionDuration = 1f, transitionDuration = 1.0f;

    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private Material material;
    [SerializeField] private Mesh mesh;

    private float duration;
    private bool transitioning;
    private FunctionLibrary.FunctionName transitionFunction;

    private ComputeBuffer positionsBuffer;

    private readonly static int
        positionId = Shader.PropertyToID("_Positions"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        stepId = Shader.PropertyToID("_Step"),
        timeId = Shader.PropertyToID("_Time"),
        transitionProgressId = Shader.PropertyToID("_TransitionProgress")
        ;


    private void OnEnable() {
        positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector3)));
    }
    private void OnDisable() {
        positionsBuffer.Release();
        positionsBuffer = null;
    }

    private void Update() {

        duration += Time.deltaTime;
        if (transitioning) {
            if (duration > transitionDuration) {
                duration -= transitionDuration;
                transitioning = false;
            }
        } else if (duration > functionDuration) {
            duration -= functionDuration;
            transitioning = true;
            transitionFunction = function;
            PickNextFunction();
        }


        UpdateFunctionOnGPU();

    }

    private void PickNextFunction() {
        function = transitionMode == TransitionMode.Cycle ?
            FunctionLibrary.GetNextFunctionName(function) :
            FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }


    private void UpdateFunctionOnGPU() {

        int kernelIndex = (int)function + (int)(transitioning ? transitionFunction : function) * FunctionLibrary.FunctionCount;

        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);
        if (transitioning) {
            computeShader.SetFloat(transitionProgressId, Mathf.SmoothStep(0f, 1f, duration / transitionDuration));
        }


        computeShader.SetBuffer(kernelIndex, positionId, positionsBuffer);

        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(kernelIndex, groups, groups, 1);

        material.SetBuffer(positionId, positionsBuffer);
        material.SetFloat(stepId, step);


        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);

    }

    // private void UpdateFunctionTransition() {
    //     FunctionLibrary.Function
    //         from = FunctionLibrary.GetFunction(transitionFunction),
    //         to = FunctionLibrary.GetFunction(function);

    //     float progress = duration / transitionDuration;
    //     float time = Time.time;
    //     float step = 2f / resolution;
    //     float v = 0.5f * step - 1f;
    //     for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
    //         if (x == resolution) {
    //             x = 0;
    //             z += 1;
    //             v = (z + 0.5f) * step - 1f;
    //         }
    //         float u = (x + 0.5f) * step - 1f;
    //         points[i].localPosition = FunctionLibrary.Morph(u, v, time, from, to, progress);
    //     }

    // }

    // private void UpdateFunction() {
    //     FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);

    //     float step = 2f / resolution;
    //     float v = 0.5f * step - 1f;
    //     for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
    //         if (x == resolution) {
    //             x = 0;
    //             z += 1;
    //             v = (z + 0.5f) * step - 1f;
    //         }
    //         float u = (x + 0.5f) * step - 1f;
    //         points[i].localPosition = f(u, v, Time.time);
    //     }
    // }
}
