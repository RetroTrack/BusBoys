using BusBoys.Assets.Scripts.ML.Observations;
using System.Collections;
using System.Threading.Tasks;
using Unity.InferenceEngine;
using Unity.MLAgents.Sensors;

using UnityEngine;

namespace BusBoys.Assets.Scripts.Sensors.Traffic
{
    public class TrafficLightDetector : MonoBehaviour, IObservationSource
    {
        [Header("Camera")]
        [SerializeField] private Camera trafficCamera;

        [Header("AI Model")]
        public ModelAsset modelAsset;
        public RenderTexture carCamera;

        [Header("Settings")]
        public float detectionInterval = 0.5f;
        public float confidenceThreshold = 0.65f;
        [SerializeField] bool isDetecting = true;

        private Worker worker;
        private static readonly string[] labels = { "green", "red", "yellow" };

        public enum StoplightState { None, Red, Yellow, Green }
        private StoplightState CurrentStopLightState = StoplightState.None;

        private Tensor<float> inputTensor;

        private bool inferencePending = false;
        private Tensor<float> outputTensor;

        // Cached observations array — no new float[3] every call
        private readonly float[] _observations = new float[3];
        public float[] Observations => _observations;

        // Thread-safe state handoff from background parse task to main thread
        private volatile StoplightState _pendingState = StoplightState.None;
        private volatile bool _hasPendingState = false;
        private volatile bool _parseTaskRunning = false;

        public float UpdateInterval { get => detectionInterval; set => detectionInterval = value; }

        //Passing through observation to the AI model.
        public void Collect(VectorSensor sensor)
        {
            sensor.AddObservation(Observations);
        }

        void Start()
        {
            carCamera.format = RenderTextureFormat.ARGB32;

            var model = ModelLoader.Load(modelAsset);
            worker = new Worker(model, BackendType.GPUCompute);

            inputTensor = new Tensor<float>(new TensorShape(1, 3, 640, 640));

            StartCoroutine(DetectionLoop());
        }

        //Detect traffic light loop. This is to keep detecting the traffic lights.
        IEnumerator DetectionLoop()
        {
            var wait = new WaitForSeconds(detectionInterval);

            while (true)
            {
                if (!isDetecting)
                {
                    yield return wait;
                    continue;
                }

                // Don't start a new inference while parse task is still running
                if (!inferencePending && !_parseTaskRunning)
                {
                    trafficCamera.Render();
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
            // Apply state that was set by the background parse task
            if (_hasPendingState)
            {
                _hasPendingState = false;
                UpdateState(_pendingState);
            }

            if (!inferencePending || outputTensor == null) return;
            if (!outputTensor.IsReadbackRequestDone()) return;

            // Hand off tensor and reset flags immediately so DetectionLoop can proceed
            var tensor = outputTensor;
            outputTensor = null;
            inferencePending = false;
            _parseTaskRunning = true;

            /* FIX: Use GetAwaiter().OnCompleted() instead of async/await.
            
            With async/await the continuation (everything after the await) runs
            on Unity's SynchronizationContext — i.e. the main thread — which is
            exactly what caused the 11ms InvokeCallback stall.
            
            GetAwaiter().OnCompleted() lets us control WHERE the continuation runs.
            We immediately hop to Task.Run so the float[] copy and the parse loop
            both execute on a thread-pool thread. The main thread only queues the
            awaiter and returns — no blocking, no copying, no parsing. 
            */
            var awaiter = tensor.ReadbackAndCloneAsync().GetAwaiter();
            awaiter.OnCompleted(() =>
            {
                // Get the result on the main thread, but do NOT do any heavy work here.
                var cpuTensor = awaiter.GetResult();

                // DownloadToArray() is a single bulk memcpy — fast on main thread.
                // dataOnBackend.Download() is the zero-alloc alternative but requires
                // managing a pre-allocated NativeArray; ToArray() is simpler and the
                // copy itself is not the bottleneck.
                var snapshot = cpuTensor.DownloadToArray();
                cpuTensor.Dispose();
                tensor.Dispose();

                // Parse data safely on a background thread, then set pending state for main thread to apply
                Task.Run(() =>
                {
                    try
                    {
                        _pendingState = ParseDetectionFromBuffer(snapshot);
                    }
                    finally
                    {
                        _parseTaskRunning = false;
                        _hasPendingState = true;
                    }
                });
            });
        }

        void UpdateState(StoplightState state)
        {
            CurrentStopLightState = state;

            // Update cached array in-place instead of allocating a new one
            _observations[0] = state == StoplightState.Green ? 1f : 0f;
            _observations[1] = state == StoplightState.Red ? 1f : 0f;
            _observations[2] = state == StoplightState.Yellow ? 1f : 0f;

            switch (state)
            {
                case StoplightState.Red: Debug.Log("🔴 Rood stoplicht"); break;
                case StoplightState.Yellow: Debug.Log("🟡 Geel stoplicht"); break;
                case StoplightState.Green: Debug.Log("🟢 Groen stoplicht"); break;
                default: Debug.Log("❌ Geen stoplicht zichtbaar"); break;
            }
        }

        ///<summary>
        /// Parses YOLO output from a managed float[] snapshot.
        /// Safe to call from a background thread — no Unity API usage.
        ///
        /// Loop order: class outer, detection inner → sequential memory access,
        /// no cache misses. Pre-computed classOffset avoids repeated multiplication.
        ///
        /// Tensor layout [1, 7, 8400] → flat index = classIndex * 8400 + detectionIndex
        /// </summary>
        StoplightState ParseDetectionFromBuffer(float[] output)
        {
            int bestClass = -1;
            float bestConf = 0f;

            for (int c = 0; c < 3; c++)
            {
                int classOffset = (4 + c) * 8400;

                for (int i = 0; i < 8400; i++)
                {
                    float conf = output[classOffset + i];
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
        }
    }
}