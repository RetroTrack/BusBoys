using UnityEngine;
namespace BusBoys.Assets.Scripts.Vehicles.Bus.Electric
{
    public class BatteryCharger : MonoBehaviour
    {
        public float chargeRate = 20f; // % per seconde

        private void OnTriggerStay(Collider other)
        {
            BusBattery battery = other.GetComponent<BusBattery>();

            if (battery != null && battery.batteryPercentage < 100)
            {
                battery.batteryPercentage += (chargeRate * Time.deltaTime);
                Debug.Log("Auto laad op!!");
            }
            if (battery != null && battery.batteryPercentage == 100)
            {
                Debug.Log("Auto is vol!!");
            }
        }
    }
}

//zorg dat op de laadpaal in unity isTrigger aangevinkt is en de collider groot genoeg staat.