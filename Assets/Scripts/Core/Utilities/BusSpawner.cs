using BusBoys.Assets.Scripts.Core.Graph;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Utilities
{
    public class BusSpawner : MonoBehaviour
    {
        [SerializeField] private NavGraph navGraph;
        [SerializeField] private Transform defaultPosition;
        [SerializeField] private Vector2 randomOffsetRange = new Vector2(-10f, 10f);
        [SerializeField] private float yOffset = 0.5f;

        //Gets and sets a random offset from the normal value.
        public Vector3 GetRandomOffsetFromDefault()
        {
            Vector3 offset = GetRandomOffset();
            return defaultPosition.position + offset;
        }

        //Get the actual random value.
        public Vector3 GetRandomOffset()
        {
            float randomPostionX = Random.Range(randomOffsetRange.x, randomOffsetRange.y);
            float randomPostionZ = Random.Range(randomOffsetRange.x, randomOffsetRange.y);
            return new Vector3(randomPostionX, yOffset, randomPostionZ);
        }

        //Get a random node a random node position
        public Vector3 GetRandomNodePosition()
        {
            int randomIndex = Random.Range(0, navGraph.Nodes.Count);
            IGraphNode spawnNode = navGraph.Nodes[randomIndex];
            return spawnNode.Position;
        }

        //Get a random rotation on the Y axis. So the bus keeps straight.
        public Quaternion GetRandomRotation()
        {
            float randomYRotation = Random.Range(0f, 360f);
            return Quaternion.Euler(0f, randomYRotation, 0f);
        }
    }
}
