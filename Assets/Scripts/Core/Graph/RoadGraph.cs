using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    public class RoadGraph : MonoBehaviour
    {
        public List<IGraphNode> Nodes = new List<IGraphNode>();
        public List<RoadEdge> Edges = new List<RoadEdge>();

        [SerializeField] private float turnPenaltyMultiplier = 2f; // higher rather no turning

        // A* pathfinding that considers turn penalties based on the angle between consecutive edges
        public List<IGraphNode> FindPath(IGraphNode start, IGraphNode goal, Vector3? incomingDirection = null)
        {
            var openSet = new List<IGraphNode> { start };
            var cameFrom = new Dictionary<IGraphNode, IGraphNode>();
            var cameFromDir = new Dictionary<IGraphNode, Vector3>(); // track approach direction per node
            var gScore = new Dictionary<IGraphNode, float> { [start] = 0f };
            var fScore = new Dictionary<IGraphNode, float> { [start] = GetDistanceBetween(start, goal) };

            // Seed the starting direction if provided (e.g. bus's forward vector)
            if (incomingDirection.HasValue && incomingDirection.Value != Vector3.zero)
                cameFromDir[start] = incomingDirection.Value.normalized;

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(n => fScore.GetValueOrDefault(n, float.MaxValue)).First();

                if (current == goal)
                    return Reconstruct(cameFrom, current);

                openSet.Remove(current);

                foreach (var neighbor in current.Neighbors)
                {
                    Vector3 edgeDir = (neighbor.Position - current.Position).normalized;
                    float baseCost = GetDistanceBetween(current, neighbor);

                    // Add turn penalty if we know which direction we approached 'current' from
                    float turnCost = 0f;
                    if (cameFromDir.TryGetValue(current, out Vector3 approachDir))
                    {
                        // dot = 1 means straight ahead, -1 means U-turn
                        float dot = Vector3.Dot(approachDir, edgeDir);
                        float turnAngle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;

                        // Scale penalty: 0° = no cost, 180° = full multiplier × base cost
                        turnCost = (turnAngle / 180f) * baseCost * turnPenaltyMultiplier;
                    }

                    float tentative = gScore.GetValueOrDefault(current, float.MaxValue)
                                    + baseCost + turnCost;

                    if (tentative < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                    {
                        cameFrom[neighbor] = current;
                        cameFromDir[neighbor] = edgeDir; // remember how we arrived at this neighbor
                        gScore[neighbor] = tentative;
                        fScore[neighbor] = tentative + GetDistanceBetween(neighbor, goal);
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }
            return null;
        }

        // Get straight-line distance between nodes
        float GetDistanceBetween(IGraphNode a, IGraphNode b) =>
            Vector3.Distance(a.Position, b.Position);


        // Reconstructs the path by backtracking from the goal to the start using the cameFrom map
        List<IGraphNode> Reconstruct(Dictionary<IGraphNode, IGraphNode> cameFrom, IGraphNode current)
        {
            var path = new List<IGraphNode> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }
    }
}