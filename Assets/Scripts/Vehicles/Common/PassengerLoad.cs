using System.Collections.Generic;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Vehicles.Common
{
    public class PassengerLoad : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;

        [Header("Bus Settings")]
        [SerializeField] private float emptyBusMass = 2800f;
        [SerializeField] private float averagePassengerMass = 75f;
        [SerializeField] private int maxCapacity = 6;

        [Header("Zitplaats gebied (local space)")]
        [SerializeField] private Vector3 seatingAreaMin = new Vector3(-1.2f, 0.5f, -5f);
        [SerializeField] private Vector3 seatingAreaMax = new Vector3(1.2f, 0.5f, 5f);

        [Header("Center of Mass")]
        [SerializeField] private Vector3 emptyCenterOfMass = new Vector3(0f, 0.5f, 0f);

        private readonly List<Vector3> _passengerPositions = new List<Vector3>();

        public int PassengerCount => _passengerPositions.Count;
        public bool IsFull => PassengerCount >= maxCapacity;

        private void Awake()
        {
            rb.centerOfMass = emptyCenterOfMass;
        }

        // TODO: koppel aan de instap-trigger (deur, NPC, etc.)
        public void BoardPassenger()
        {
            if (IsFull) return;

            var seat = new Vector3(
                Random.Range(seatingAreaMin.x, seatingAreaMax.x),
                Random.Range(seatingAreaMin.y, seatingAreaMax.y),
                Random.Range(seatingAreaMin.z, seatingAreaMax.z)
            );

            _passengerPositions.Add(seat);
            UpdatePhysics();
        }

        // TODO: koppel aan de uitstap-trigger (deur, NPC, etc.)
        public void AlightPassenger()
        {
            if (PassengerCount <= 0) return;

            int randomIndex = Random.Range(0, PassengerCount);
            _passengerPositions.RemoveAt(randomIndex);
            UpdatePhysics();
        }

        private void UpdatePhysics()
        {
            UpdateMass();
            UpdateCenterOfMass();
        }

        private void UpdateMass()
        {
            rb.mass = emptyBusMass + (PassengerCount * averagePassengerMass);
        }

        private void UpdateCenterOfMass()
        {
            if (PassengerCount == 0)
            {
                rb.centerOfMass = emptyCenterOfMass;
                return;
            }

            Vector3 passengerCOM = Vector3.zero;
            foreach (var pos in _passengerPositions)
                passengerCOM += pos;
            passengerCOM /= PassengerCount;

            float passengerMass = PassengerCount * averagePassengerMass;
            rb.centerOfMass = (emptyCenterOfMass * emptyBusMass + passengerCOM * passengerMass) / rb.mass;
        }
    }
}