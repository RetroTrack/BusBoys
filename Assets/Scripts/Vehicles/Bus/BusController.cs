using BusBoys.Assets.Scripts.ML.Rewards;
using BusBoys.Assets.Scripts.Vehicles.Common;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Vehicles.Bus
{
    public class BusController : VehicleController
    {
        float defaultMotorTorque = 2000f;
        [SerializeField] AgentRewardProvider rewardProvider;

        public void Start()
        {
            defaultMotorTorque = motorTorque;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
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

        public void ModifyTorque(float multiplier)
        {
            motorTorque = defaultMotorTorque * multiplier;
        }
    }
}