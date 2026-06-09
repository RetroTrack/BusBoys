using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    [System.Serializable]
    public class NavEdge : MonoBehaviour
    {
        public NavNode node;
        public float GetCostTo(NavNode other) =>
            Vector3.Distance(transform.position, other.transform.position);
    }
}