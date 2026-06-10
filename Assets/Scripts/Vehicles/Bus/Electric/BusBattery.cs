using UnityEngine;

namespace BusBoys.Assets.Scripts.Vehicles.Bus.Electric
{
    public class BusBattery : MonoBehaviour
    {
        [SerializeField] BusController busController;


        [SerializeField] private float drainPerMeter = 0.001f; // hoeveel % per meter
        [SerializeField] private float drainSpeedThreshold = 0.01f; // minimale snelheid om te beginnen met ontladen (Normalized speed)
        public float batteryPercentage = 100f;
        private Vector3 lastPosition;

        public void FixedUpdate()
        {
            //Battery:
            Vector3 currentPosition = busController.transform.position;
            float distance = Vector3.Distance(currentPosition, lastPosition);
            if (busController.CurrentSpeedNormalized > drainSpeedThreshold)
            {
                batteryPercentage -= distance * drainPerMeter;
            }
            lastPosition = currentPosition;

            if (batteryPercentage <= 0f) //als de batterij leeg is kan de bus niet meer bewegen
            {
                batteryPercentage = 0f;
                busController.ModifyTorque(0f); // Zet motor torque op 0
            }
            else
            {
                busController.ModifyTorque(1f);
            }
        }
    }
}
