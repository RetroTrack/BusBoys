using System.Collections;
using Unity.InferenceEngine;
using UnityEngine;

public class TrafficLightDetector : MonoBehaviour
{
    [Header("AI Model")]
    public ModelAsset modelAsset;
    public RenderTexture carCamera;

    [Header("Settings")]
    public float detectionInterval = 0.5f;
    public float confidenceThreshold = 0.65f;

    private Worker worker;
    private static readonly string[] labels = { "green", "red", "yellow" };

    public enum StoplightState { None, Red, Yellow, Green }
    public StoplightState CurrentStopLightState = StoplightState.None;

    private StoplightState lastState = StoplightState.None;
    private Tensor<float> inputTensor;
    private Texture2D readbackTexture;

    private bool inferencePending = false;
    private Tensor<float> outputTensor;

    void Start()
    {
        carCamera.format = RenderTextureFormat.ARGB32;

        var model = ModelLoader.Load(modelAsset);
        worker = new Worker(model, BackendType.GPUCompute);

        inputTensor = new Tensor<float>(new TensorShape(1, 3, 640, 640));
        readbackTexture = new Texture2D(640, 640, TextureFormat.RGBA32, false);

        StartCoroutine(DetectionLoop());
    }

    IEnumerator DetectionLoop()
    {
        var wait = new WaitForSeconds(detectionInterval);

        while (true)
        {
            if (!inferencePending)
            {
                TextureConverter.ToTensor(carCamera, inputTensor, default);

                worker.Schedule(inputTensor);

                outputTensor = worker.PeekOutput("output0") as Tensor<float>;
                outputTensor.ReadbackRequest();

                inferencePending = true;
            }

            yield return wait;
        }
    }

    void Update()
    {
        if (inferencePending &&
    outputTensor != null &&
    outputTensor.IsReadbackRequestDone())
        {
            using var cpuTensor = outputTensor.ReadbackAndClone();

            var state = ParseDetection(cpuTensor);

            outputTensor.Dispose();
            outputTensor = null;
            inferencePending = false;

            UpdateState(state);
        }
    }

    void UpdateState(StoplightState state)
    {
        CurrentStopLightState = state;
        switch (state)
        {
            case StoplightState.Red: Debug.Log("🔴 Rood stoplicht"); break;
            case StoplightState.Yellow: Debug.Log("🟡 Geel stoplicht"); break;
            case StoplightState.Green: Debug.Log("🟢 Groen stoplicht"); break;
            default: Debug.Log("❌ Geen stoplicht zichtbaar"); break;
        }
    }

    StoplightState ParseDetection(Tensor<float> output)
    {
        int bestClass = -1;
        float bestConf = 0f;

        for (int i = 0; i < 8400; i++)
        {
            for (int c = 0; c < 3; c++)
            {
                float conf = output[0, 4 + c, i];
                if (conf > confidenceThreshold && conf > bestConf)
                {
                    bestConf = conf;
                    bestClass = c;
                }
            }
        }

        if (bestClass < 0) return StoplightState.None;

        Debug.Log($"Detectie: {labels[bestClass]} ({bestConf:P0})");
        return bestClass switch
        {
            0 => StoplightState.Green,
            1 => StoplightState.Red,
            2 => StoplightState.Yellow,
            _ => StoplightState.None
        };
    }

    void OnDestroy()
    {
        worker?.Dispose();
        inputTensor?.Dispose();
        outputTensor?.Dispose();
        if (readbackTexture != null) Destroy(readbackTexture);
    }
}