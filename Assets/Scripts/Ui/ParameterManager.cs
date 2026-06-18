using BusBoys.Assets.Scripts.Core.Graph;
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
        [SerializeField] Slider turnPenaltyMultiplierSlider;

        [SerializeField] TextMeshProUGUI speedText;
        [SerializeField] TextMeshProUGUI BatteryText;
        [SerializeField] TextMeshProUGUI PasserbyText;
        [SerializeField] TextMeshProUGUI turnPenaltyMultiplierText;

        [SerializeField] TextMeshProUGUI MonitoringText;

        [SerializeField] Toggle toggleFrontWheels;
        [SerializeField] Toggle toggleBackWheels;

        [SerializeField] Toggle toggleShowRay;

        [SerializeField] VehicleController Vehicle;
        [SerializeField] BusBattery Battery;
        [SerializeField] List<Crossing> crossings;
        [SerializeField] NavGraph navGraph;

        [SerializeField] Button resetParButton;

        [SerializeField] TextMeshProUGUI HideShowText;
        private bool ShowUI = true; //true = show, false = hide
        
        private DriveType currentDriveType;
        private DriveType LastDriveType;

        void Start()
        {
            HideShowText.text = "Show";
            toggleFrontWheels.isOn = true;
            toggleBackWheels.isOn = false;

            setSliderValues(speedSlider, 1f, 45f, 25f);
            setSliderValues(batteryDrainPerMeterSlider, 0.0001f, 0.1f, 0.001f);
            setSliderValues(PasserbyOddsSlider, 0.01f, 1, 0.2f);
            setSliderValues(turnPenaltyMultiplierSlider, 0f , 20f, 2f);

        }

        // Update is called once per frame
        void Update()
        {
            updateValues();
            WheelValues();
            CheckRouteRay(); // nog gemaakt
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
            turnPenaltyMultiplierText.text = $"TunPenaltyMulti: {batteryDrainPerMeterSlider.value:F2} x";
            Battery.drainPerMeter = batteryDrainPerMeterSlider.value;
            Vehicle.maxSpeed = speedSlider.value;
            navGraph.turnPenaltyMultiplier = turnPenaltyMultiplierSlider.value;


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
                currentDriveType = DriveType.AllWheelDrive;
                straightWheels();
            }
            else if (toggleFrontWheels.isOn == false && toggleBackWheels.isOn == true)
            {
                currentDriveType = DriveType.RearWheelDrive;
                Vehicle.driveType = DriveType.RearWheelDrive;
                Vehicle.brakingType = BrakingType.RearWheelBraking;
                Vehicle.steeringType = SteeringType.RearWheelSteering;
                straightWheels();
            }
            else if (toggleFrontWheels.isOn == true && toggleBackWheels.isOn == false)
            {
                currentDriveType = DriveType.FrontWheelDrive;
                Vehicle.driveType = DriveType.FrontWheelDrive;
                Vehicle.brakingType = BrakingType.FrontWheelBraking;
                Vehicle.steeringType = SteeringType.FrontWheelSteering;
                straightWheels();
            }
            else
            {
                toggleFrontWheels.isOn = true;
                straightWheels();
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

        void straightWheels()
        {
            if(LastDriveType != currentDriveType)
            {
                //Debug.Log($"last: {LastDriveType} Now:{currentDriveType}");
                if (currentDriveType == DriveType.RearWheelDrive)
                {
                    Vehicle.SetWheelAngle(0f, WheelType.Front);
                }
                else if (currentDriveType == DriveType.FrontWheelDrive)
                {
                    Vehicle.SetWheelAngle(0f, WheelType.Rear);
                }

            }
            LastDriveType = currentDriveType;
        }

        public void HideAndShowButton()
        {
            ShowUI = !ShowUI;
            setUi(ShowUI);
            HideShowText.text = ShowUI ? "HideUI" : "ShowUI";
        }
        void setUi(bool set) {
            speedSlider.gameObject.SetActive(set);
            batteryDrainPerMeterSlider.gameObject.SetActive(set);
            PasserbyOddsSlider.gameObject.SetActive(set);
            speedText.gameObject.SetActive(set);
            BatteryText.gameObject.SetActive(set);
            PasserbyText.gameObject.SetActive(set);
            toggleFrontWheels.gameObject.SetActive(set);
            toggleBackWheels.gameObject.SetActive(set);
            resetParButton.gameObject.SetActive(set);
            MonitoringText.gameObject.SetActive(set);
            turnPenaltyMultiplierSlider.gameObject.SetActive(set);
            turnPenaltyMultiplierText.gameObject.SetActive(set);
            toggleShowRay.gameObject.SetActive(set);

        }
        void CheckRouteRay()
        {
           if(toggleShowRay.isOn == true)
            {
                //show ray
            }
           else
            {
                //dont show
            }
        }
    }
}
