using System;
using System.Collections.Generic;
using System.Linq;
using BusBoys.Assets.Scripts.Core.Graph;
using BusBoys.Assets.Scripts.Vehicles.Bus.Electric;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Pathfinding
{
    public class RouteNavigator : MonoBehaviour
    {
        public List<Transform> ChargingPoints = new();
        public List<Transform> Waypoints = new();
        [SerializeField] NavGraph navGraph;
        [SerializeField] BusBattery Battery;
        private float BatteryPercentageTrehshold = 20;

        int currentWaypointIndex = 0;
        public List<IGraphNode> CurrentPath { get; private set; } = new();
        public int CurrentPathIndex { get; private set; } = 0;
        public NavGraph NavGraph => navGraph;
        public bool HasValidPath => CurrentPath != null && CurrentPathIndex < CurrentPath.Count;

        public void BeginEpisode()
        {
            CurrentPath = new List<IGraphNode>();
            CurrentPathIndex = 0;
            currentWaypointIndex = 0;
        }

        public float CalculateMaxWaypointSpan()
        {
            if (Waypoints == null || Waypoints.Count < 2) return 200f;
            float max = 0f;
            for (int i = 0; i < Waypoints.Count; i++)
            {
                int next = (i + 1) % Waypoints.Count;
                max = Mathf.Max(max, Vector3.Distance(Waypoints[i].position, Waypoints[next].position));
            }
            return max;
        }

        public IGraphNode GetNextPathNode()
        {
            if (CurrentPath == null || CurrentPathIndex >= CurrentPath.Count) return null;
            return CurrentPath[CurrentPathIndex++];
        }

        public bool HasReachedEndOfPath() =>
            CurrentPath == null || CurrentPathIndex >= CurrentPath.Count;

        public float GetNodeReachedDistance()
        {
            int index = CurrentPathIndex - 1;

            if (CurrentPath == null || index < 0 || index >= CurrentPath.Count)
                return 0f;

            return CurrentPath[index].NodeReachedDistance;
        }

        // Renamed from ArriveAtStop — works for any waypoint type
        public void ArriveAtWaypoint(Vector3 position, Vector3 facing)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % Waypoints.Count;
            PathfindFromPosition(position, facing);
        }

        public void PathfindFromPosition(Vector3 from, Vector3? facing = null)
        {
            if (Waypoints.Count == 0)
            {
                Debug.LogError("No waypoints assigned");
                return;
            }

            if (navGraph == null)
            {
                Debug.LogError("NavGraph reference missing");
                return;
            }

            var startNode = FindClosestNode(from);
            var goalNode = FindClosestNode(Waypoints[currentWaypointIndex].position);

            if (startNode == null || goalNode == null)
            {
                Debug.LogWarning("RouteNavigator: could not find graph nodes near start or goal.");
                return;
            }
            //Battery check
            if (Battery != null && Battery.batteryPercentage < BatteryPercentageTrehshold)
            {
                var chargingNode = ChargingPoints
                    .Select(cp => FindClosestNode(cp.position))
                    .OrderBy(n => Vector3.Distance(from, n.Position))
                    .FirstOrDefault();

                CurrentPath = navGraph.FindPath(startNode, goalNode, facing) ?? new List<IGraphNode>();
                CurrentPathIndex = 0;
            }
        }

        public Transform PeekCurrentWaypoint() =>
            Waypoints.Count == 0 ? null : Waypoints[currentWaypointIndex % Waypoints.Count];

        IGraphNode FindClosestNode(Vector3 position) =>
            navGraph.Nodes
                .OrderBy(n => Vector3.Distance(position, n.Position))
                .FirstOrDefault();
    }
}