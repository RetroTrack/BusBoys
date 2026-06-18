using BusBoys.Assets.Scripts.Sensors.Lidar;
using BusBoys.Assets.Scripts.Vehicles.Bus.Electric;
using BusBoys.Assets.Scripts.Vehicles.Common;
using TMPro;
using UnityEngine;
using static BusBoys.Assets.Scripts.Sensors.Lidar.LidarSensor;

namespace BusBoys
{
    public class Monitoring : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public VehicleController Controller;
        public BusBattery Battery;
        public TextMeshProUGUI VehicleStats;
        public LidarSensor Lidar;
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (Lidar.passerbyDetected) {
                VehicleStats.text = 
                $"Position: {Battery.currentPosition}\n" +
                $"Battery: {Battery.batteryPercentage:F2}% \n" +
                $"Speed: {Controller.CurrentSpeed:F2} Km/h \n" +
                $"SteeringAngle: {Controller.CurrentSteerAngle:F2}°  \n" +
                $"Lidar: Voetganger Gedetecteerd!!";
            }
            else
            {
                VehicleStats.text =
                $"Position: {Battery.currentPosition}\n" +
                $"Battery: {Battery.batteryPercentage:F2}% \n" +
                $"Speed: {Controller.CurrentSpeed:F2} Km/h \n" +
                $"SteeringAngle: {Controller.CurrentSteerAngle:F2}°  \n" +
                $"Lidar: Geen voetganger Gedetecteerd";
            }



        }
    }
}