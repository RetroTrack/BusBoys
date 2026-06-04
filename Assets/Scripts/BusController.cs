using UnityEngine;
using UnityEngine.InputSystem;

public class BusController : MonoBehaviour
{
    [Header("Bus Components")]
    [SerializeField] private Rigidbody rb;
    public WheelCollider wheelFrontLeft, wheelFrontRight, wheelBackLeft, wheelBackRight; //public zodat de trafficDetector de wielen op kan halen

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



    InputAction moveAction;
    InputAction brakeAction;

    public void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        brakeAction = InputSystem.actions.FindAction("Jump");

        moveAction.Enable();
        brakeAction.Enable();
    }

    private void FixedUpdate()
    {
        // Measuring
        currentSpeed = rb.linearVelocity.magnitude * 3.6f; // Convert m/s to km/h

        //Input
        Vector2 input = moveAction.ReadValue<Vector2>();
        bool isBreaking = brakeAction.ReadValue<float>() > 0.1f;

        // Driving
        AccelerateBus(input, isBreaking);

        if (isBreaking)
            BrakeBus(input);
        

        // Steering
        SteerBus(input);
    }

    private void SteerBus(Vector2 input)
    {
        float targetSteerAngle = input.x * maxSteerAngle;

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
                break;
            case SteeringType.RearWheelSteering:
                wheelBackLeft.steerAngle = -currentSteerAngle;
                wheelBackRight.steerAngle = -currentSteerAngle;
                break;
            default:
                wheelFrontLeft.steerAngle = currentSteerAngle;
                wheelFrontRight.steerAngle = currentSteerAngle;
                wheelBackLeft.steerAngle = -currentSteerAngle;
                wheelBackRight.steerAngle = -currentSteerAngle;
                break;
        }
    }

    private void BrakeBus(Vector2 input)
    {
        // Apply brake torque to the wheels
        float outputBreakTorque = (1-Mathf.Abs(input.y)) * brakeTorque;
        switch(brakingType)
        {
            case BrakingType.FrontWheelBraking:
                wheelFrontLeft.motorTorque = 0f;
                wheelFrontRight.motorTorque = 0f;
                wheelFrontLeft.brakeTorque = outputBreakTorque;
                wheelFrontRight.brakeTorque = outputBreakTorque;
                break;
            case BrakingType.RearWheelBraking:
                wheelBackLeft.motorTorque = 0f;
                wheelBackRight.motorTorque = 0f;
                wheelBackLeft.brakeTorque = outputBreakTorque;
                wheelBackRight.brakeTorque = outputBreakTorque;
                break;
            default:
                wheelFrontLeft.motorTorque = 0f;
                wheelFrontRight.motorTorque = 0f;
                wheelBackLeft.motorTorque = 0f;
                wheelBackRight.motorTorque = 0f;
                wheelFrontLeft.brakeTorque = outputBreakTorque;
                wheelFrontRight.brakeTorque = outputBreakTorque;
                wheelBackLeft.brakeTorque = outputBreakTorque;
                wheelBackRight.brakeTorque = outputBreakTorque;
                break;
        }
    }

    private void AccelerateBus(Vector2 input, bool isBreaking)
    {
        float outputMotorTorque = input.y * motorTorque;

        // Speed Limiting
        float targetSpeed = usingNormalSpeed ? normalSpeed : maxSpeed;
        if (currentSpeed > targetSpeed)
        {
            outputMotorTorque = 0f; // Stop applying torque if over speed limit
        }

        if (!isBreaking)
        {
            wheelFrontLeft.brakeTorque = 0;
            wheelFrontRight.brakeTorque = 0;
            wheelBackLeft.brakeTorque = 0;
            wheelBackRight.brakeTorque = 0;
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
