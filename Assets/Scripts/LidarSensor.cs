using System.Collections.Generic;
using UnityEngine;
using RangeAttribute = UnityEngine.RangeAttribute;

public class LidarSensor : MonoBehaviour
{
    [Header("Lidar Settings")]
    [SerializeField, Range(1, 360)] private int numberOfRays = 360;
    [SerializeField] private Vector2 angleRange = new Vector2(-60f, 60f);
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float updateInterval = 0.1f;

    [Header("Visualization Settings")]
    [SerializeField] private bool visualizeRays = true;
    [SerializeField] private Color nearColor = Color.red;
    [SerializeField] private Color farColor = Color.green;
    [SerializeField] private float rayWidth = 0.02f;

    [HideInInspector] public List<LidarHit> hits = new List<LidarHit>();

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            timer = 0f;
            Scan();
        }
    }

    void Scan()
    {
        hits.Clear();
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

                Debug.DrawLine(transform.position, hit.point, c, updateInterval);
                hits.Add(new LidarHit { direction = dir, distance = hit.distance, hit = true });
            }
            else
            {
                // No hit detected
                Debug.DrawLine(transform.position,
                    transform.position + dir * maxDistance,
                    farColor,
                    updateInterval);
                hits.Add(new LidarHit { direction = dir, distance = maxDistance, hit = false });
            }
        }
    }
    public struct LidarHit
    {
        public Vector3 direction;
        public float distance;
        public bool hit;
    }
}
