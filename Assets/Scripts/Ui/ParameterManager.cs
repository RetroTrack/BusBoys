using BusBoys.Assets.Scripts.Vehicles.Bus.Electric;
using BusBoys.Assets.Scripts.Vehicles.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace BusBoys
{
    public class ParameterManager : MonoBehaviour
    {
        public Slider speedSlider;
        public Slider batteryDrainPerMeterSlider;

        public TextMeshProUGUI speedText;
        public TextMeshProUGUI BatteryText;

        public Toggle toggleFrontWheels;
        public Toggle toggleBackWheels;

        public VehicleController Vehicle;
        public BusBattery Battery;




        void Start()
        {
            toggleFrontWheels.isOn = true;
            toggleBackWheels.isOn = false;

            speedSlider.minValue = 1f;
            speedSlider.maxValue = 45;
            speedSlider.value = 25; // beginwaard

            batteryDrainPerMeterSlider.minValue = 0.0001f; //marge van 12
            batteryDrainPerMeterSlider.maxValue = 0.1f;
            batteryDrainPerMeterSlider.value = 0.001f;

        }

        // Update is called once per frame
        void Update()
        {
            updateValues();
            WheelValues();
        }

        private void updateValues()
        {
            speedText.text = $"MaxSpeed: {speedSlider.value:F2} km/H" ;
            BatteryText.text = $"Batt-Drain: {batteryDrainPerMeterSlider.value:F5}%/m ";

            Battery.drainPerMeter = batteryDrainPerMeterSlider.value;
            Vehicle.maxSpeed = speedSlider.value;
        }

        private void WheelValues()
        {
            if (toggleFrontWheels.isOn == true && toggleBackWheels.isOn == true)
            {
                Vehicle.driveType = DriveType.AllWheelDrive;
                Vehicle.brakingType = BrakingType.AllWheelBraking;
                Vehicle.steeringType = SteeringType.AllWheelSteering;
            }
            else if (toggleFrontWheels.isOn == false && toggleBackWheels.isOn == true)
            {
                Vehicle.driveType = DriveType.RearWheelDrive;
                Vehicle.brakingType = BrakingType.RearWheelBraking;
                Vehicle.steeringType = SteeringType.RearWheelSteering;
            }
            else //if ((toggleFrontWheels == true && toggleBackWheels == false) || (toggleFrontWheels == false && toggleBackWheels == false))//als beide false zijn gaat die ook naar front wheel drive 
            {
                Vehicle.driveType = DriveType.FrontWheelDrive;
                Vehicle.brakingType = BrakingType.FrontWheelBraking;
                Vehicle.steeringType = SteeringType.FrontWheelSteering;
            }

        }

        public void resetValues()
        {
            toggleFrontWheels.isOn = true;
            toggleBackWheels.isOn = false;
            speedSlider.value = 25;
            batteryDrainPerMeterSlider.value = 0.001f;

        }
    }
}
