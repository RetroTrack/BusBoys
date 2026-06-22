using UnityEngine;

namespace BusBoys.Assets.Scripts.Configs
{
    [CreateAssetMenu(fileName = "RewardConfig", menuName = "ML Agents/RewardConfig")]
    public class RewardConfig : ScriptableObject
    {
        public float rewardForProgressTowardsNode = 0.002f;
        public float rewardForReachingStop = 50f;
        public float rewardForReachingNode = 20f;
        public float recalculationPenalty = -2f;
        public float collisionPenalty = -10f;
        public float drivingThroughRedLightPenalty = -5f;
        public float drivingOffRoadPenalty = -20f;
        public float brakingPenalty = -0.003f;
        public float steeringWithoutMovingPenalty = -0.005f;
        public float stayOnPavementPenalty = -0.005f;
        public float batteryDepletedPenalty = -10f;
        public float batteryOverflowReward = -0.02f;
        public float batteryChargingReward = 0.01f;
    }
}
