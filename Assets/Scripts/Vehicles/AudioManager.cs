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
        float minSpeed = 0.5f;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            engineSound.Play();
            engineSound.volume = 0f;
        }
        void Update()
        {
            if (lidar.passerbyDetected)
            {
                engineSound.volume = 0.3f;
                honkSound.volume = 1f;
                if (!honkSound.isPlaying)  // voorkom dat het geluid steeds opnieuw begint
                {
                    honkSound.Play();
                }
            }
            else
            {
                honkSound.Stop();
                if (speed > minSpeed && !engineSound.isPlaying)
                {
                    engineSound.volume = 1f;
                    engineSound.pitch = 1f + speed * 0.05f;
                }
            }

        }

    }
}