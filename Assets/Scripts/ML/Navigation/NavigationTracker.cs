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

        [Header("Training Curriculum")]
        [SerializeField] private TrainingMode trainingMode = TrainingMode.SingleNode;
        [Tooltip("How many nodes to visit before ending the episode (MultiNode mode only)")]
        [SerializeField] private int multiNodeTarget = 2;

        [Header("Navigation Settings")]
        [SerializeField] private float waypointReachedDistance = 2f;
        [SerializeField] private float wrongDirectionAngleThreshold = 120f;
        [SerializeField] private float recalculateCooldown = 2f;

        public IGraphNode TargetPathNode => targetPathNode;
        public bool HasTargetNode => targetPathNode != null;
        public float MaxNodeDistance { get; private set; }
        public float MaxWaypointSpan { get; private set; }

        private IGraphNode targetPathNode;
        private IGraphNode singleNodeGoal;      // the final goal node in node-targeted modes
        private int nodesVisitedThisEpisode;
        private float previousDistanceToNode;
        private float lastRecalculateTime = -999f;

        //
        public void BeginEpisode()
        {
            targetPathNode           = null;
            singleNodeGoal           = null;
            nodesVisitedThisEpisode  = 0;

            routeNavigator.BeginEpisode();

            MaxNodeDistance  = routeNavigator.NavGraph.CalculateMaxEdgeLength();
            MaxWaypointSpan  = routeNavigator.CalculateMaxWaypointSpan();

            switch (trainingMode)
            {
                case TrainingMode.SingleNode:
                case TrainingMode.MultiNode:
                    InitialiseNodeMode();
                    break;

                case TrainingMode.SingleWaypoint:
                case TrainingMode.FullRoute:
                    InitialiseWaypointMode();
                    break;
            }

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
            if (targetPathNode == null || !targetPathNode.IsAlive())
            {
                if (!routeNavigator.HasReachedEndOfPath() && routeNavigator.HasValidPath)
                    targetPathNode = routeNavigator.GetNextPathNode();
                return;
            }

            Vector3 toNode = targetPathNode.Position - transform.position;
            float distToNode = toNode.magnitude;
            float progress = previousDistanceToNode - distToNode;
            rewardProvider.AddReward(progress * (progress > 0 ? rewardProvider.rewardConfig.rewardForProgressTowardsNode : rewardProvider.rewardConfig.rewardForProgressTowardsNode * 3),
                "Progress towards node"
            );
            previousDistanceToNode = distToNode;

            if (distToNode <= routeNavigator.GetNodeReachedDistance() && routeNavigator.HasValidPath)
            {
                OnPathNodeReached();
                return;
            }

            TryAdvanceWaypoint();
        }


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

        public float NormalisedDistanceToGoalNode =>
            singleNodeGoal == null ? 1f
            : Vector3.Distance(transform.position, singleNodeGoal.Position) / MaxNodeDistance;


        void InitialiseNodeMode()
        {
            // Pick a random goal node that isn't right next to the bus
            singleNodeGoal = PickDistantRandomNode(minDistanceFraction: 0.3f);

            if (singleNodeGoal == null)
            {
                Debug.LogWarning("NavigationTracker: Could not pick a distant goal node; using any random node.");
                int idx = Random.Range(0, routeNavigator.NavGraph.Nodes.Count);
                singleNodeGoal = routeNavigator.NavGraph.Nodes[idx];
            }

            routeNavigator.PathfindToNode(transform.position, singleNodeGoal, transform.forward);
            SkipUntilCorrectNode();
        }

        void InitialiseWaypointMode()
        {
            routeNavigator.PathfindFromPosition(transform.position, transform.forward);
            SkipUntilCorrectNode();
        }

        void OnPathNodeReached()
        {
            rewardProvider.AddReward(rewardProvider.rewardConfig.rewardForReachingNode, "Path node reached");
            nodesVisitedThisEpisode++;
            targetPathNode = routeNavigator.GetNextPathNode();


            switch (trainingMode)
            {
                case TrainingMode.SingleNode:
                    // End episode as soon as we arrive at the goal node
                    if (routeNavigator.HasReachedEndOfPath() || targetPathNode == null)
                    {
                        Debug.Log("NavigationTracker: Goal node reached — ending episode.");
                        rewardProvider.AddReward(rewardProvider.rewardConfig.rewardForReachingStop, "Goal node reached");
                        rewardProvider.EndEpisode();
                    }
                    break;

                case TrainingMode.MultiNode:
                    // End episode after visiting the required number of intermediate nodes
                    if (nodesVisitedThisEpisode >= multiNodeTarget ||
                        routeNavigator.HasReachedEndOfPath() || targetPathNode == null)
                    {
                        Debug.Log($"NavigationTracker: {nodesVisitedThisEpisode} nodes visited — ending episode.");
                        rewardProvider.AddReward(rewardProvider.rewardConfig.rewardForReachingStop, "Multi-node goal reached");
                        rewardProvider.EndEpisode();
                    }
                    break;

                case TrainingMode.SingleWaypoint:
                case TrainingMode.FullRoute:
                    // Path nodes are just stepping stones; waypoint arrival is handled in TryAdvanceWaypoint
                    break;
            }
        }

        void TryAdvanceWaypoint()
        {
            // Only relevant in waypoint modes
            if (trainingMode == TrainingMode.SingleNode || trainingMode == TrainingMode.MultiNode)
                return;

            Transform nextWaypoint = routeNavigator.PeekCurrentWaypoint();
            if (nextWaypoint == null) return;

            float dist = Vector3.Distance(transform.position, nextWaypoint.position);
            if (dist > waypointReachedDistance) return;

            Debug.Log("NavigationTracker: waypoint reached.");
            rewardProvider.AddReward(rewardProvider.rewardConfig.rewardForReachingStop, "Waypoint reached");

            if (trainingMode == TrainingMode.SingleWaypoint)
            {
                rewardProvider.EndEpisode();
            }
            else // FullRoute
            {
                routeNavigator.ArriveAtWaypoint(transform.position, transform.forward);
                targetPathNode = routeNavigator.GetNextPathNode();
            }
        }

        void TryRecalculateIfWrongDirection(Vector3 toNode)
        {
            float angle      = Vector3.Angle(transform.forward, toNode);
            bool  wrongWay   = angle > wrongDirectionAngleThreshold;
            bool  cooldownOk = Time.time - lastRecalculateTime > recalculateCooldown;

            if (!wrongWay || !cooldownOk) return;

            lastRecalculateTime = Time.time;

            if (trainingMode == TrainingMode.SingleNode || trainingMode == TrainingMode.MultiNode)
                routeNavigator.PathfindToNode(transform.position, singleNodeGoal, transform.forward);
            else
                routeNavigator.PathfindFromPosition(transform.position, transform.forward);

            if (!routeNavigator.HasValidPath) return;

            var newNode = routeNavigator.GetNextPathNode();
            if (newNode == targetPathNode) return;

            targetPathNode = newNode;
            Debug.Log($"NavigationTracker: wrong direction ({angle:F0}°), recalculating.");
            rewardProvider.AddReward(rewardProvider.rewardConfig.recalculationPenalty, "Wrong direction");
        }

        void SkipUntilCorrectNode()
        {
            targetPathNode = routeNavigator.GetNextPathNode();
            while (targetPathNode != null &&
                   Vector3.Distance(transform.position, targetPathNode.Position) <= routeNavigator.GetNodeReachedDistance())
            {
                if (!routeNavigator.HasValidPath) return;
                targetPathNode = routeNavigator.GetNextPathNode();
            }
        }
        private IGraphNode PeekPathNode(int offset)
        {
            var path = routeNavigator.CurrentPath;
            if (path == null) return null;

            int index = routeNavigator.CurrentPathIndex - 1 + offset;

            if (index >= 0 && index < path.Count)
                return path[index];

            // Lookahead valt buiten het pad — gebruik de waypoint als fallback
            if (offset > 0)
            {
                Transform wp = routeNavigator.PeekCurrentWaypoint();
                if (wp != null) return new WaypointGraphNode(wp.position);
            }

            return null;
        }

        // Picks a random node that is at least (minDistanceFraction * MaxNodeDistance * graphDiameter) away.
        IGraphNode PickDistantRandomNode(float minDistanceFraction = 0.3f)
        {
            var nodes = routeNavigator.NavGraph.Nodes;
            if (nodes == null || nodes.Count == 0) return null;

            // Estimate a minimum travel distance using the graph's rough diameter
            float minDist = MaxNodeDistance * minDistanceFraction * 3f;

            // Collect candidates that are far enough away
            var candidates = new System.Collections.Generic.List<IGraphNode>();
            foreach (var n in nodes)
            {
                if (Vector3.Distance(transform.position, n.Position) >= minDist)
                    candidates.Add(n);
            }

            if (candidates.Count == 0) return null;

            return candidates[Random.Range(0, candidates.Count)];
        }
    }
}
