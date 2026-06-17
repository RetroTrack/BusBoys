using System.Collections.Generic;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Vehicles.Common
{
    public abstract class VehicleController : MonoBehaviour
    {
        [Header("Vehicle Components")]
        [SerializeField] private Rigidbody rb;

        [Header("Wheels")]
        [SerializeField] private List<WheelCollider> frontWheelColliders = new List<WheelCollider>();
        [SerializeField] private List<WheelCollider> rearWheelColliders = new List<WheelCollider>();
        [SerializeField] private List<Transform> frontWheelMeshes = new List<Transform>();
        [SerializeField] private List<Transform> rearWheelMeshes = new List<Transform>();

        [Header("Vehicle Settings")]
        [Tooltip("Amount of torque applied to the wheels (in Nm)"), SerializeField] protected float motorTorque = 2000f;
        [Tooltip("Amount of torque applied when braking (in Nm)"), SerializeField] protected float brakeTorque = 5000f;
        [Tooltip("Maximum steering angle (in degrees)"), SerializeField] protected float maxSteerAngle = 38f;
        [Tooltip("Speed of steering response"), SerializeField] protected float steeringSpeed = 100f;
        [Tooltip("Maximum speed of the bus (in km/h)"), SerializeField] public float maxSpeed = 45f;
        [Space]
        [Tooltip("Drive type (front, rear, awd)"), SerializeField] public DriveType driveType = DriveType.FrontWheelDrive;
        [Tooltip("Steering type (front, rear, both)"), SerializeField] public SteeringType steeringType = SteeringType.FrontWheelSteering;
        [Tooltip("Braking type (front, rear, both)"), SerializeField] public BrakingType brakingType = BrakingType.FrontWheelBraking;


        [Header("Runtime Data")]
        [SerializeField] protected float currentSpeed = 0f;
        [SerializeField] protected float currentSteerAngle = 0f;

        protected float motorInput, brakeInput, steeringInput;
        public float CurrentSpeedNormalized => currentSpeed / maxSpeed;

        public float CurrentSpeed => currentSpeed; //zodat de ui de currentspeed en steer angle uit kan lezen 
        public float CurrentSteerAngle => currentSteerAngle;
        public virtual void FixedUpdate()
        {
            // Measuring
            currentSpeed = rb.linearVelocity.magnitude * 3.6f; // Convert m/s to km/h
        }

        public void Accelerate()
        {
            float outputMotorTorque = motorInput * motorTorque;

            // Speed Limiting
            float targetSpeed = maxSpeed;
            if (currentSpeed > targetSpeed)
            {
                outputMotorTorque = 0f; // Stop applying torque if over speed limit
            }

            switch (driveType)
            {
                case DriveType.FrontWheelDrive:
                    SetWheelTorque(outputMotorTorque, WheelType.Front);
                    break;
                case DriveType.RearWheelDrive:
                    SetWheelTorque(outputMotorTorque, WheelType.Rear);
                    break;
                default:
                    SetWheelTorque(outputMotorTorque, WheelType.Both);
                    break;
            }
        }
        public void Steer()
        {
            float targetSteerAngle = steeringInput * maxSteerAngle;

            // Smoothly transition to the target steer angle
            currentSteerAngle = Mathf.MoveTowards(
                currentSteerAngle,
                targetSteerAngle,
                steeringSpeed * Time.fixedDeltaTime
            );

            switch (steeringType)
            {
                case SteeringType.FrontWheelSteering:
                    SetWheelAngle(currentSteerAngle, WheelType.Front);
                    break;
                case SteeringType.RearWheelSteering:
                    SetWheelAngle(currentSteerAngle, WheelType.Rear);
                    break;
                default:
                    SetWheelAngle(currentSteerAngle, WheelType.Both);
                    break;
            }
        }
        public void Brake()
        {

            float outputBreakTorque = Mathf.Max(0f, brakeInput) * brakeTorque;
            switch (brakingType)
            {
                case BrakingType.FrontWheelBraking:
                    SetWheelBrake(outputBreakTorque, WheelType.Front);
                    break;
                case BrakingType.RearWheelBraking:
                    SetWheelBrake(outputBreakTorque, WheelType.Rear);
                    break;
                default:
                    SetWheelBrake(outputBreakTorque, WheelType.Both);
                    break;
            }
        }
        public void SetInputs(float motorInput, float brakeInput, float steeringInput)
        {
            this.motorInput = Mathf.Clamp(motorInput, -1f, 1f);
            this.brakeInput = Mathf.Clamp(brakeInput, 0f, 1f);
            this.steeringInput = Mathf.Clamp(steeringInput, -1f, 1f);
        }
        public void ResetVehicle()
        {
            if (rb == null)
            {
                Debug.LogError("Rigidbody reference is missing. Cannot reset vehicle.");
                return;
            }
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            motorInput = 0f;
            brakeInput = 0f;
            steeringInput = 0f;
            currentSpeed = 0f;
            currentSteerAngle = 0f;
            ResetWheels(WheelType.Both);

        }

        protected void ResetWheels(WheelType wheelType)
        {
            SetWheelBrake(0f, wheelType);
            SetWheelTorque(0f, wheelType);
            SetWheelAngle(0f, wheelType);
        }

        protected void SetWheelBrake(float torque, WheelType wheelType)
        {
            switch (wheelType)
            {
                case WheelType.Front:
                    foreach (var wheel in frontWheelColliders)
                    {
                        wheel.brakeTorque = torque;
                    }
                    break;
                case WheelType.Rear:
                    foreach (var wheel in rearWheelColliders)
                    {
                        wheel.brakeTorque = torque;
                    }
                    break;
                case WheelType.Both:
                    SetWheelBrake(torque, WheelType.Front);
                    SetWheelBrake(torque, WheelType.Rear);
                    break;
            }
        }

        protected void SetWheelTorque(float torque, WheelType wheelType)
        {
            switch (wheelType)
            {
                case WheelType.Front:
                    foreach (var wheel in frontWheelColliders)
                    {
                        wheel.motorTorque = torque;
                    }
                    break;
                case WheelType.Rear:
                    foreach (var wheel in rearWheelColliders)
                    {
                        wheel.motorTorque = torque;
                    }
                    break;
                case WheelType.Both:
                    SetWheelTorque(torque, WheelType.Front);
                    SetWheelTorque(torque, WheelType.Rear);
                    break;
            }
        }

        protected void SetWheelAngle(float angle, WheelType wheelType)
        {
            switch (wheelType)
            {
                case WheelType.Front:
                    foreach (var wheel in frontWheelColliders)
                    {
                        wheel.steerAngle = angle;
                    }
                    foreach (var wheelMesh in frontWheelMeshes)
                    {
                        wheelMesh.localRotation = Quaternion.Euler(0f, angle, 0f);
                    }
                    break;
                case WheelType.Rear:
                    foreach (var wheel in rearWheelColliders)
                    {
                        wheel.steerAngle = angle;
                    }
                    foreach (var wheelMesh in rearWheelMeshes)
                    {
                        wheelMesh.localRotation = Quaternion.Euler(0f, angle, 0f);
                    }
                    break;
                case WheelType.Both:
                    SetWheelAngle(angle, WheelType.Front);
                    SetWheelAngle(-angle, WheelType.Rear);
                    break;
            }
        }

        public float[] GetWheelEncoderValues(WheelType wheelType)
        {
            List<float> encoderValues = new List<float>();
            switch (wheelType)
            {
                case WheelType.Front:
                    foreach (var wheel in frontWheelColliders)
                    {
                        encoderValues.Add(wheel.rpm);
                    }
                    break;
                case WheelType.Rear:
                    foreach (var wheel in rearWheelColliders)
                    {
                        encoderValues.Add(wheel.rpm);
                    }
                    break;
                case WheelType.Both:
                    foreach (var wheel in frontWheelColliders)
                    {
                        encoderValues.Add(wheel.rpm);
                    }
                    foreach (var wheel in rearWheelColliders)
                    {
                        encoderValues.Add(wheel.rpm);
                    }
                    break;
            }
            return encoderValues.ToArray();
        }
    }
}
