using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoadNode : MonoBehaviour
{
    [SerializeField] bool showGizmos = true;
    [SerializeField] bool showLinks = true;
    public List<RoadNode> neighbors = new List<RoadNode>();
    [Tooltip("Distance at which the agent considers a path node reached")]
    public float nodeReachedDistance = 5f;

    public float GetCostTo(RoadNode other) =>
        Vector3.Distance(transform.position, other.transform.position);

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

    [ContextMenu("ResetReached")]
    public void ResetReached()
    {
        nodeReachedDistance = 5f;
    }
}
