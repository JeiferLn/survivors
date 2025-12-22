using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ------------- REFERENCES -------------
    private CharacterController controller;
    private Camera mainCam;
    private PlayerState playerState;

    // ------------- PUBLIC DATA FOR OTHER SYSTEMS -------------
    public Vector3 LocalMoveDirection { get; private set; }

    // ------------- INPUT DEVICE TRACKING -------------
    private enum InputDeviceType
    {
        MouseKeyboard,
        Gamepad,
    }

    [SerializeField]
    private TextMeshPro keyUI;

    [SerializeField]
    private string actionName = "Pickup";

    private PlayerInput playerInput;

    private InputDeviceType currentInputDevice = InputDeviceType.Gamepad;

    // ------------- MOVEMENT VARIABLES -------------
    [Header("Movimiento")]
    [SerializeField]
    private float walkMovementSpeed = 6f;

    [SerializeField]
    private float runMovementSpeed = 10f;

    [SerializeField]
    private float aimingMoveSpeed = 4f;

    [HideInInspector]
    public bool canMove = true;

    // ------------- GRAVITY VARIABLES -------------
    [Header("Gravity")]
    [SerializeField]
    private float gravity = -9.81f;

    private float verticalVelocity;

    // ------------- ROTATION VARIABLES -------------
    [Header("Rotación")]
    [SerializeField]
    private float gamepadRotationSpeed = 10f;

    [SerializeField]
    private float mouseRotationSpeed = 15f;

    private Quaternion targetRotation;

    // ------------- LAYER MASK -------------
    [Header("Layer")]
    [SerializeField]
    private LayerMask walkAreaLayer;

    // ------------- INPUT VARIABLES -------------
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isAiming;
    private bool isRunning;

    // ---------------- INPUTS --------------------
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
        DetectInputDevice(ctx);
    }

    public void OnRun(InputAction.CallbackContext ctx)
    {
        isRunning = ctx.ReadValueAsButton();
        DetectInputDevice(ctx);
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
        DetectInputDevice(ctx);
    }

    // ---------------- START --------------------
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
        mainCam = Camera.main;
        playerState = GetComponent<PlayerState>();
        targetRotation = transform.rotation;
    }

    // ---------------- UPDATE --------------------
    private void Update()
    {
        if (!canMove)
            return;

        MovePlayer();
        ApplyGravity();
        HandleRotation();
    }

    // ---------------- MOVEMENT --------------------
    private void MovePlayer()
    {
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
        LocalMoveDirection = transform.InverseTransformDirection(direction);

        UpdateLocomotionState(direction);

        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        float dot = Vector3.Dot(transform.forward, direction);
        bool isGoingBack = dot < -0.3f;

        float currentSpeed;

        if (playerState.IsAiming)
        {
            currentSpeed = isGoingBack ? aimingMoveSpeed / 2f : aimingMoveSpeed;
        }
        else
        {
            float baseSpeed = playerState.IsRunning ? runMovementSpeed : walkMovementSpeed;

            currentSpeed = isGoingBack ? baseSpeed / 2f : baseSpeed;
        }

        controller.Move(direction * currentSpeed * Time.deltaTime);
    }

    // ---------------- LOCOMOTION STATE --------------------
    private void UpdateLocomotionState(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.01f)
        {
            playerState.SetLocomotion(PlayerLocomotionState.Idle);
            return;
        }

        playerState.SetLocomotion(
            isRunning ? PlayerLocomotionState.Running : PlayerLocomotionState.Walking
        );
    }

    // ---------------- GRAVITY --------------------
    private void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    // ---------------- ROTATION --------------------
    private void HandleRotation()
    {
        if (currentInputDevice == InputDeviceType.Gamepad)
            RotateWithGamepad();
        else
            RotateWithMouse();

        ApplySmoothRotation();
    }

    private void RotateWithGamepad()
    {
        Vector3 dir = new Vector3(lookInput.x, 0f, lookInput.y);

        if (dir.sqrMagnitude > 0.01f)
            targetRotation = Quaternion.LookRotation(dir);
    }

    private void RotateWithMouse()
    {
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 300f, walkAreaLayer))
        {
            Vector3 dir = hit.point - transform.position;
            dir.y = 0;

            if (dir.sqrMagnitude > 0.01f)
                targetRotation = Quaternion.LookRotation(dir);
        }
    }

    private void ApplySmoothRotation()
    {
        float rotationSpeed =
            currentInputDevice == InputDeviceType.Gamepad
                ? gamepadRotationSpeed
                : mouseRotationSpeed;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    // ---------------- DEVICE DETECTION --------------------
    private void DetectInputDevice(InputAction.CallbackContext ctx)
    {
        var device = ctx.control.device;
        playerInput = GetComponent<PlayerInput>();

        if (device is Gamepad)
        {
            if (ctx.valueType == typeof(Vector2) && ctx.ReadValue<Vector2>().sqrMagnitude > 0.1f)
                currentInputDevice = InputDeviceType.Gamepad;
        }
        else if (device is Mouse || device is Keyboard)
        {
            currentInputDevice = InputDeviceType.MouseKeyboard;
        }

        UpdateKeyDisplay();
    }

    private void UpdateKeyDisplay()
    {
        var action = playerInput.actions[actionName];

        // Obtener el índice correcto según el dispositivo actual
        int bindingIndex = GetCorrectBindingIndex(action);


        if (bindingIndex != -1)
        {
            string buttonCharacter = action.GetBindingDisplayString(bindingIndex);

            switch (buttonCharacter)
            {
                case "Triangle": buttonCharacter = "\\u25B2"; break;
                case "Square": buttonCharacter = "\\u25A1"; break;
                case "Circle": buttonCharacter = "\\u25CB"; break;
                case "Cross": buttonCharacter = "X"; break;
            }
            
            keyUI.text = buttonCharacter;
        }
        else
        {
            keyUI.text = "?";
        }
    }

    private int GetCorrectBindingIndex(InputAction action)
    {
        // Iterar por todos los bindings
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];

            if (currentInputDevice == InputDeviceType.Gamepad)
            {
                // Buscar bindings de gamepad
                if (binding.path.Contains("<Gamepad>") ||
                    binding.path.Contains("button") ||
                    binding.path.Contains("rightTrigger") ||
                    binding.path.Contains("leftTrigger"))
                {
                    return i;
                }
            }
            else if (currentInputDevice == InputDeviceType.MouseKeyboard)
            {
                // Buscar bindings de teclado
                if (binding.path.Contains("<Keyboard>"))
                {
                    return i;
                }
            }
        }

        return -1; // No encontrado
    }
}