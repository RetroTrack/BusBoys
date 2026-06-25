using BusBoys.Assets.Scripts.Sensors.CollisionReward;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Environment.Hitboxes
{
    public class PedestrianHitbox : MonoBehaviour
    {
        private CollisionRewarder busCollisionRewarder;

        //To make sure the bus does not hit pedestrian. And giving panelty if bus is hitting a pedestrian.
        public void FixedUpdate()
        {
            if (busCollisionRewarder != null)
            {
                busCollisionRewarder.HitPedestrian();

            }
        }

        //On enter of an collision and collider is bus. Set the rewarder to the bus rewarder
        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bus"))
            {
                var collisionRewarder = other.GetComponentInChildren<CollisionRewarder>();
                if (collisionRewarder != null)
                {
                    busCollisionRewarder = collisionRewarder;
                }
            }
        }

        //On exit of an collision and collider is bus. Set reward to null
        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Bus"))
            {
                busCollisionRewarder = null;
            }
        }
    }
}
