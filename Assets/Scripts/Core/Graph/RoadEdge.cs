using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    [System.Serializable]
    public class RoadEdge : MonoBehaviour
    {
        [SerializeField] bool showGizmos = true;
        public RoadNode node;
        public float GetCostTo(RoadNode other) =>
            Vector3.Distance(transform.position, other.transform.position);

        void OnDrawGizmos()
        {
            if (!showGizmos) return;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.4f);
        }

        void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.6f);
        }
    }
}