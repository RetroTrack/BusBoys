using BusBoys.Assets.Scripts.Sensors.CollisionReward;
using BusBoys.Assets.Scripts.Vehicles.Bus;
using BusBoys.Assets.Scripts.Vehicles.Common;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Environment.Hitboxes
{
    public class BusHitbox : MonoBehaviour
    {
        [SerializeField] private BusController busController;

        //Checks if the entered collider is a pedestrian
        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Pedestrian"))
            {
                busController.SetShouldBrake(true);
            }
        }

        //Checks if the exit collider is a pedestrian so it can start driving again.
        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Pedestrian"))
            {
                busController.SetShouldBrake(false);
            }
        }
    }
}
