using BusBoys.Assets.Scripts.Core.Graph;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Utilities
{
    public class BusSpawner : MonoBehaviour
    {
        [SerializeField] private NavGraph navGraph;
        public Vector3 GetRandomNodePosition()
        {
            int randomIndex = Random.Range(0, navGraph.Nodes.Count);
            IGraphNode spawnNode = navGraph.Nodes[randomIndex];
            return spawnNode.Position;
        }

        public Quaternion GetRandomRotation()
        {
            float randomYRotation = Random.Range(0f, 360f);
            return Quaternion.Euler(0f, randomYRotation, 0f);
        }
    }
}
