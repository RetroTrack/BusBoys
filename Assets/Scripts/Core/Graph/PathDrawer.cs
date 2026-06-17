using BusBoys.Assets.Scripts.Core.Pathfinding;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    public class PathDrawer : MonoBehaviour
    {
        [SerializeField] private RouteNavigator nav;
        [SerializeField] private Transform busTransform;
        [SerializeField] private NavGraph navGraph;

        [Header("Node Drawing Settings")]
        [SerializeField] private bool isNodeDrawingEnabled = true;
        [SerializeField] private float nodeGizmoSize = 0.5f;
        [SerializeField] private Color nodeColor = Color.yellow;
        [SerializeField] private Color linkColor = Color.white;

        [Header("Path Drawing Settings")]
        [SerializeField] private bool isPathDrawingEnabled = true;
        [SerializeField] private Color pathColor = Color.blue;
        [SerializeField] private Color remainingPathColor = Color.green; // current target
        [SerializeField] private Color travelledPathColor = Color.grey;  // already visited nodes

        [Header("Stop Drawing Settings")]
        [SerializeField] private bool isStopDrawingEnabled = true;
        [SerializeField] private float stopGizmoSize = 1f;
        [SerializeField] private Color stopColor = Color.magenta;

        [Header("ChargingPoint Drawing Settings")]
        [SerializeField] private bool isChargingPointDrawingEnabled = true;
        [SerializeField] private float ChargingPointGizmoSize = 1f;
        [SerializeField] private Color ChargingPointColor = Color.red;

        void OnDrawGizmos()
        {
            if (nav == null || navGraph == null) return;

            // Draw the graph nodes and links
            if (isNodeDrawingEnabled)
            {
                foreach (var node in navGraph.Nodes)
                {
                    if (node == null) continue;
                    Gizmos.color = nodeColor;
                    Gizmos.DrawSphere(node.Position, nodeGizmoSize);
                    foreach (var neighbor in node.Neighbors)
                    {
                        if (neighbor != null)
                        {
                            Gizmos.color = linkColor;
                            Gizmos.DrawLine(node.Position, neighbor.Position);
                        }
                    }
                }
            }

            // Draw the A* path split into travelled and remaining
            if (nav.CurrentPath != null && nav.CurrentPath.Count > 1 && isPathDrawingEnabled)
            {
                int currentPathIndex = nav.CurrentPathIndex; // see below

                for (int i = 0; i < nav.CurrentPath.Count - 1; i++)
                {
                    var a = nav.CurrentPath[i];
                    var b = nav.CurrentPath[i + 1];
                    if (a == null || b == null) continue;

                    // Nodes before the current index are already travelled
                    Gizmos.color = i < currentPathIndex ? travelledPathColor : pathColor;
                    Gizmos.DrawLine(a.Position, b.Position);
                }

                // Draw spheres on each node
                foreach (var node in nav.CurrentPath)
                {
                    if (node != null)
                        Gizmos.DrawSphere(node.Position, 0.3f);
                }

                // Draw line from bus to the next upcoming node
                if (busTransform != null && currentPathIndex < nav.CurrentPath.Count)
                {
                    Gizmos.color = remainingPathColor;
                    Vector3 nextNodePos = nav.CurrentPath[currentPathIndex].Position;
                    Gizmos.DrawLine(busTransform.position, nextNodePos);

                    // Draw a sphere at the bus's position
                    Gizmos.DrawWireSphere(busTransform.position, 0.5f);
                }
            }

            // Draw bus stops
            if (nav.Waypoints != null && isStopDrawingEnabled)
            {
                for (int i = 0; i < nav.Waypoints.Count; i++)
                {
                    var stop = nav.Waypoints[i];
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
            //Draw ChargingStations
            if (nav.ChargingPoints != null && isChargingPointDrawingEnabled)
            {
                for (int i = 0; i < nav.ChargingPoints.Count; i++)
                {
                    var chargingPoint = nav.ChargingPoints[i];
                    if (chargingPoint == null) continue;
                    Gizmos.color = ChargingPointColor;
                    Gizmos.DrawSphere(chargingPoint.position, ChargingPointGizmoSize);

#if UNITY_EDITOR
                    UnityEditor.Handles.Label(
                        chargingPoint.position + Vector3.up * (ChargingPointGizmoSize + 0.5f),
                        $"Busstop {i}"
                    );
#endif
                }
            }
        }
    }
}
