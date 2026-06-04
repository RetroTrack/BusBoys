using Unity.InferenceEngine;
using UnityEngine;

public class TrafficLightDetector : MonoBehaviour
{
    [Header("AI Model")]
    public ModelAsset modelAsset;
    public RenderTexture carCamera;

    [Header("Instellingen")]
    public float detectionInterval = 0.5f;
    public float confidenceThreshold = 0.65f;

    private Worker worker;
    private Texture2D buffer;
    private string[] labels = { "green", "red", "yellow" };
    private string lastDetected = "none";

    void Start()
    {
        var model = ModelLoader.Load(modelAsset);
        worker = new Worker(model, BackendType.GPUCompute);
        buffer = new Texture2D(640, 640, TextureFormat.RGB24, false);

        InvokeRepeating(nameof(RunDetection), 1f, detectionInterval);
    }

    void RunDetection()
    {
        if (carCamera == null)
        {
            Debug.LogError("carCamera is niet ingesteld!");
            return;
        }

        RenderTexture.active = carCamera;
        buffer.ReadPixels(new Rect(0, 0, 640, 640), 0, 0);
        buffer.Apply();
        RenderTexture.active = null;

        using var tensor = TextureConverter.ToTensor(buffer, 640, 640, 3);
        worker.Schedule(tensor);

        var output = worker.PeekOutput("output0") as Tensor<float>;
        output = output.ReadbackAndClone();

        string detected = ParseDetection(output);
        output.Dispose();

        if (detected != lastDetected)
        {
            lastDetected = detected;
            switch (detected)
            {
                case "red": Debug.Log("🔴 Rood stoplicht"); break;
                case "green": Debug.Log("🟢 Groen stoplicht"); break;
                case "yellow": Debug.Log("🟡 Geel stoplicht"); break;
                case "none": Debug.Log("❌ Geen stoplicht zichtbaar"); break;
            }
        }
    }

    string ParseDetection(Tensor<float> output)
    {
        int bestClass = -1;
        float bestConf = 0f;
        float highestConf = 0f;
        int highestClass = -1;

        for (int i = 0; i < 8400; i++)
        {
            for (int c = 0; c < 3; c++)
            {
                float conf = output[0, 4 + c, i];

                if (conf > highestConf)
                {
                    highestConf = conf;
                    highestClass = c;
                }

                if (conf > confidenceThreshold && conf > bestConf)
                {
                    bestConf = conf;
                    bestClass = c;
                }
            }
        }

        if (highestClass >= 0)
            Debug.Log($"Model ziet: {labels[highestClass]} ({highestConf:P0} zekerheid) | Threshold: {confidenceThreshold:P0} | {(highestConf < confidenceThreshold ? "— te laag voor actie" : "✅")}");

        if (bestClass >= 0)
            Debug.Log($"Detectie bevestigd: {labels[bestClass]} ({bestConf:P0} zekerheid)");

        return bestClass >= 0 ? labels[bestClass] : "none";
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
}