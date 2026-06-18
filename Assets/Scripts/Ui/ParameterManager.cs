using BusBoys.Assets.Scripts.Vehicles.Bus.Electric;
using BusBoys.Assets.Scripts.Vehicles.Common;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace BusBoys
{
    public class ParameterManager : MonoBehaviour
    {
        [SerializeField] Slider speedSlider;
        [SerializeField] Slider batteryDrainPerMeterSlider;
        [SerializeField] Slider PasserbyOddsSlider;

        [SerializeField] TextMeshProUGUI speedText;
        [SerializeField] TextMeshProUGUI BatteryText;
        [SerializeField] TextMeshProUGUI PasserbyText;

        [SerializeField] Toggle toggleFrontWheels;
        [SerializeField] Toggle toggleBackWheels;

        [SerializeField] VehicleController Vehicle;
        [SerializeField] BusBattery Battery;
        [SerializeField] List<Crossing> crossings;



        void Start()
        {
            toggleFrontWheels.isOn = true;
            toggleBackWheels.isOn = false;

            setSliderValues(speedSlider, 1f, 45f, 25f);
            setSliderValues(batteryDrainPerMeterSlider, 0.0001f, 0.1f, 0.001f);
            setSliderValues(PasserbyOddsSlider, 0.01f, 1, 0.2f);

        }

        // Update is called once per frame
        void Update()
        {
            updateValues();
            WheelValues();
        }

        private void Awake()
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("Crossing");

            foreach (GameObject obj in taggedObjects)
            {
                Crossing crossing = obj.GetComponent<Crossing>();
                if (crossing != null)
                {
                    crossings.Add(crossing);
                }
            }
        }

        private void updateValues()
        {
            speedText.text = $"MaxSpeed: {speedSlider.value:F2} km/H";
            BatteryText.text = $"Batt-Drain: {batteryDrainPerMeterSlider.value:F5}%/m ";
            PasserbyText.text = $"PasserbyOdds: {PasserbyOddsSlider.value *100:F2}%";

            Battery.drainPerMeter = batteryDrainPerMeterSlider.value;
            Vehicle.maxSpeed = speedSlider.value;
            foreach (Crossing crossing in crossings)
            {
                crossing.passerbyOdds = PasserbyOddsSlider.value;
            }
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
            else if (toggleFrontWheels.isOn == true && toggleBackWheels.isOn == false)
            {
                Vehicle.driveType = DriveType.FrontWheelDrive;
                Vehicle.brakingType = BrakingType.FrontWheelBraking;
                Vehicle.steeringType = SteeringType.FrontWheelSteering;
            }
            else
            {
                toggleFrontWheels.isOn = true;
            }

        }

        public void resetValues()
        {
            toggleFrontWheels.isOn = true;
            toggleBackWheels.isOn = false;
            speedSlider.value = 25;
            batteryDrainPerMeterSlider.value = 0.001f;
            PasserbyOddsSlider.value = 0.20f;

        }

        void setSliderValues(Slider slider, float minValue, float maxValue, float StartValue)
        {
            slider.minValue = minValue; //minimale waarde
            slider.maxValue = maxValue; //maximale waarde
            slider.value = StartValue;  //beginwaarde
        }
    }
}
