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
        [SerializeField] Slider passerbyOddsSlider;
        [SerializeField] Slider turnPenaltyMultiplierSlider;

        [SerializeField] TextMeshProUGUI speedText;
        [SerializeField] TextMeshProUGUI batteryText;
        [SerializeField] TextMeshProUGUI passerbyText;
        [SerializeField] TextMeshProUGUI turnPenaltyMultiplierText;

        [SerializeField] TextMeshProUGUI MonitoringText;

        [SerializeField] Toggle toggleFrontWheels;
        [SerializeField] Toggle toggleBackWheels;

        [SerializeField] Toggle toggleShowRay;

        [SerializeField] VehicleController vehicle;
        [SerializeField] BusBattery battery;
        [SerializeField] List<Crossing> crossings;
        [SerializeField] NavGraph navGraph;
        [SerializeField] PathDrawer pathDrawer;
        [SerializeField] Button resetParButton;

        [SerializeField] TextMeshProUGUI HideShowText;
        private bool ShowUI = true; //true = show, false = hide
        
        private DriveType currentDriveType;
        private DriveType LastDriveType;

        private string uiSpeedText;
        private string uiBatteryText;
        private string uiPasserbyText;
        private string uiTurnPenaltyText;

        //Sets default parameters on starting the simulation.
        void Start()
        {
            //Set all boxes 
            HideShowText.text = "Show";
            toggleFrontWheels.isOn = true;
            toggleBackWheels.isOn = false;
            toggleShowRay.isOn = true;

            //Set slider values to the default values and give them a min and max value.
            SetSliderValues(speedSlider, 1f, 45f, 25f);
            SetSliderValues(batteryDrainPerMeterSlider, 0.0001f, 0.1f, 0.001f);
            SetSliderValues(passerbyOddsSlider, 0.01f, 1, 0.2f);
            SetSliderValues(turnPenaltyMultiplierSlider, 0f , 20f, 2f);

            //Get all values from editor and re-use them in this script.
            uiSpeedText = speedText.text;
            uiBatteryText = batteryText.text;
            uiTurnPenaltyText = turnPenaltyMultiplierText.text;
            uiPasserbyText = passerbyText.text;

        }

        // Update is called once per frame
        void Update()
        {
            //Update slider text.
            UpdateSliderText();
            //Update wheel modes.
            SetVehicleDrivingType();
        }

        //Add all crossing instances to one list.
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

        //Update the slider text in the UI.
        private void UpdateSliderText()
        {
            speedText.text = $"{uiSpeedText}: {speedSlider.value:F2} km/H";
            batteryText.text = $"{uiBatteryText}: {batteryDrainPerMeterSlider.value:F3}%/m ";
            passerbyText.text = $"{uiPasserbyText}: {passerbyOddsSlider.value *100:F2}%";
            turnPenaltyMultiplierText.text = $"{uiTurnPenaltyText}: {turnPenaltyMultiplierSlider.value:F1} x";
            battery.drainPerMeter = batteryDrainPerMeterSlider.value;
            vehicle.maxSpeed = speedSlider.value;
            navGraph.turnPenaltyMultiplier = turnPenaltyMultiplierSlider.value;


            foreach (Crossing crossing in crossings)
            {
                crossing.passerbyOdds = passerbyOddsSlider.value;
            }
        }

        //Set vehicle driving type.
        private void SetVehicleDrivingType()
        {
            if (toggleFrontWheels.isOn == true && toggleBackWheels.isOn == true)
            {
                currentDriveType = DriveType.AllWheelDrive;
                vehicle.driveType = DriveType.AllWheelDrive;
                vehicle.brakingType = BrakingType.AllWheelBraking;
                vehicle.steeringType = SteeringType.AllWheelSteering;
                StraightenWheels();
            }
            else if (toggleFrontWheels.isOn == false && toggleBackWheels.isOn == true)
            {
                currentDriveType = DriveType.RearWheelDrive;
                vehicle.driveType = DriveType.RearWheelDrive;
                vehicle.brakingType = BrakingType.RearWheelBraking;
                vehicle.steeringType = SteeringType.RearWheelSteering;
                StraightenWheels();
            }
            else if (toggleFrontWheels.isOn == true && toggleBackWheels.isOn == false)
            {
                currentDriveType = DriveType.FrontWheelDrive;
                vehicle.driveType = DriveType.FrontWheelDrive;
                vehicle.brakingType = BrakingType.FrontWheelBraking;
                vehicle.steeringType = SteeringType.FrontWheelSteering;
                StraightenWheels();
            }
            else
            {
                toggleFrontWheels.isOn = true;
                StraightenWheels();
            }

        }

        //Resets all values on the push off a button.
        public void ResetValues()
        {
            toggleFrontWheels.isOn = true;
            toggleBackWheels.isOn = false;
            speedSlider.value = 25;
            batteryDrainPerMeterSlider.value = 0.001f;
            passerbyOddsSlider.value = 0.20f;
            turnPenaltyMultiplierSlider.value = 2f;
        }

        //Sets all slider values with a min max and start value.
        void SetSliderValues(Slider slider, float minValue, float maxValue, float StartValue)
        {
            slider.minValue = minValue; //minimale waarde
            slider.maxValue = maxValue; //maximale waarde
            slider.value = StartValue;  //beginwaarde
        }

        //Straightens the wheels 
        void StraightenWheels()
        {
            if(LastDriveType != currentDriveType)
            {
                if (currentDriveType == DriveType.RearWheelDrive)
                {
                    vehicle.SetWheelAngle(0f, WheelType.Front);
                }
                else if (currentDriveType == DriveType.FrontWheelDrive)
                {
                    vehicle.SetWheelAngle(0f, WheelType.Rear);
                }

            }
            LastDriveType = currentDriveType;
        }

        //Toggle UI visuals.
        public void HideAndShowButton()
        {
            ShowUI = !ShowUI;
            SetUi(ShowUI);
            HideShowText.text = ShowUI ? "HideUI" : "ShowUI";
        }

        //Toggle UI. Sets all UI elements active or inactive. To make sure they are visible or not.
        void SetUi(bool set) {
            speedSlider.gameObject.SetActive(set);
            batteryDrainPerMeterSlider.gameObject.SetActive(set);
            passerbyOddsSlider.gameObject.SetActive(set);
            speedText.gameObject.SetActive(set);
            batteryText.gameObject.SetActive(set);
            passerbyText.gameObject.SetActive(set);
            toggleFrontWheels.gameObject.SetActive(set);
            toggleBackWheels.gameObject.SetActive(set);
            resetParButton.gameObject.SetActive(set);
            MonitoringText.gameObject.SetActive(set);
            turnPenaltyMultiplierSlider.gameObject.SetActive(set);
            turnPenaltyMultiplierText.gameObject.SetActive(set);
            toggleShowRay.gameObject.SetActive(set);

        }

        //Set line rendering in game on or off.
        public void SetRouteRay()
        {
           pathDrawer.renderLinesInGame = toggleShowRay.isOn;
        }
    }
}
