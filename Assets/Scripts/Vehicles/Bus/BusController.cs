using BusBoys.Assets.Scripts.ML.Rewards;
using BusBoys.Assets.Scripts.Vehicles.Common;
using System;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Vehicles.Bus
{
    public class BusController : VehicleController
    {
        private bool shouldBrake = false;
        float defaultMotorTorque = 2000f;
        [SerializeField] AgentRewardProvider rewardProvider;

        public void Start()
        {
            defaultMotorTorque = motorTorque;
        }
        public void SetShouldBrake(bool shouldBrake)
        {
            this.shouldBrake = shouldBrake;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            // Driving
            Accelerate();

            // Braking
            Brake();
            if (brakeInput > 0 && !shouldBrake)
            {
                rewardProvider.AddReward(rewardProvider.rewardConfig.brakingPenalty, "Braking");
            }


            // Steering
            Steer();

            // Rewarding
            CheckFallenOfMap();
            CheckSteeringWithoutGas();
        }

        private void CheckSteeringWithoutGas()
        {
            if(steeringInput != 0 && currentSpeed < 1f)
            {
                rewardProvider.AddReward(rewardProvider.rewardConfig.steeringWithoutMovingPenalty, "Steering without moving");
            }
        }

        public void CheckFallenOfMap()
        {
            if (transform.position.y < -1f) // Fallen off the world
            {
                rewardProvider.SetReward(rewardProvider.rewardConfig.drivingOffRoadPenalty, "Fallen off map");
                rewardProvider.EndEpisode();
            }
        }

        public void ModifyTorque(float multiplier)
        {
            motorTorque = defaultMotorTorque * multiplier;
        }
    }
}