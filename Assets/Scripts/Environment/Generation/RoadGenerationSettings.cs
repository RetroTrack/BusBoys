using UnityEngine;

namespace BusBoys.Assets.Scripts.Environment.Generation
{
    [CreateAssetMenu(fileName = "RoadGenerationSettings", menuName = "Generation/RoadGenerationSettings")]
    public class RoadGenerationSettings : ScriptableObject
    {
        [Header("Grid")]
        public Vector2Int gridSize = new Vector2Int(10, 8);
        public float cellSize = 20f;

        [Header("Generation")]
        public int minAnchorPoints = 4;
        public int maxAnchorPoints = 10;
        public int minAnchorDistance = 4;
        public int anchorAttemptsMultiplier = 12;
        public int extraConnections = 2;
        public bool preferEdgeUsage = true;
        public bool generatePavement = true;

        [Header("Prefabs")]
        public GameObject roadStraight;
        public GameObject roadCorner;
        public GameObject roadTJunction;
        public GameObject roadCross;
        public GameObject roadPavement;

        [Header("Path Cost")]
        public float reusedRoadCostBonus = 0.35f;
        public float turnPenalty = 0.20f;
        public float nearParallelPenalty = 2.25f;
        public bool blockPerfectParallelRoads = true;

        [Header("Rules")]
        public bool disallowAdjacentMajorJunctions = true;
    }
}
