using UnityEngine;
using UnityEngine.UI;
using BusBoys.Assets.Scripts.Vehicles.Common;
using BusBoys.Assets.Scripts.Vehicles.Bus.Electric;
using TMPro;

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
            VehicleStats.text = $"Position: {Battery.currentPosition}\n Battery: {Battery.batteryPercentage} \n Speed: {Controller.CurrentSpeed} Km/h \n SteeringAngle: {Controller.CurrentSteerAngle}°  \n";

        }
    }
}
/*
 * in vehicle controller plakken
         public float CurrentSpeed => currentSpeed; //zodat de ui de currentspeed en steer angle uit kan lezen 
        public float CurrentSteerAngle => currentSteerAngle;
 */