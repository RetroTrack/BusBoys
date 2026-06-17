using BusBoys.Assets.Scripts.ML.Rewards;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Sensors.CollisionReward
{
    public class CollisionRewarder : MonoBehaviour
    {
        [SerializeField] private AgentRewardProvider rewardProvider;
        [SerializeField] private LayerMask layerMask;

        public void OnCollisionEnter(Collision collision)
        {
            if (rewardProvider == null)
            {
                Debug.LogError("Reward provider is not assigned in the inspector, reward will not be calculated.");
                return;
            }

            if (IsInLayerMask(collision.gameObject.layer, layerMask))
            {
                rewardProvider.AddReward(rewardProvider.rewardConfig.collisionPenalty);
            }
        }
        public static bool IsInLayerMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }
}