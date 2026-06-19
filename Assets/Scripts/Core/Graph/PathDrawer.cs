using BusBoys.Assets.Scripts.Core.Pathfinding;
using BusBoys.Assets.Scripts.ML.Navigation;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    public class PathDrawer : MonoBehaviour
    {
        [SerializeField] private RouteNavigator nav;
        [SerializeField] private Transform busTransform;
        [SerializeField] private NavigationTracker navigationTracker;
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


        [Header("ML Drawing Settings")]
        [SerializeField] private bool isMLDrawingEnabled = true;
        [SerializeField] private Color currentNodeColor = Color.cyan; // current node
        [SerializeField] private Color lookaheadNodeColor = Color.yellow;  // lookahead node

        [Header("Rendering Lines In Game")]
        public bool renderLinesInGame = true;
        [SerializeField] private Material lineMaterial;
        [SerializeField] private LineRenderer currentNodeRenderer;
        [SerializeField] private LineRenderer lookaheadNodeRenderer;
        [SerializeField] private LineRenderer remainingPathRenderer;

        private void Start()
        {
            SetupLineRenderer(currentNodeRenderer, pathColor, 0.1f);
            SetupLineRenderer(lookaheadNodeRenderer, lookaheadNodeColor, 0.1f);
            SetupLineRenderer(remainingPathRenderer, remainingPathColor, 0.1f);
        }

        private void SetupLineRenderer(LineRenderer lineRenderer, Color color, float width)
        {
            if (lineRenderer == null) return;

            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;

            lineRenderer.material = lineMaterial;
            lineRenderer.material.color = color;

            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            lineRenderer.useWorldSpace = true;
        }

        void OnDrawGizmos()
        {
            if (nav == null || navGraph == null) return;

            // Draw the graph nodes and links
            if (isNodeDrawingEnabled)
                DrawGraph();

            // Draw the A* path split into travelled and remaining
            if(isPathDrawingEnabled)
                DrawAStarPath();

            // Draw agent observations
            if (isMLDrawingEnabled)
                DrawAgentObservations();

            // Draw bus stops
            if (isStopDrawingEnabled)
                DrawPOI(nav.Waypoints, stopColor, stopGizmoSize, "Bus Stop");
            //Draw ChargingStations
            if (isChargingPointDrawingEnabled)
                DrawPOI(nav.ChargingPoints, ChargingPointColor, ChargingPointGizmoSize, "Charging Station");
        }

        private void Update()
        {
            if (renderLinesInGame)
            {
                RenderAgentObservations();
                RenderAStarPath();
            }
            else
            {
                ClearLineRenderers();
            }
        }

        private void ClearLineRenderers()
        {
            if (currentNodeRenderer != null)
                currentNodeRenderer.positionCount = 0;
            if (lookaheadNodeRenderer != null)
                lookaheadNodeRenderer.positionCount = 0;
            if (remainingPathRenderer != null)
                remainingPathRenderer.positionCount = 0;
        }
        



        private void RenderAStarPath()
        {
            if(remainingPathRenderer == null)
            {
                return;
            }
            if (nav.CurrentPath == null ||
                nav.CurrentPath.Count == 0)
            {
                remainingPathRenderer.positionCount = 0;
                return;
            }

            int startIndex = nav.CurrentPathIndex;
            int count = nav.CurrentPath.Count - startIndex;

            remainingPathRenderer.positionCount = count;

            for (int i = startIndex; i < nav.CurrentPath.Count; i++)
            {
                remainingPathRenderer.SetPosition(
                    i - startIndex,
                    nav.CurrentPath[i].Position + Vector3.up * 0.5f
                );
            }
        }

        private void RenderAgentObservations()
        {
            if(lookaheadNodeRenderer == null || currentNodeRenderer == null)
            {
                return;
            }
            if (navigationTracker == null || busTransform == null)
            {
                currentNodeRenderer.positionCount = 0;
                lookaheadNodeRenderer.positionCount = 0;
                return;
            }
            var currentNode = nav.PeekPathNode(0);
            var lookaheadNode = nav.PeekPathNode(1);

            // Huidige target node → cyaan
            if (currentNode != null)
            {
                currentNodeRenderer.positionCount = 2;
                currentNodeRenderer.SetPosition(0, busTransform.position + Vector3.up * 0.5f);
                currentNodeRenderer.SetPosition(1, currentNode.Position + Vector3.up * 0.5f);
            }

            // Lookahead node → geel
            if (lookaheadNode != null)
            {
                lookaheadNodeRenderer.positionCount = 2;
                lookaheadNodeRenderer.SetPosition(0, busTransform.position + Vector3.up * 0.5f);
                lookaheadNodeRenderer.SetPosition(1, lookaheadNode.Position + Vector3.up * 0.5f);
            }
        }




        private void DrawGraph()
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

        private void DrawAStarPath()
        {
            if (nav.CurrentPath != null && nav.CurrentPath.Count > 1)
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
            }
        }

        private void DrawAgentObservations()
        {
            if (navigationTracker != null && busTransform != null)
            {
                var currentNode = nav.PeekPathNode(0);
                var lookaheadNode = nav.PeekPathNode(1);

                // Huidige target node → cyaan
                if (currentNode != null)
                {
                    Gizmos.color = currentNodeColor;
                    Gizmos.DrawLine(busTransform.position, currentNode.Position);
                    Gizmos.DrawSphere(currentNode.Position, nodeGizmoSize * 1.6f);
                }

                // Lookahead node → geel
                if (lookaheadNode != null)
                {
                    Gizmos.color = lookaheadNodeColor;
                    Gizmos.DrawLine(busTransform.position, lookaheadNode.Position);
                    Gizmos.DrawSphere(lookaheadNode.Position, nodeGizmoSize * 1.2f);
                }
            }
        }

        private void DrawPOI(List<Transform> points, Color color, float size, string label)
        {
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                if (point == null) continue;
                Gizmos.color = color;
                Gizmos.DrawSphere(point.position, size);

#if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    point.position + Vector3.up * (size + 0.5f),
                    $"{label} {i}"
                );
#endif
            }

        }

    }
}
