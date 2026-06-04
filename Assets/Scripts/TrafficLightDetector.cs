using Unity.InferenceEngine;
using UnityEngine;

public class TrafficLightDetector : MonoBehaviour
{
    [Header("AI Model")]
    public ModelAsset modelAsset;
    public RenderTexture carCamera;

    [Header("Bus (sleep hier de bus)")]
    public BusController busController;

    [Header("Instellingen")]
    public float detectionInterval = 0.5f;
    public float brakeTorque = 5000f;
    private float confidenceThreshold = 0.65f; //Treshhold ai

    private WheelCollider wheelFrontLeft;
    private WheelCollider wheelFrontRight;
    private WheelCollider wheelBackLeft;
    private WheelCollider wheelBackRight;

    private Worker worker;
    private Texture2D buffer;
    private string[] labels = { "green", "red", "yellow" };
    private string lastDetected = "none";
    private bool isBraking = false;

    void Start()
    {
        wheelFrontLeft = busController.wheelFrontLeft;
        wheelFrontRight = busController.wheelFrontRight;
        wheelBackLeft = busController.wheelBackLeft;
        wheelBackRight = busController.wheelBackRight;

        if (wheelFrontLeft == null || wheelFrontRight == null ||
            wheelBackLeft == null || wheelBackRight == null)
        {
            Debug.LogError("Wielen niet gevonden via BusController!");
            return;
        }

        Debug.Log("Wielen gevonden!");

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
                case "red": Debug.Log("🔴 Rood stoplicht — bus stopt"); break;
                case "green": Debug.Log("🟢 Groen stoplicht — bus rijdt door"); break;
                case "yellow": Debug.Log("🟡 Geel stoplicht gedetecteerd"); break;
                case "none": Debug.Log("❌ Geen stoplicht zichtbaar"); break;
            }
        }

        ApplyBrake(detected == "red" || detected == "yellow");
    }

    string ParseDetection(Tensor<float> output)
    {
        float bestConf = confidenceThreshold;
        int bestClass = -1;
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

                if (conf > bestConf)
                {
                    bestConf = conf;
                    bestClass = c;
                }
            }
        }

        // Log altijd wat het model ziet
        if (highestClass >= 0)
            Debug.Log($"Model ziet: {labels[highestClass]} ({highestConf:P0} zekerheid) {(highestConf < confidenceThreshold ? "— te laag voor actie" : "✅")}");

        if (bestClass >= 0)
            Debug.Log($"Detectie bevestigd: {labels[bestClass]} ({bestConf:P0} zekerheid)");

        return bestClass >= 0 ? labels[bestClass] : "none";
    }

    void ApplyBrake(bool brake)
    {
        Debug.Log("Break Break Break");
        if (brake == isBraking) return;
        isBraking = brake;

        float torque = brake ? brakeTorque : 0f;

        wheelFrontLeft.brakeTorque = torque;
        wheelFrontRight.brakeTorque = torque;
        wheelBackLeft.brakeTorque = torque;
        wheelBackRight.brakeTorque = torque;

        if (brake)
        {
            wheelFrontLeft.motorTorque = 0f;
            wheelFrontRight.motorTorque = 0f;
            wheelBackLeft.motorTorque = 0f;
            wheelBackRight.motorTorque = 0f;
        }
    }

    void FixedUpdate()
    {
        if (!isBraking) return;

        wheelFrontLeft.brakeTorque = brakeTorque;
        wheelFrontRight.brakeTorque = brakeTorque;
        wheelBackLeft.brakeTorque = brakeTorque;
        wheelBackRight.brakeTorque = brakeTorque;

        wheelFrontLeft.motorTorque = 0f;
        wheelFrontRight.motorTorque = 0f;
        wheelBackLeft.motorTorque = 0f;
        wheelBackRight.motorTorque = 0f;
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
}