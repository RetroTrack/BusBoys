using System;
using BusBoys.Assets.Scripts.Vehicles.Bus.Electric;
using BusBoys.Assets.Scripts.Vehicles.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusBoys
{
    public class Monitoring : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public VehicleController Controller;
        public BusBattery Battery;
        public TextMeshProUGUI VehicleStats;
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            VehicleStats.text = $"Position: {Battery.currentPosition}\n Battery: {Battery.batteryPercentage:F2} \n Speed: {Controller.CurrentSpeed:F2} Km/h \n SteeringAngle: {Controller.CurrentSteerAngle:F2}°  \n";

        }
    }
}
/*
 * in vehicle controller plakken
         public float CurrentSpeed => currentSpeed; //zodat de ui de currentspeed en steer angle uit kan lezen 
        public float CurrentSteerAngle => currentSteerAngle;
 */

/*
             var pos = Battery.currentPosition;
            var battPer = Math.Round(Battery.batteryPercentage, 2);
            var currSpeed = Math.Round(Controller.CurrentSpeed,2);
            var steerAng = Math.Round(Controller.CurrentSteerAngle,2);
            //VehicleStats.text = $"Position: {pos}\n Battery: {battPer} \n Speed: {currSpeed} Km/h \n SteeringAngle: {steerAng}°  \n";

 */