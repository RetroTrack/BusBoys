using BusBoys.Assets.Scripts.Sensors.CollisionReward;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Environment.Hitboxes
{
    public class PavementHitbox : MonoBehaviour 
    {
        private CollisionRewarder busCollisionRewarder;

        //To make sure the bus stays on the pavement. And giving panelty if bus is hitting pavement.
        public void FixedUpdate()
        {
            if (busCollisionRewarder != null)
            {
                busCollisionRewarder.StayOnPavement();
            }
        }

        //If the bus hits the pavement. The collision rewarder is added.
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

        //If the bus stops hitting pavement. Remove the rewarder instance.
        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Bus"))
            {
                busCollisionRewarder = null;
            }
        }
    }
}
