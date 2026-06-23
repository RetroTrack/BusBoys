using BusBoys.Assets.Scripts.Core.Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    public class NavGraph : MonoBehaviour
    {
        [HideInInspector] public List<IGraphNode> Nodes = new();
        [HideInInspector] public List<NavEdge> Edges = new();

        public float turnPenaltyMultiplier = 2f;

        public List<IGraphNode> FindPath(IGraphNode start, IGraphNode goal, Vector3? incomingDir = null)
            => AStarPathfinder.FindPath(start, goal, ComputeEdgeCost, incomingDir);

        //Gets the distance to the next node.
        public float CalculateMaxEdgeLength()
        {
            float max = 0f;
            foreach (var node in Nodes)
            {
                if (!node.IsAlive()) continue;
                foreach (var neighbor in node.Neighbors)
                {
                    if (!neighbor.IsAlive()) continue;
                    max = Mathf.Max(max, Vector3.Distance(node.Position, neighbor.Position));
                }
            }
            return max > 0f ? max : 50f;
        }

        //Calculates the cost to connect to an edge. This must be as low as possible.
        float ComputeEdgeCost(IGraphNode from, IGraphNode to, Vector3? approachDir)
        {
            float baseCost = Vector3.Distance(from.Position, to.Position);
            if (!approachDir.HasValue) return baseCost;

            Vector3 edgeDir = (to.Position - from.Position).normalized;
            float dot = Vector3.Dot(approachDir.Value, edgeDir);
            float turnAngle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
            return baseCost + (turnAngle / 180f) * baseCost * turnPenaltyMultiplier;
        }
    }
}