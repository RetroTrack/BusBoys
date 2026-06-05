using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadGraph : MonoBehaviour
{
    [SerializeField] private float linkingDistance = 1f;

    public List<RoadNode> FindPath(RoadNode start, RoadNode goal)
    {
        var openSet = new List<RoadNode> { start };
        var cameFrom = new Dictionary<RoadNode, RoadNode>();
        var gScore = new Dictionary<RoadNode, float> { [start] = 0f };
        var fScore = new Dictionary<RoadNode, float> { [start] = Heuristic(start, goal) };

        while (openSet.Count > 0)
        {
            var current = openSet.OrderBy(n => fScore.GetValueOrDefault(n, float.MaxValue)).First();

            if (current == goal)
                return Reconstruct(cameFrom, current);

            openSet.Remove(current);

            foreach (var neighbor in current.neighbors)
            {
                float tentative = gScore.GetValueOrDefault(current, float.MaxValue)
                                + current.GetCostTo(neighbor);

                if (tentative < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative;
                    fScore[neighbor] = tentative + Heuristic(neighbor, goal);
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        return null;
    }

    float Heuristic(RoadNode a, RoadNode b) =>
        Vector3.Distance(a.transform.position, b.transform.position);

    List<RoadNode> Reconstruct(Dictionary<RoadNode, RoadNode> cameFrom, RoadNode current)
    {
        var path = new List<RoadNode> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }



    [ContextMenu("Link Edges")]
    public void LinkEdges()
    {
        List<RoadEdge> edges = new List<RoadEdge>();

        foreach (var edge in FindObjectsByType<RoadEdge>(FindObjectsSortMode.None))
            edges.Add(edge);

        foreach (var edge in edges)
        {
            foreach (var other in edges)
            {
                if (edge == other)
                    continue;
                if (Vector3.Distance(edge.transform.position, other.transform.position) <= linkingDistance)
                {
                    var nodeA = edge.node;
                    var nodeB = other.node;
                    if (nodeA != null && nodeB != null)
                    {
                        if (!nodeA.neighbors.Contains(nodeB))
                            nodeA.neighbors.Add(nodeB);
                        if (!nodeB.neighbors.Contains(nodeA))
                            nodeB.neighbors.Add(nodeA);
                    }
                }
            }
        }

    }
}