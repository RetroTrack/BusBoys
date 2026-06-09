using BusBoys.Assets.Scripts.ML.Rewards;
using BusBoys.Assets.Scripts.Vehicles.Common;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Vehicles.Bus
{
    public class BusController : VehicleController
    {
        [SerializeField] AgentRewardProvider rewardProvider;

        private void FixedUpdate()
        {
            // Driving
            Accelerate();

            // Braking
            Brake();


            // Steering
            Steer();

            // Rewarding
            CheckFallenOfMap();
        }

        public void CheckFallenOfMap()
        {
            if (transform.position.y < -5f) // Fallen off the world
            {
                rewardProvider.SetReward(rewardProvider.rewardConfig.fallenOffMapPenalty);
                rewardProvider.EndEpisode();
            }
        }
    }
}