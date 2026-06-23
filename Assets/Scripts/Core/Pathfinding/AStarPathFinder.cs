using BusBoys.Assets.Scripts.Core.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Pathfinding
{
    public static class AStarPathfinder
    {
        // Cost function is injected — the caller decides how to price an edge
        // A* Star algorithm to find the best route. (See what A* does online)
        // This is our own version with some minor tweaks to make it fit our grid and roads.
        public static List<IGraphNode> FindPath(
            IGraphNode start,
            IGraphNode goal,
            Func<IGraphNode, IGraphNode, Vector3?, float> edgeCostFn,
            Vector3? incomingDirection = null)
        {
            if (start == null || goal == null)
                return new List<IGraphNode>();

            var openSet = new List<IGraphNode> { start };
            var cameFrom = new Dictionary<IGraphNode, IGraphNode>();
            var cameFromDir = new Dictionary<IGraphNode, Vector3>();
            var gScore = new Dictionary<IGraphNode, float> { [start] = 0f };
            var fScore = new Dictionary<IGraphNode, float>
            { [start] = Vector3.Distance(start.Position, goal.Position) };

            if (incomingDirection.HasValue && incomingDirection.Value != Vector3.zero)
                cameFromDir[start] = incomingDirection.Value.normalized;

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(n => fScore.GetValueOrDefault(n, float.MaxValue)).First();
                if (current == goal) return Reconstruct(cameFrom, current);
                openSet.Remove(current);

                foreach (var neighbor in current.Neighbors)
                {
                    cameFromDir.TryGetValue(current, out Vector3 approachDir);
                    float cost = edgeCostFn(current, neighbor, approachDir == Vector3.zero ? null : approachDir);
                    float tentative = gScore.GetValueOrDefault(current, float.MaxValue) + cost;

                    if (tentative < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                    {
                        cameFrom[neighbor] = current;
                        cameFromDir[neighbor] = (neighbor.Position - current.Position).normalized;
                        gScore[neighbor] = tentative;
                        fScore[neighbor] = tentative + Vector3.Distance(neighbor.Position, goal.Position);
                        if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                    }
                }
            }
            return new List<IGraphNode>();
        }

        //Reconstruction of the A* algoritm.
        static List<IGraphNode> Reconstruct(Dictionary<IGraphNode, IGraphNode> cameFrom, IGraphNode current)
        {
            var path = new List<IGraphNode> { current };
            while (cameFrom.ContainsKey(current)) { current = cameFrom[current]; path.Insert(0, current); }
            return path;
        }
    }
}