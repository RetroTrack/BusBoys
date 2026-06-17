using BusBoys.Assets.Scripts.Core.Graph;
using BusBoys.Assets.Scripts.Core.Pathfinding;
using BusBoys.Assets.Scripts.ML.Rewards;
using UnityEngine;

namespace BusBoys.Assets.Scripts.ML.Navigation
{
    public class NavigationTracker : MonoBehaviour
    {
        [SerializeField] private RouteNavigator routeNavigator;
        [SerializeField] private AgentRewardProvider rewardProvider;

        [Header("Navigation Settings")]
        [SerializeField] private float waypointReachedDistance = 2f;
        [SerializeField] private float wrongDirectionAngleThreshold = 120f;
        [SerializeField] private float recalculateCooldown = 2f;

        public IGraphNode TargetPathNode => targetPathNode;
        public bool HasTargetNode => targetPathNode != null;    

        private IGraphNode targetPathNode;
        public float MaxNodeDistance { get; private set; }
        public float MaxWaypointSpan { get; private set; }
        private float previousDistanceToNode;
        private float lastRecalculateTime = -999f;

        public void BeginEpisode()
        {
            targetPathNode = null;
            routeNavigator.BeginEpisode();

            MaxNodeDistance = routeNavigator.NavGraph.CalculateMaxEdgeLength();
            MaxWaypointSpan = routeNavigator.CalculateMaxWaypointSpan();

            routeNavigator.PathfindFromPosition(transform.position, transform.forward);
            SkipUntilCorrectNode();

            if (targetPathNode == null)
            {
                Debug.LogWarning("NavigationTracker: No valid targetPathNode at episode start.");
                previousDistanceToNode = 0f;
                return;
            }

            previousDistanceToNode = Vector3.Distance(transform.position, targetPathNode.Position);
        }

        public void AdvanceAlongPath()
        {
            if (targetPathNode == null)
            {
                if (!routeNavigator.HasReachedEndOfPath() && routeNavigator.HasValidPath)
                    targetPathNode = routeNavigator.GetNextPathNode();
                return;
            }

            Vector3 toNode = targetPathNode.Position - transform.position;
            float distToNode = toNode.magnitude;

            rewardProvider.AddReward(
                (previousDistanceToNode - distToNode) * rewardProvider.rewardConfig.rewardForProgressTowardsNode,
                "Progress towards node"
            );
            previousDistanceToNode = distToNode;

            if (distToNode <= routeNavigator.GetNodeReachedDistance() && routeNavigator.HasValidPath)
            {
                targetPathNode = routeNavigator.GetNextPathNode();
                return;
            }

            //TryRecalculateIfWrongDirection(toNode);
            TryAdvanceWaypoint();
        }

        // Exposed for ML observations, normalised between 0 and 1
        public float NormalisedDistanceToNode =>
            targetPathNode == null ? 1f
            : Vector3.Distance(transform.position, targetPathNode.Position) / MaxNodeDistance;

        public float NormalisedDistanceToWaypoint
        {
            get
            {
                var wp = routeNavigator.PeekCurrentWaypoint();
                return wp == null ? 1f
                    : Vector3.Distance(transform.position, wp.position) / MaxWaypointSpan;
            }
        }

        // Private helpers

        void TryRecalculateIfWrongDirection(Vector3 toNode)
        {
            float angle = Vector3.Angle(transform.forward, toNode);
            bool wrongWay = angle > wrongDirectionAngleThreshold;
            bool cooldownDone = Time.time - lastRecalculateTime > recalculateCooldown;

            if (!wrongWay || !cooldownDone) return;

            lastRecalculateTime = Time.time;
            routeNavigator.PathfindFromPosition(transform.position, transform.forward);
            if (!routeNavigator.HasValidPath)
                return; 
            var newNode = routeNavigator.GetNextPathNode();
            if (newNode == targetPathNode)
                return; // no meaningful change
            targetPathNode = newNode;
            Debug.Log($"NavigationTracker: wrong direction ({angle:F0}°), recalculating.");
            rewardProvider.AddReward(rewardProvider.rewardConfig.recalculationPenalty, "Wrong direction");
        }

        void TryAdvanceWaypoint()
        {
            Transform nextWaypoint = routeNavigator.PeekCurrentWaypoint();
            if (nextWaypoint == null) return;

            float dist = Vector3.Distance(transform.position, nextWaypoint.position);
            if (dist > waypointReachedDistance) return;

            Debug.Log("NavigationTracker: waypoint reached, advancing route.");
            rewardProvider.AddReward(rewardProvider.rewardConfig.rewardForReachingStop, "Waypoint reached");
            rewardProvider.EndEpisode();
            //routeNavigator.ArriveAtWaypoint(transform.position, transform.forward);
            //targetPathNode = routeNavigator.GetNextPathNode();
        }

        void SkipUntilCorrectNode()
        {
            targetPathNode = routeNavigator.GetNextPathNode();
            while (targetPathNode != null &&
                   Vector3.Distance(transform.position, targetPathNode.Position) <= routeNavigator.GetNodeReachedDistance())
            {
                if (!routeNavigator.HasValidPath)
                    return;
                targetPathNode = routeNavigator.GetNextPathNode();
            }
        }

    }
}
