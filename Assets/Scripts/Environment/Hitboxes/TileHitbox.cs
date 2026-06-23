using BusBoys.Assets.Scripts.Sensors.CollisionReward;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Environment.Hitboxes
{
    public class TileHitbox : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bus"))
            {
                var collisionRewarder = other.GetComponentInChildren<CollisionRewarder>();
                if (collisionRewarder != null)
                {
                    collisionRewarder.DriveOffRoad();
                }
            }
        }
    }
}
