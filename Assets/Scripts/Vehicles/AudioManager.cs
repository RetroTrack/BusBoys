using BusBoys.Assets.Scripts.Core.Graph;
using BusBoys.Assets.Scripts.Sensors.Lidar;
using BusBoys.Assets.Scripts.Vehicles.Common;
using UnityEngine;

namespace BusBoys
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private LidarSensor lidar;
        [SerializeField] private VehicleController vehicle;

        public AudioSource engineSound;
        public AudioSource honkSound;
        float speed;
        public float minSpeed = 0.1f;
        float HonkCooldownTime = 30;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
             
        }

        // Update is called once per frame
        void Update()
        {
            HonkCooldownTime -= Time.deltaTime;

            speed = vehicle.CurrentSpeed;

            if (speed > minSpeed)
            {
                engineSound.volume = 1f;
                engineSound.pitch = 1f + speed * 0.05f;
            }
            else
            {
                engineSound.volume = 0.2f;
            }

            if (lidar.passerbyDetected && HonkCooldownTime <= 0)
            {
                honkSound.volume = 1f;
            }
            else if (!lidar.passerbyDetected)
            {
                honkSound.Stop();
            }
        }
    }
    
}
