using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    //Defines the edge of a node. This is bound to the node. Multiple NavEdges may exist with the NavNode.
    //The function directly gets the connection to another possible connecting node.
    [System.Serializable]
    public class NavEdge : MonoBehaviour
    {
        public NavNode node;
        public float GetCostTo(NavNode other) =>
            Vector3.Distance(transform.position, other.transform.position);
    }
}