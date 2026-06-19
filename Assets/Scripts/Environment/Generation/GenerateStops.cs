using BusBoys.Assets.Scripts.Core.Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Environment.Generation
{
    public class GenerateStops : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private RouteNavigator routeNavigator;
        [SerializeField] private GameObject busStop;
        [SerializeField] private GameObject chargeStation;
        [SerializeField] private GameObject generatedRoads;
        [SerializeField] private GameObject generatedStops;
        [SerializeField] private Mesh straightRoad;

        [Header("Variables")]
        [SerializeField] private int totalStops;
        [SerializeField] private int amountOfStops;
        [SerializeField] private int amountOfChargers;


        [Header("Debug")]
        [SerializeField] private List<GameObject> straightRoads = new List<GameObject>();
        [SerializeField] private List<GameObject> spawnedStops = new List<GameObject>();
        [SerializeField] private List<Transform> targets = new List<Transform>();

        private List<int> randomValue = new List<int>();

        public void GenerateStop()
        {
            ResetStops();

            MeshFilter[] roads = generatedRoads.GetComponentsInChildren<MeshFilter>(true);

            foreach (MeshFilter road in roads)
            {
                if (road != null && road.sharedMesh == straightRoad)
                {
                    straightRoads.Add(road.gameObject);
                }
            }

            // Verwijder null/destroyed entries zonder tijdens foreach te muteren
            straightRoads.RemoveAll(s => s == null);

            randomValue.Clear();
            totalStops = amountOfStops + amountOfChargers;

            if (straightRoads.Count < totalStops)
            {
                Debug.LogWarning($"GenerateStops: only {straightRoads.Count} straight road segments available, " +
                                  $"need {totalStops}. Reducing stop count for this episode.");
                totalStops = straightRoads.Count;
            }

            if (totalStops <= 0)
            {
                Debug.LogWarning("GenerateStops: no straight road segments found, skipping stop generation.");
                return;
            }

            int j = straightRoads.Count / totalStops;
            var usedIndices = new HashSet<int>();

            for (int i = 1; i <= totalStops; i++)
            {
                int candidate = Random.Range(j * (i - 1), j * i);
                // Bescherm tegen duplicate/out-of-range indices als j klein is
                candidate = Mathf.Clamp(candidate, 0, straightRoads.Count - 1);
                randomValue.Add(candidate);
            }

            for (int i = 0; i < totalStops; i++)
            {
                int r = randomValue[i];
                if (r < 0 || r >= straightRoads.Count) continue; // extra veiligheidscheck

                Vector3 spawnPos = new Vector3();
                GameObject roadObj = straightRoads[r];
                if (roadObj == null) continue;

                Vector3 roadTrans = roadObj.transform.position;
                GameObject temp = null;
                if (roadObj.transform.eulerAngles.y > 0f)
                {
                    if (amountOfStops - 1 < i)
                    {
                        spawnPos = new Vector3(roadTrans.x, roadTrans.y, roadTrans.z - 5f);
                        temp = Instantiate(chargeStation, spawnPos, Quaternion.Euler(-90, 180, 0), generatedStops.transform);
                    }
                    else
                    {
                        spawnPos = new Vector3(roadTrans.x, roadTrans.y, roadTrans.z - 6.5f);
                        temp = Instantiate(busStop, spawnPos, Quaternion.identity, generatedStops.transform);
                    }
                }
                else
                {
                    if (amountOfStops - 1 < i)
                    {
                        spawnPos = new Vector3(roadTrans.x - 5f, roadTrans.y, roadTrans.z);
                        temp = Instantiate(chargeStation, spawnPos, Quaternion.Euler(-90, 180, 0), generatedStops.transform);
                    }
                    else
                    {
                        spawnPos = new Vector3(roadTrans.x - 6.5f, roadTrans.y, roadTrans.z);
                        temp = Instantiate(busStop, spawnPos, Quaternion.Euler(0, 90, 0), generatedStops.transform);
                    }
                }

                if (temp.GetComponent<BusStop>().target != null)
                    targets.Add(temp.GetComponent<BusStop>().target);

                spawnedStops.Add(temp);
            }

            ReturnBusStops();
        }

        public void ReturnBusStops()
        {
            routeNavigator.Waypoints = targets;
        }

        public void ResetStops()
        {
            if (generatedStops != null)
            {
                for (int i = generatedStops.transform.childCount - 1; i >= 0; i--)
                {
                    var child = generatedStops.transform.GetChild(i);
                    child.SetParent(null);
                    Destroy(child.gameObject);
                }
            }

            targets.Clear();
            spawnedStops.Clear();
            straightRoads.Clear();

            if (routeNavigator != null && routeNavigator.Waypoints != null)
                routeNavigator.Waypoints.Clear();
        }
    }
}
