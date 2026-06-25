using System.Collections.Generic;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    [System.Serializable]
    public class NavNode : MonoBehaviour, IGraphNode
    {
        [SerializeField] private List<IGraphNode> neighbors = new List<IGraphNode>();
        [Tooltip("Distance at which the agent considers a path node reached")]
        public float NodeReachedDistance => 5f;

        public Vector3 Position => transform.position;

        public IReadOnlyList<IGraphNode> Neighbors => neighbors;

        //Adds a neighbour to the existing node.
        public void AddNeighbor(IGraphNode neighbor)
        {
            if (!neighbors.Contains(neighbor))
                neighbors.Add(neighbor);
        }

        //Removes all previously added neighbours.
        public void ClearNeighbors()
        {
            neighbors.Clear();
        }

    }
}