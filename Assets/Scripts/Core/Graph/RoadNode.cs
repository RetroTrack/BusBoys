using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    [System.Serializable]
    public class RoadNode : MonoBehaviour, IGraphNode
    {
        [SerializeField] bool showGizmos = true;
        [SerializeField] bool showLinks = true;
        public List<RoadNode> neighbors = new List<RoadNode>();
        [Tooltip("Distance at which the agent considers a path node reached")]
        public float NodeReachedDistance => 5f;

        public Vector3 Position => transform.position;

        public IReadOnlyList<IGraphNode> Neighbors => neighbors.Cast<IGraphNode>().ToList();


        void OnDrawGizmos()
        {
            if (showGizmos)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(transform.position, 0.4f);
            }

            if (showLinks)
            {
                Gizmos.color = Color.white;
                foreach (var n in neighbors)
                    if (n != null)
                        Gizmos.DrawLine(transform.position, n.transform.position);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (showGizmos)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, 0.6f);
            }
            if (showLinks)
            {
                Gizmos.color = Color.green;
                foreach (var n in neighbors)
                    if (n != null)
                        Gizmos.DrawLine(transform.position, n.transform.position);
            }
        }

    }
}