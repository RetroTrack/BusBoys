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
            VehicleStats.text = 
                $"Position: {Battery.currentPosition}\n" +
                $"Battery: {Battery.batteryPercentage:F2} \n" +
                $"Speed: {Controller.CurrentSpeed:F2} Km/h \n" +
                $"SteeringAngle: {Controller.CurrentSteerAngle:F2}°  \n";

        }
    }
}