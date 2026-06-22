using BusBoys.Assets.Scripts.Sensors.CollisionReward;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Environment.Hitboxes
{
    public class PedestrianHitbox : MonoBehaviour
    {
        private CollisionRewarder busCollisionRewarder;

        public void FixedUpdate()
        {
            if (busCollisionRewarder != null)
            {
                busCollisionRewarder.HitPedestrian();

            }
        }
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

        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Bus"))
            {
                busCollisionRewarder = null;
            }
        }
    }
}
