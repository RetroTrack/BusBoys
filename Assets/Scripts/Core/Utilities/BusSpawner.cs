using BusBoys.Assets.Scripts.Core.Graph;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Utilities
{
    public class BusSpawner : MonoBehaviour
    {
        [SerializeField] private NavGraph navGraph;
        [SerializeField] private Transform defaultPostion;
        [SerializeField] private Vector2 randomOffsetRange = new Vector2(-10f, 10f);


        public Vector3 GetRandomOffsetFromDefault()
        {
            Vector3 offset = GetRandomOffset();
            return new Vector3(defaultPostion.position.x + offset.x, defaultPostion.position.y, defaultPostion.position.z + offset.z);
        }

        public Vector3 GetRandomOffset()
        {
            float randomPostionX = Random.Range(randomOffsetRange.x, randomOffsetRange.y);
            float randomPostionZ = Random.Range(randomOffsetRange.x, randomOffsetRange.y);
            return new Vector3(randomPostionX, 0f, randomPostionZ);
        }

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
