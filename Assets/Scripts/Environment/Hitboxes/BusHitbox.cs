using BusBoys.Assets.Scripts.Sensors.CollisionReward;
using BusBoys.Assets.Scripts.Vehicles.Bus;
using BusBoys.Assets.Scripts.Vehicles.Common;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Environment.Hitboxes
{
    public class BusHitbox : MonoBehaviour
    {
        [SerializeField] private BusController busController;
        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Pedestrian"))
            {
                busController.SetShouldBrake(true);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Pedestrian"))
            {
                busController.SetShouldBrake(false);
            }
        }
    }
}
