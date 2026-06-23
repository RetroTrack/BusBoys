using BusBoys.Assets.Scripts.ML.Observations;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Vehicles.Common
{
    public class VehicleBootstrap : MonoBehaviour, IObservationSource
    {
        public VehicleController vehicleController;
        
        //Runs on the beginning of a new AI training episode. And gives it random types to train on.
        public void BeginEpisode()
        {
            if (vehicleController == null)
                return;

            vehicleController.brakingType = RandomEnum<BrakingType>();
            vehicleController.driveType = RandomEnum<DriveType>();
            vehicleController.steeringType = RandomEnum<SteeringType>();
        }

        //Gets an random enum value.
        private static T RandomEnum<T>() where T : System.Enum
        {
            var values = System.Enum.GetValues(typeof(T));
            return (T)values.GetValue(Random.Range(0, values.Length));
        }

        //NormalizeEnum value.
        private static float NormalizeEnum<T>(T value) where T : System.Enum
        {
            return System.Convert.ToInt32(value) /
                   (float)(System.Enum.GetValues(typeof(T)).Length - 1);
        }

        //Collect observations to return to the AI.
        public void Collect(VectorSensor sensor)
        {
            if (vehicleController == null)
                return;
            sensor.AddObservation(NormalizeEnum(vehicleController.brakingType));
            sensor.AddObservation(NormalizeEnum(vehicleController.driveType));
            sensor.AddObservation(NormalizeEnum(vehicleController.steeringType));
        }
    }
}
