using BusBoys.Assets.Scripts.Core.Graph;
using BusBoys.Assets.Scripts.Core.Pathfinding;
using BusBoys.Assets.Scripts.ML.Rewards;
using UnityEngine;

namespace BusBoys.Assets.Scripts.ML.Navigation
{
    public class BusNavigationTracker : MonoBehaviour
    {
        [SerializeField] private BusRouteManager routeManager;
        [SerializeField] private AgentRewardProvider rewardProvider;

        [Header("Navigation Settings")]
        [Tooltip("Distance at which the agent considers a bus stop reached")]
        [SerializeField] private float stopReachedDistance = 2f;
        [SerializeField] private float wrongDirectionAngleThreshold = 120f; // degrees
        [SerializeField] private float wrongDirectionCheckDistance = 5f; // only check if node is within this distance
        [SerializeField] private float recalculateCooldown = 2f; // prevent spam recalculating
        private float lastRecalculateTime = -999f;


        private IGraphNode targetPathNode; // Cached node for observations
        private float maxNodeDistance; // Used for normalising distance to node in observations
        private float maxStopDistance; // Used for normalising distance to stop in observations
        private float previousDistanceToNode;
        // Checks through path nodes and triggers ArriveAtStop when the bus stop is reached

        public void BeginEpisode()
        {
            maxNodeDistance = CalculateMaxNodeDistance();
            maxStopDistance = CalculateMaxStopDistance();

            routeManager.PathfindFromPosition(transform.position, transform.forward);

            SkipUntilCorrectNode();
            previousDistanceToNode = Vector3.Distance(transform.position, targetPathNode.Position);
        }

        public void AdvanceAlongPath()
        {
            if (targetPathNode == null)
            {
                if (!routeManager.HasReachedEndOfPath())
                    targetPathNode = routeManager.GetNextPathNode();
                return;
            }

            Vector3 toNode = targetPathNode.Position - transform.position;
            float distToNode = toNode.magnitude;
            float progress = previousDistanceToNode - distToNode;

            rewardProvider.AddReward(progress * rewardProvider.rewardConfig.rewardForProgressTowardsNode);

            previousDistanceToNode = distToNode;
            // Advance to next node if close enough
            if (distToNode <= routeManager.GetNodeReachedDistance())
            {
                targetPathNode = routeManager.GetNextPathNode();
                return;
            }

            // Wrong direction check
            float angle = Vector3.Angle(transform.forward, toNode);
            bool isFacingWrongWay = angle > wrongDirectionAngleThreshold;
            bool cooldownExpired = Time.time - lastRecalculateTime > recalculateCooldown;

            if (isFacingWrongWay && cooldownExpired)
            {
                Debug.Log($"Bus facing wrong way ({angle:F0}°), recalculating...");
                lastRecalculateTime = Time.time;
                //AddReward(rewardConfig.recalculationPenalty);

                routeManager.PathfindFromPosition(transform.position, transform.forward);
                targetPathNode = routeManager.GetNextPathNode();
                return;
            }

            // Check if bus has reached the bus stop
            Transform nextStop = routeManager.PeekNextBusStop();
            if (nextStop != null)
            {
                float distToStop = Vector3.Distance(transform.position, nextStop.position);
                if (distToStop <= stopReachedDistance)
                {
                    Debug.Log("Arrived at stop, pathfinding to next...");
                    routeManager.ArriveAtStop(transform.position, transform.forward);
                    targetPathNode = routeManager.GetNextPathNode();
                    //AddReward(rewardForReachingStop);
                    return;
                }
            }
        }


        private float CalculateMaxNodeDistance()
        {
            float max = 0f;
            foreach (var node in FindObjectsByType<RoadNode>(FindObjectsSortMode.None)) // TODO: RoadGraph.Nodes
                foreach (var neighbor in node.neighbors)
                    max = Mathf.Max(max, Vector3.Distance(node.transform.position, neighbor.transform.position));

            return max > 0f ? max : 50f; // fallback if graph is empty
        }
        private float CalculateMaxStopDistance()
        {
            var stops = routeManager.busStops; // see below
            if (stops == null || stops.Count < 2) return 200f; // fallback

            float max = 0f;
            for (int i = 0; i < stops.Count; i++)
            {
                int next = (i + 1) % stops.Count;
                max = Mathf.Max(max, Vector3.Distance(stops[i].position, stops[next].position));
            }
            return max;
        }

        private void SkipUntilCorrectNode()
        {
            targetPathNode = routeManager.GetNextPathNode();

            while (targetPathNode != null &&
                   Vector3.Distance(transform.position, targetPathNode.Position)
                   <= routeManager.GetNodeReachedDistance())
            {
                targetPathNode = routeManager.GetNextPathNode();
            }
        }


    }
}
