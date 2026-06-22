using UnityEngine;
namespace BusBoys.Assets.Scripts.Vehicles.Bus.Electric
{
    public class BatteryCharger : MonoBehaviour
    {
        public float chargeRate = 20f; // % per seconde
        public Transform target;

        private void OnTriggerStay(Collider other)
        {
            if(!other.CompareTag("Bus")) return;
            BusBattery battery = other.GetComponentInChildren<BusBattery>();
            if (battery == null) return;
            battery.ChargeBattery(chargeRate * Time.deltaTime);
        }
    }
}

//zorg dat op de laadpaal in unity isTrigger aangevinkt is en de collider groot genoeg staat.