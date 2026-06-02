using UnityEngine;
using UnityEngine.InputSystem;

public class BusController : MonoBehaviour
{
    [Header("Bus Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private WheelCollider wheelFrontLeft, wheelFrontRight, wheelBackLeft, wheelBackRight;

    [Header("Bus Settings")]
    [SerializeField] private float driveSpeed = 1500f;
    [SerializeField] private float maxSteerAngle = 38f;
    [SerializeField] private float steeringSpeed = 5f;
    public bool fourWheelSteering = false;
    public bool fourWheelDrive = true;


    InputAction moveAction;
    float currentSteerAngle = 0f;

    public void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        moveAction.Enable();
    }

    private void FixedUpdate()
    {
        //Input
        Vector2 input = moveAction.ReadValue<Vector2>();

        // Driving
        float motorTorque = input.y * driveSpeed;
        if (fourWheelDrive)
        {
            wheelFrontLeft.motorTorque = motorTorque;
            wheelFrontRight.motorTorque = motorTorque;
        }
        wheelBackLeft.motorTorque = motorTorque;
        wheelBackRight.motorTorque = motorTorque;

        // Steering
        float targetSteerAngle = input.x * maxSteerAngle;

        currentSteerAngle = Mathf.MoveTowards(
            currentSteerAngle,
            targetSteerAngle,
            steeringSpeed * Time.fixedDeltaTime
        );

        if(fourWheelSteering)
        {
            wheelBackLeft.steerAngle = currentSteerAngle;
            wheelBackRight.steerAngle = currentSteerAngle;
        }

        wheelFrontLeft.steerAngle = currentSteerAngle;
        wheelFrontRight.steerAngle = currentSteerAngle;
    }
}
