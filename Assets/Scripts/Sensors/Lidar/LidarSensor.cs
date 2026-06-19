using BusBoys.Assets.Scripts.ML.Observations;
using System;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Sensors.Lidar
{
    public class LidarSensor : MonoBehaviour, IObservationSource
    {
        [Header("Lidar Settings")]
        [SerializeField, Range(1, 360)] private int numberOfRays = 10;
        [SerializeField] private Vector2 angleRange = new Vector2(-60f, 60f);
        [SerializeField, Range(1, 20)] private int raysPerSegment = 5; // sub-rays per segment
        public float maxDistance = 20f;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float updateInterval = 0.1f;

        [Header("Visualization Settings")]
        [SerializeField] private bool visualizeRays = true;
        [SerializeField] private Color nearColor = Color.red;
        [SerializeField] private Color farColor = Color.green;

        [HideInInspector] public LidarHit[] hits;
        [HideInInspector] public bool passerbyDetected;

        private float timer;

        public float[] Observations => hits != null
            ? Array.ConvertAll(hits, h => h.normalizedDistance)
            : new float[numberOfRays];

        public float UpdateInterval { get => updateInterval; set => updateInterval = value; }

        public void Start()
        {
            hits = new LidarHit[numberOfRays];
        }

        public void Collect(VectorSensor sensor)
        {
            sensor.AddObservation(Observations);
        }

        void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (timer >= updateInterval)
            {
                timer = 0f;
                Scan();
            }
        }

        void Scan()
        {
            passerbyDetected = false;

            float totalSpan = angleRange.y - angleRange.x;
            float segmentSpan = totalSpan / numberOfRays;
            float subStep = raysPerSegment > 1 ? segmentSpan / (raysPerSegment - 1) : 0f;

            for (int i = 0; i < numberOfRays; i++)
            {
                float segmentCenter = angleRange.x + i * segmentSpan + segmentSpan * 0.5f;
                float segmentStart = segmentCenter - segmentSpan * 0.5f;

                float bestDistance = 0f;
                bool anyHit = false;
                Vector3 bestDir = transform.TransformDirection(
                    Quaternion.Euler(0, segmentCenter, 0) * Vector3.forward);
                Vector3 bestHitPoint = Vector3.zero;  // voor visualisatie

                for (int j = 0; j < raysPerSegment; j++)
                {
                    float angle = raysPerSegment > 1 ? segmentStart + j * subStep : segmentCenter;
                    Vector3 dir = transform.TransformDirection(
                        Quaternion.Euler(0, angle, 0) * Vector3.forward);

                    if (Physics.Raycast(transform.position, dir, out RaycastHit hit, maxDistance, layerMask))
                    {
                        float t = 1f - (hit.distance / maxDistance);

                        if (hit.collider.gameObject.name == "Passerby")
                            passerbyDetected = true;

                        if (t > bestDistance)
                        {
                            bestDistance = t;
                            bestDir = dir;
                            bestHitPoint = hit.point;  // sla het punt op
                            anyHit = true;
                        }
                    }
                }

                // Teken alleen één lijn per segment
                if (visualizeRays)
                {
                    if (anyHit)
                        Debug.DrawLine(transform.position, bestHitPoint,
                            Color.Lerp(farColor, nearColor, bestDistance), updateInterval);
                    else
                        Debug.DrawLine(transform.position,
                            transform.position + bestDir * maxDistance, farColor, updateInterval);
                }

                hits[i] = new LidarHit
                {
                    direction = bestDir,
                    normalizedDistance = bestDistance,
                    hit = anyHit
                };
            }
        }

        public struct LidarHit
        {
            public Vector3 direction;
            public float normalizedDistance;
            public bool hit;
        }
    }
}