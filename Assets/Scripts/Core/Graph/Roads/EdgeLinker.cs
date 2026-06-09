using System.Collections.Generic;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph.Roads
{
    public class EdgeLinker : MonoBehaviour
    {
        [SerializeField] private float linkingDistance = 1f;

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
}
