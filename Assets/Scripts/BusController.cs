using UnityEngine;
using UnityEngine.InputSystem;

public class BusController : MonoBehaviour
{
    [Header("Bus Components")]
    [SerializeField] private Rigidbody rb;
    public WheelCollider wheelFrontLeft, wheelFrontRight, wheelBackLeft, wheelBackRight; //public zodat de trafficDetector de wielen op kan halen
    [SerializeField] private Transform wheelFrontLeftMesh, wheelFrontRightMesh, wheelBackLeftMesh, wheelBackRightMesh;

    [Header("Bus Settings")]
    [Tooltip("Amount of torque applied to the wheels (in Nm)"), SerializeField] private float motorTorque = 2000f;
    [Tooltip("Amount of torque applied when braking (in Nm)"), SerializeField] private float brakeTorque = 5000f;
    [Tooltip("Maximum steering angle (in degrees)"), SerializeField] private float maxSteerAngle = 38f;
    [Tooltip("Speed of steering response"), SerializeField] private float steeringSpeed = 100f;
    [Tooltip("Maximum speed of the bus (in km/h)"), SerializeField] private float maxSpeed = 45f;
    [Tooltip("Normal operating speed of the bus (in km/h)"), SerializeField] private float normalSpeed = 25f;
    [Tooltip("Indicates whether the bus is using normal speed or maximum speed"), SerializeField] private bool usingNormalSpeed = true;

    [Header("Runtime Variables")]
    [SerializeField] float currentSpeed = 0f;
    [SerializeField] float currentSteerAngle = 0f;
    [SerializeField] DriveType driveType = DriveType.FrontWheelDrive;
    [SerializeField] SteeringType steeringType = SteeringType.FrontWheelSteering;
    [SerializeField] BrakingType brakingType = BrakingType.FrontWheelBraking;

    [Header("Sensors")]
    public LidarSensor lidarSensor; // Public zodat de bus agent er bij kan
    public TrafficLightDetector trafficLightDetector; // Public zodat de bus agent er bij kan
    
    public float CurrentSpeedNormalized => currentSpeed / maxSpeed;

    float motorInput, brakeInput, steerInput;


    private void FixedUpdate()
    {
        // Measuring
        currentSpeed = rb.linearVelocity.magnitude * 3.6f; // Convert m/s to km/h


        // Driving
        AccelerateBus();

        BrakeBus();


        // Steering
        SteerBus();
    }   

    public void ResetBus()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        currentSpeed = 0f;
        currentSteerAngle = 0f;
        motorInput = 0f;
        brakeInput = 0f;
        steerInput = 0f;
        wheelFrontLeft.motorTorque = 0f;
        wheelFrontRight.motorTorque = 0f;
        wheelBackLeft.motorTorque = 0f;
        wheelBackRight.motorTorque = 0f;
        wheelFrontLeft.brakeTorque = 0f;
        wheelFrontRight.brakeTorque = 0f;
        wheelBackLeft.brakeTorque = 0f;
        wheelBackRight.brakeTorque = 0f;
        wheelFrontLeft.steerAngle = 0f;
        wheelFrontRight.steerAngle = 0f;
        wheelBackLeft.steerAngle = 0f;
        wheelBackRight.steerAngle = 0f;
        wheelFrontLeftMesh.localRotation = Quaternion.identity;
        wheelFrontRightMesh.localRotation = Quaternion.identity;
        wheelBackLeftMesh.localRotation = Quaternion.identity;
        wheelBackRightMesh.localRotation = Quaternion.identity;
    }


    public void ControlBus(float motorInput, float brakeInput, float steerInput)
    {
        this.motorInput = motorInput;
        this.brakeInput = brakeInput;
        this.steerInput = steerInput;
    }

    private void SteerBus()
    {
        float targetSteerAngle = steerInput * maxSteerAngle;

        // Smoothly transition to the target steer angle
        currentSteerAngle = Mathf.MoveTowards(
            currentSteerAngle,
            targetSteerAngle,
            steeringSpeed * Time.fixedDeltaTime
        );

        switch (steeringType)
        {
            case SteeringType.FrontWheelSteering:
                wheelFrontLeft.steerAngle = currentSteerAngle;
                wheelFrontRight.steerAngle = currentSteerAngle;
                wheelFrontLeftMesh.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
                wheelFrontRightMesh.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
                break;
            case SteeringType.RearWheelSteering:
                wheelBackLeft.steerAngle = -currentSteerAngle;
                wheelBackRight.steerAngle = -currentSteerAngle;
                wheelBackLeftMesh.localRotation = Quaternion.Euler(0f, -currentSteerAngle, 0f);
                wheelBackRightMesh.localRotation = Quaternion.Euler(0f, -currentSteerAngle, 0f);
                break;
            default:
                wheelFrontLeft.steerAngle = currentSteerAngle;
                wheelFrontRight.steerAngle = currentSteerAngle;
                wheelBackLeft.steerAngle = -currentSteerAngle;
                wheelBackRight.steerAngle = -currentSteerAngle;
                wheelFrontLeftMesh.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
                wheelFrontRightMesh.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
                wheelBackLeftMesh.localRotation = Quaternion.Euler(0f, -currentSteerAngle, 0f);
                wheelBackRightMesh.localRotation = Quaternion.Euler(0f, -currentSteerAngle, 0f);
                break;
        }
    }

    private void BrakeBus()
    {
        // Apply brake torque to the wheels
        float outputBreakTorque = Mathf.Max(0f, brakeInput) * brakeTorque;
        switch (brakingType)
        {
            case BrakingType.FrontWheelBraking:
                wheelFrontLeft.brakeTorque = outputBreakTorque;
                wheelFrontRight.brakeTorque = outputBreakTorque;
                break;
            case BrakingType.RearWheelBraking:
                wheelBackLeft.brakeTorque = outputBreakTorque;
                wheelBackRight.brakeTorque = outputBreakTorque;
                break;
            default:
                wheelFrontLeft.brakeTorque = outputBreakTorque;
                wheelFrontRight.brakeTorque = outputBreakTorque;
                wheelBackLeft.brakeTorque = outputBreakTorque;
                wheelBackRight.brakeTorque = outputBreakTorque;
                break;
        }
    }

    private void AccelerateBus()
    {
        float outputMotorTorque = motorInput * motorTorque;
        float averageBrakeTorque = (wheelBackLeft.brakeTorque + wheelBackRight.brakeTorque + wheelFrontLeft.brakeTorque + wheelFrontRight.brakeTorque)/4;


        // Speed Limiting
        float targetSpeed = usingNormalSpeed ? normalSpeed : maxSpeed;
        if (currentSpeed > targetSpeed)
        {
            outputMotorTorque = 0f; // Stop applying torque if over speed limit
        }

        // Apply motor torque to the wheels
        switch (driveType)
        {
            case DriveType.FrontWheelDrive:
                wheelFrontLeft.motorTorque = outputMotorTorque;
                wheelFrontRight.motorTorque = outputMotorTorque;
                break;
            case DriveType.RearWheelDrive:
                wheelBackLeft.motorTorque = outputMotorTorque;
                wheelBackRight.motorTorque = outputMotorTorque;
                break;
            default:
                wheelFrontLeft.motorTorque = outputMotorTorque;
                wheelFrontRight.motorTorque = outputMotorTorque;
                wheelBackLeft.motorTorque = outputMotorTorque;
                wheelBackRight.motorTorque = outputMotorTorque;
                break;
        }
    }

    public float[] GetWheelEncoderValues()
    {
        return new float[]
        {
            wheelFrontLeft.rpm,
            wheelFrontRight.rpm,
            wheelBackLeft.rpm,
            wheelBackRight.rpm
        };
    }


    public enum DriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        AllWheelDrive
    }

    public enum SteeringType
    {
        FrontWheelSteering,
        RearWheelSteering,
        AllWheelSteering
    }

    public enum BrakingType
    {
        FrontWheelBraking,
        RearWheelBraking,
        AllWheelBraking
    }
}
