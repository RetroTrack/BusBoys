using BusBoys.Assets.Scripts.Core.Graph;
using BusBoys.Assets.Scripts.Vehicles.Bus.Electric;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Pathfinding
{
    public class RouteNavigator : MonoBehaviour
    {
        public List<Transform> ChargingPoints = new();
        public List<Transform> Waypoints = new();
        [SerializeField] NavGraph navGraph;
        [SerializeField] BusBattery Battery;
        [SerializeField] private float batteryPercentageThreshold = 30;

        int currentWaypointIndex = 0;
        public List<IGraphNode> CurrentPath { get; private set; } = new();
        public int CurrentPathIndex { get; private set; } = 0;
        public NavGraph NavGraph => navGraph;
        public bool HasValidPath => CurrentPath != null && CurrentPathIndex < CurrentPath.Count;

        //AI Begin episode function. These values needed the reset.
        public void BeginEpisode()
        {
            CurrentPath = new List<IGraphNode>();
            CurrentPathIndex = 0;
            currentWaypointIndex = 0;
        }

        //Calculate the maximum possible waypoints.
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

        //Get path to the next node.
        public IGraphNode GetNextPathNode()
        {
            if (CurrentPath == null || CurrentPathIndex >= CurrentPath.Count) return null;
            return CurrentPath[CurrentPathIndex++];
        }

        //Gets called when the end of a path has been reached.
        public bool HasReachedEndOfPath() =>
            CurrentPath == null || CurrentPathIndex >= CurrentPath.Count;

        //Returns the distance of the node to reach.
        public float GetNodeReachedDistance()
        {
            int index = CurrentPathIndex - 1;
            if (CurrentPath == null || index < 0 || index >= CurrentPath.Count)
                return 0f;
            return CurrentPath[index].NodeReachedDistance;
        }

        //When the bus arrives at a waypoint.
        public void ArriveAtWaypoint(Vector3 position, Vector3 facing)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % Waypoints.Count;
            PathfindFromPosition(position, facing);
        }

        // Standard pathfinding towards the current waypoint. Used in SingleWaypoint and FullRoute modes.
        public void PathfindFromPosition(Vector3 from, Vector3? facing = null)
        {
            if (Waypoints.Count == 0)
            {
                Debug.LogError("RouteNavigator: No waypoints assigned.");
                return;
            }
            if (navGraph == null)
            {
                Debug.LogError("RouteNavigator: NavGraph reference missing.");
                return;
            }

            var startNode = FindClosestNode(from);
            var goalNode = FindClosestNode(Waypoints[currentWaypointIndex].position);

            if (startNode == null || goalNode == null)
            {
                Debug.LogWarning("RouteNavigator: could not find graph nodes near start or goal.");
                return;
            }

            // Battery detour (kept from original; note: currently always routes to waypoint goal)
            if (Battery != null && Battery.batteryPercentage < batteryPercentageThreshold)
            {
                var chargingNode = ChargingPoints
                    .Select(cp => FindClosestNode(cp.position))
                    .OrderBy(n => Vector3.Distance(from, n.Position))
                    .FirstOrDefault();

                if (chargingNode != null)
                { goalNode = chargingNode; }
            }

            CurrentPath = navGraph.FindPath(startNode, goalNode, facing) ?? new List<IGraphNode>();
            CurrentPathIndex = 0;
        }

        /// Pathfinds directly to a specific graph node. Used in SingleNode and MultiNode training modes.
        public void PathfindToNode(Vector3 from, IGraphNode goalNode, Vector3? facing = null)
        {
            var startNode = FindClosestNode(from);

            if (startNode == null || goalNode == null)
            {
                Debug.LogWarning("Missing start or goal node.");
                return;
            }

            CurrentPath = navGraph.FindPath(startNode, goalNode, facing)
                          ?? new List<IGraphNode>();

            CurrentPathIndex = 0;
        }

        //Looks to the path ahead.
        public IGraphNode PeekPathNode(int offset)
        {
            var path = CurrentPath;
            if (path == null) return null;

            int index = CurrentPathIndex - 1 + offset;

            if (index >= 0 && index < path.Count)
                return path[index];

            // Lookahead valt buiten het pad — gebruik de waypoint als fallback
            if (offset > 0)
            {
                Transform wp = PeekCurrentWaypoint();
                if (wp != null) return new WaypointGraphNode(wp.position);
            }

            return null;
        }

        //Look at the current waypoint
        public Transform PeekCurrentWaypoint() =>
            Waypoints.Count == 0 ? null : Waypoints[currentWaypointIndex % Waypoints.Count];

        //Finds the closest node.
        IGraphNode FindClosestNode(Vector3 position) =>
        navGraph.Nodes
        .Where(n => n.IsAlive())
        .OrderBy(n => Vector3.Distance(position, n.Position))
        .FirstOrDefault();
    }

}
