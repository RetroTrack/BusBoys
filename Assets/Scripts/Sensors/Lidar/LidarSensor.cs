using BusBoys.Assets.Scripts.ML.Observations;
using System;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Sensors.Lidar
{
    public class LidarSensor : MonoBehaviour, IObservationSource
    {
        [Header("Lidar Settings")]
        [SerializeField, Range(1, 360)] private int numberOfRays = 20;
        [SerializeField] private Vector2 angleRange = new Vector2(-60f, 60f);
        public float maxDistance = 20f;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float updateInterval = 0.1f;

        [Header("Visualization Settings")]
        [SerializeField] private bool visualizeRays = true;
        [SerializeField] private Color nearColor = Color.red;
        [SerializeField] private Color farColor = Color.green;

        [HideInInspector] public LidarHit[] hits;

        private float timer;

        public  float[] Observations => hits != null ? Array.ConvertAll(hits, h => h.normalizedDistance) : new float[numberOfRays];

        public float UpdateInterval { get => updateInterval; set => updateInterval = value; }

        public void Awake()
        {
            hits = new LidarHit[numberOfRays];
        }

        public void Collect(VectorSensor sensor)
        {
            sensor.AddObservation(Observations);
        }

        void Update()
        {
            timer += Time.deltaTime;

            if (timer >= updateInterval)
            {
                timer = 0f;
                Scan();
            }
        }
        //TODO: make more like a parking sensor (detect in angle instead of single ray
        void Scan()
        {
            hits = new LidarHit[numberOfRays]; // Clear previous hits
            float step = (angleRange.y - angleRange.x) / (numberOfRays - 1);

            for (int i = 0; i < numberOfRays; i++)
            {
                float angle = angleRange.x + i * step;

                Vector3 dir = transform.TransformDirection(Quaternion.Euler(0, angle, 0) * Vector3.forward);

                if (Physics.Raycast(transform.position, dir, out RaycastHit hit, maxDistance, layerMask))
                {
                    // Hit detected
                    float t = hit.distance / maxDistance;
                    Color c = Color.Lerp(nearColor, farColor, t);

                    if (visualizeRays)
                        Debug.DrawLine(transform.position, hit.point, c, updateInterval);
                    hits[i] = new LidarHit { direction = dir, normalizedDistance = t, hit = true };
                }
                else
                {
                    // No hit detected
                    if (visualizeRays)
                        Debug.DrawLine(transform.position, transform.position + dir * maxDistance, farColor, updateInterval);
                    hits[i] = new LidarHit { direction = dir, normalizedDistance = 1, hit = false };
                }
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