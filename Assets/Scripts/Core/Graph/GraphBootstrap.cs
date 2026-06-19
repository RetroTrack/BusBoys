using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    public class GraphBootstrap : MonoBehaviour
    {
        [SerializeField] private NavGraph graph;
        [Tooltip("The GameObject containing the navigation nodes and edges (used for automatic linking)"), SerializeField] private GameObject environment;
        [SerializeField] private float linkingDistance = 1f;



        private void Awake()
        {
            if (graph == null)
            {
                Debug.LogError("Graph reference is not set in the inspector.");
                return;
            }
            if (environment == null)
            {
                Debug.LogError("Environment reference is not set in the inspector.");
                return;
            }
            AddNodes();
            LinkEdges();
        }

        [ContextMenu("Add Nodes (not persistent)")]
        public void AddNodes()
        {
            // Find children of the environment that are NavEdges and add them to the graph
            List<NavNode> nodes = environment.GetComponentsInChildren<NavNode>().ToList();
            graph.Nodes = nodes.Cast<IGraphNode>().ToList();
            Debug.Log($"Added {nodes.Count} nodes to the graph.");
            Debug.Log($"Graph now has {graph.Nodes.Count} nodes and {graph.Edges.Count} edges.");
        }

        [ContextMenu("Link Edges (not persistent)")]
        public void LinkEdges()
        {
            // Find children of the environment that are NavEdges and add them to the graph
            graph.Edges = environment.GetComponentsInChildren<NavEdge>().ToList();

            foreach (var edge in graph.Edges)
            {
                foreach (var other in graph.Edges)
                {
                    if (edge == other)
                        continue;

                    if (Vector3.Distance(edge.transform.position, other.transform.position) > linkingDistance)
                        continue;

                    var nodeA = edge.node;
                    var nodeB = other.node;
                    if (nodeA == null || nodeB == null)
                        continue;

                    nodeA.AddNeighbor(nodeB);
                    nodeB.AddNeighbor(nodeA);
                }
            }
            Debug.Log($"Linked {graph.Edges.Count} edges in the graph.");
            Debug.Log($"Graph now has {graph.Nodes.Count} nodes and {graph.Edges.Count} edges.");
        }

    }
}
