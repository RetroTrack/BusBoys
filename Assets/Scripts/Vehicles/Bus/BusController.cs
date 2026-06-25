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


        //Set the default torque on start.
        public void Start()
        {
            defaultMotorTorque = motorTorque;
        }

        //If the bus should be braking at that moment or not.
        public void SetShouldBrake(bool shouldBrake)
        {
            this.shouldBrake = shouldBrake;
        }

        //Update all bus components.
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

        //Giving penalty if steering stationary.
        private void CheckSteeringWithoutGas()
        {
            if(steeringInput != 0 && currentSpeed < 1f)
            {
                rewardProvider.AddReward(rewardProvider.rewardConfig.steeringWithoutMovingPenalty, "Steering without moving");
            }
        }

        //Check if bus has fallen of the map
        public void CheckFallenOfMap()
        {
            if (transform.position.y < -1f) // Fallen off the world
            {
                rewardProvider.SetReward(rewardProvider.rewardConfig.drivingOffRoadPenalty, "Fallen off map");
                rewardProvider.EndEpisode();
            }
        }

        //Change the torque modifier.
        public void ModifyTorque(float multiplier)
        {
            motorTorque = defaultMotorTorque * multiplier;
        }
    }
}