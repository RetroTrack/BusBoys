using UnityEngine;
using UnityEngine.InputSystem;

namespace BusBoys.Assets.Scripts.Core.Utilities
{

    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 4, -8);

        [Header("Sensitivity")]
        [SerializeField] private float mouseSensitivity = 0.15f;
        [SerializeField] private float controllerSensitivity = 180f;

        [Header("Pitch Limits")]
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 70f;

        private InputAction lookAction;

        private float yaw;
        private float pitch;

        [SerializeField] private bool isLooking;
        [SerializeField] bool gamepadLook = false;
        [SerializeField] bool mouseLook = false;

        private void Awake()
        {
            lookAction = InputSystem.actions.FindAction("Look");
            if (target == null) return;
            SetDefaultPosition();
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            SetDefaultPosition();
        }

        private void SetDefaultPosition()
        {
            yaw = target.eulerAngles.y;
            pitch = 10f; // Default pitch angle
        }

        private void Update()
        {
            HandleLookState();
            HandleRotation();
            HandleCursor();
            ApplyCamera();
        }

        private void HandleLookState()
        {
            mouseLook =
                Mouse.current != null &&
                Mouse.current.rightButton.isPressed;

            gamepadLook =
                lookAction.activeControl != null &&
                lookAction.activeControl.device is Gamepad;

            isLooking = mouseLook || gamepadLook;
        }

        private void HandleRotation()
        {
            if (!isLooking)
                return;

            Vector2 look = lookAction.ReadValue<Vector2>();

            float sensitivity = 100f;
            if(gamepadLook)
                sensitivity = controllerSensitivity;
            else if (mouseLook)
                sensitivity = mouseSensitivity;

            yaw += look.x * sensitivity * Time.deltaTime;
            pitch -= look.y * sensitivity * Time.deltaTime;

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        private void HandleCursor()
        {
            if (isLooking)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void ApplyCamera()
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

            transform.position = target.position + rotation * offset;
            transform.LookAt(target.position);
        }
    }
}