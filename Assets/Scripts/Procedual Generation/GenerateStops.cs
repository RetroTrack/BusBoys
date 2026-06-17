using BusBoys.Assets.Scripts.Core.Pathfinding;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace BusBoys
{
    public class GenerateStops : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private RouteNavigator routeNavigator;
        [SerializeField] private GameObject busStop;
        [SerializeField] private GameObject generatedRoads;
        [SerializeField] private GameObject generatedStops;
        [SerializeField] private Mesh straightRoad;

        [Header("Variables")]
        [SerializeField] private int amountOfStops;

        [Header("Debug")]
        [SerializeField] private List<GameObject> straightRoads = new List<GameObject>();
        [SerializeField] private List<GameObject> spawnedStops = new List<GameObject>();
        [SerializeField] private List<Transform> targets = new List<Transform>();


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
            
            foreach (GameObject straight in straightRoads)
            {
                bool isReallyNull = ((object)straight) == null;
                bool isMissingOrDestroyed = ((object)straight) != null && straight == null;
                if (isReallyNull || isMissingOrDestroyed)
                {
                    straightRoads.Remove(straight);
                }
            }

            for (int i = 0; i < amountOfStops; i++) {
               int r = Random.Range(0, straightRoads.Count);

                Vector3 spawnPos = new Vector3();
                GameObject roadObj = straightRoads[r];
                Vector3 roadTrans = roadObj.transform.position;
                GameObject temp;
                if (roadObj.transform.eulerAngles.y > 0f)
                {
                    spawnPos = new Vector3(roadTrans.x, roadTrans.y, roadTrans.z-6.5f);
                    temp = Instantiate(busStop, spawnPos, Quaternion.identity, generatedStops.transform);
                } else
                {
                    spawnPos = new Vector3(roadTrans.x-6.5f, roadTrans.y, roadTrans.z);
                    temp = Instantiate(busStop, spawnPos, Quaternion.Euler(0,90,0) , generatedStops.transform);
                    
                }

                if (temp.GetComponent<BusStop>().target != null)
                    targets.Add(temp.GetComponent<BusStop>().target);

                spawnedStops.Add(temp);

                Debug.Log("Spawned bus stop at:" + spawnPos);
            }
            routeNavigator.Waypoints = targets;
        }

        public void ResetStops()
        {
            if (generatedStops != null)
            {
                for (int i = generatedStops.transform.childCount - 1; i >= 0; i--)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(generatedStops.transform.GetChild(i).gameObject);
                    else
#endif
                        Destroy(generatedStops.transform.GetChild(i).gameObject);
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
