using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadGraph : MonoBehaviour
{
    [SerializeField] private float linkingDistance = 1f;
    [SerializeField] private float turnPenaltyMultiplier = 2f; // higher rather no turning


    [Header("Debug")]
    [SerializeField] private Transform busTransform;
    [SerializeField] private BusRouteManager busRouteManager;
    [SerializeField] private bool showPath = true;
    [SerializeField] private bool showBusStops = true;
    [SerializeField] private float stopGizmoSize = 1f;
    [SerializeField] private Color pathColor = Color.blue;
    [SerializeField] private Color stopColor = Color.magenta;
    [SerializeField] private Color remainingPathColor = Color.green; // current target
    [SerializeField] private Color travelledPathColor = Color.grey;  // already visited nodes

    void OnDrawGizmos()
    {
        if (busRouteManager == null) return;

        // Draw the A* path split into travelled and remaining
        if (showPath && busRouteManager.currentPath != null && busRouteManager.currentPath.Count     > 1)
        {
            int currentPathIndex = busRouteManager.CurrentPathIndex; // see below

            for (int i = 0; i < busRouteManager.currentPath.Count - 1; i++)
            {
                var a = busRouteManager.currentPath[i];
                var b = busRouteManager.currentPath[i + 1];
                if (a == null || b == null) continue;

                // Nodes before the current index are already travelled
                Gizmos.color = i < currentPathIndex ? travelledPathColor : pathColor;
                Gizmos.DrawLine(a.transform.position, b.transform.position);
            }

            // Draw spheres on each node
            foreach (var node in busRouteManager.currentPath)
            {
                if (node != null)
                    Gizmos.DrawSphere(node.transform.position, 0.3f);
            }

            // Draw line from bus to the next upcoming node
            if (busTransform != null && currentPathIndex < busRouteManager.currentPath.Count)
            {
                Gizmos.color = remainingPathColor;
                Vector3 nextNodePos = busRouteManager.currentPath[currentPathIndex].transform.position;
                Gizmos.DrawLine(busTransform.position, nextNodePos);

                // Draw a sphere at the bus's position
                Gizmos.DrawWireSphere(busTransform.position, 0.5f);
            }
        }

        // Draw bus stops
        if (showBusStops && busRouteManager.busStops != null)
        {
            for (int i = 0; i < busRouteManager.busStops.Count; i++)
            {
                var stop = busRouteManager.busStops[i];
                if (stop == null) continue;

                Gizmos.color = stopColor;
                Gizmos.DrawSphere(stop.position, stopGizmoSize);

#if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    stop.position + Vector3.up * (stopGizmoSize + 0.5f),
                    $"Busstop {i}"
                );
#endif
            }
        }
    }

    public List<RoadNode> FindPath(RoadNode start, RoadNode goal, Vector3? incomingDirection = null)
    {
        var openSet = new List<RoadNode> { start };
        var cameFrom = new Dictionary<RoadNode, RoadNode>();
        var cameFromDir = new Dictionary<RoadNode, Vector3>(); // track approach direction per node
        var gScore = new Dictionary<RoadNode, float> { [start] = 0f };
        var fScore = new Dictionary<RoadNode, float> { [start] = Heuristic(start, goal) };

        // Seed the starting direction if provided (e.g. bus's forward vector)
        if (incomingDirection.HasValue && incomingDirection.Value != Vector3.zero)
            cameFromDir[start] = incomingDirection.Value.normalized;

        while (openSet.Count > 0)
        {
            var current = openSet.OrderBy(n => fScore.GetValueOrDefault(n, float.MaxValue)).First();

            if (current == goal)
                return Reconstruct(cameFrom, current);

            openSet.Remove(current);

            foreach (var neighbor in current.neighbors)
            {
                Vector3 edgeDir = (neighbor.transform.position - current.transform.position).normalized;
                float baseCost = current.GetCostTo(neighbor);

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