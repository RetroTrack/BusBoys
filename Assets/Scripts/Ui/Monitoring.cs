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
        public VehicleController Controller;
        public BusBattery Battery;
        public TextMeshProUGUI VehicleStats;
        public LidarSensor Lidar;

        // Update is called once per frame, calls updates for the stats on the left.
        void Update()
        {
            //Sets the stats on the left side of the UI. To display the current stats of the vehicle.
            VehicleStats.text = 
            $"Position: \n{Battery.currentPosition}\n\n" +
            $"Battery: \n{Battery.batteryPercentage:F2}% \n\n" +
            $"Speed: \n{Controller.CurrentSpeed:F2} Km/h \n\n" +
            $"SteeringAngle: \n{Controller.CurrentSteerAngle:F2}°  \n\n" + 
            (Lidar.passerbyDetected ? $"Lidar: \nGeen voetganger Gedetecteerd" : $"Lidar: \nVoetganger Gedetecteerd!!");
        }
    }
}