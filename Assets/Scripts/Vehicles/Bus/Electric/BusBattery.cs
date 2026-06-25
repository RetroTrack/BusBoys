using BusBoys.Assets.Scripts.ML.Observations;
using BusBoys.Assets.Scripts.ML.Rewards;
using System;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Vehicles.Bus.Electric
{
    public class BusBattery : MonoBehaviour, IObservationSource
    {
        [SerializeField] BusController busController;
        [SerializeField] AgentRewardProvider rewardProvider;


        public float drainPerMeter = 0.001f; // hoeveel % per meter
        private float drainSpeedThreshold = 0.01f; // minimale snelheid om te beginnen met ontladen (Normalized speed)
        public float batteryStartPercentage = 100f;
        public float batteryPercentage = 100f;
        private Vector3 lastPosition;
        public Vector3 currentPosition;
        
        //Reset the battery percentage.
        public void ResetBattery()
        {
            batteryPercentage = batteryStartPercentage;
        }

        public void FixedUpdate()
        {
            //Battery
            currentPosition = busController.transform.position;
            float distance = Vector3.Distance(currentPosition, lastPosition);
            if (busController.CurrentSpeedNormalized > drainSpeedThreshold)
            {
                batteryPercentage -= distance * drainPerMeter;
            }
            lastPosition = currentPosition;

            if (batteryPercentage <= 0f) //als de batterij leeg is kan de bus niet meer bewegen
            {
                batteryPercentage = 0f;
                busController.ModifyTorque(0f); // Zet motor torque op 0
                rewardProvider.AddReward(rewardProvider.rewardConfig.batteryDepletedPenalty, "Battery depleted");
                rewardProvider.EndEpisode();
            }
            else
            {
                busController.ModifyTorque(1f);
            }
        }

        //Collect information and pass observation.
        public void Collect(VectorSensor sensor)
        {
            sensor.AddObservation(batteryPercentage/batteryStartPercentage);
        }

        //Logic for rewards on charging the battery
        public void ChargeBattery(float chargeRate)
        {
            if (batteryPercentage < batteryStartPercentage)
            {
                batteryPercentage += (chargeRate * Time.deltaTime);
                rewardProvider.AddReward(rewardProvider.rewardConfig.batteryChargingReward, "Battery charging");
            }
            if (batteryPercentage >= batteryStartPercentage)
            {
                batteryPercentage = batteryStartPercentage;
                rewardProvider.AddReward(rewardProvider.rewardConfig.batteryOverflowReward, "Battery full");
            }
        }
    }
}
