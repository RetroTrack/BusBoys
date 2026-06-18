using UnityEngine;

namespace BusBoys.Assets.Scripts.Configs
{
    [CreateAssetMenu(fileName = "RewardConfig", menuName = "ML Agents/RewardConfig")]
    public class RewardConfig : ScriptableObject
    {
        public float rewardForProgressTowardsNode = 0.01f;
        public float rewardForReachingStop = 10f;
        public float rewardForReachingNode = 0.3f;
        public float recalculationPenalty = -2f;
        public float collisionPenalty = -10f;
        public float drivingThroughRedLightPenalty = -5f;
        public float fallenOffMapPenalty = -5f;
        public float brakingPenalty = -0.01f;
        public float steeringWithoutMovingPenalty = -0.01f;
    }
}
