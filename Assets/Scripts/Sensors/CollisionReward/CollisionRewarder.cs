using BusBoys.Assets.Scripts.ML.Rewards;
using System;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Sensors.CollisionReward
{
    public class CollisionRewarder : MonoBehaviour
    {
        public bool isActive = false;
        [SerializeField] private AgentRewardProvider rewardProvider;
        [SerializeField] private LayerMask layerMask;

        //On collision with something from the bus. Add panelty.
        public void OnCollisionEnter(Collision collision)
        {
            if (!isActive) return;

            if (rewardProvider == null)
            {
                Debug.LogError("Reward provider is not assigned in the inspector, reward will not be calculated.");
                return;
            }
            HandleCollision(collision.gameObject.layer);
        }

        //When driver off road. Stop current episode with a set on driven off road.
        public void DriveOffRoad()
        {
            if (!isActive) return;

            rewardProvider.SetReward(rewardProvider.rewardConfig.drivingOffRoadPenalty, "Driven off road");
            rewardProvider.EndEpisode();
        }

        //Giving rewards for staying on pavement.
        public void StayOnPavement()
        {
            if (!isActive) return;

            if (rewardProvider == null)
            {
                Debug.LogError("Reward provider is not assigned in the inspector, reward will not be calculated.");
                return;
            }
            // This method should be called by a separate sensor that detects whether the bus is on the pavement.
            // If the bus is off the pavement, we can apply a small penalty to encourage it to stay on the road.
            rewardProvider.AddReward(rewardProvider.rewardConfig.stayOnPavementPenalty, "Stayed on pavement");
        }

        //Handle collissions and end episode on collision
        public void HandleCollision(int layer)
        {
            if (!isActive) return;

            if (IsInLayerMask(layer, layerMask))
            {
                rewardProvider.SetReward(rewardProvider.rewardConfig.collisionPenalty, "Collision");
                rewardProvider.EndEpisode();
            }
        }

        //Only watches collision in certain layers.
        public static bool IsInLayerMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }

        //When pedestrian is hit.
        public void HitPedestrian()
        {
            if (!isActive) return;

            rewardProvider.SetReward(rewardProvider.rewardConfig.collisionPenalty, "Hit pedestrian");
            rewardProvider.EndEpisode();
        }
    }
}