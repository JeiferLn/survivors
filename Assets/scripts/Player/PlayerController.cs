using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ------------- REFERENCES -------------
    private CharacterController controller;
    private Camera mainCam;
    private Animator animator;

    // ------------- INPUT DEVICE TRACKING -------------
    private enum InputDeviceType
    {
        MouseKeyboard,
        Gamepad,
    }

    private InputDeviceType currentInputDevice = InputDeviceType.MouseKeyboard;

    // ------------- MOVEMENT VARIABLES -------------
    [Header("Movimiento")]
    [SerializeField]
    private float walkMovementSpeed = 6f;

    [SerializeField]
    private float runMovementSpeed = 10f;

    [SerializeField]
    private float aimingMoveSpeed = 4f;

    [SerializeField]
    private float startMoveDelay = 0.5f;

    private bool wasMoving = false;
    private float moveDelayTimer = 0f;

    [HideInInspector]
    public bool canMove = true;

    // ------------- STOP BUFFER VARIABLES -------------
    [Header("Stop Buffer")]
    [SerializeField]
    private float stopBufferDuration = 0.1f;
    private float stopBufferTimer = 0f;

    // ------------- GRAVITY VARIABLES -------------
    [Header("Gravity")]
    [SerializeField]
    private float gravity = -9.81f;
    private float verticalVelocity;

    // ------------- CAMERA VARIABLES -------------
    [Header("Camara")]
    [SerializeField]
    private Transform cameraTransform;

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

    public void SetAiming(bool aiming) => isAiming = aiming;

    // ---------------- DEVICE DETECTION --------------------
    private void DetectInputDevice(InputAction.CallbackContext ctx)
    {
        //  Get the device that triggered the action
        var device = ctx.control.device;

        //  Check if the device is a gamepad
        if (device is Gamepad)
        {
            bool hasInput = false;

            // Check the type of the value of the control
            if (ctx.valueType == typeof(Vector2))
            {
                hasInput = ctx.ReadValue<Vector2>().sqrMagnitude > 0.1f;
            }
            // Check if the value type is a float
            else if (ctx.valueType == typeof(float))
            {
                hasInput = ctx.ReadValue<float>() > 0.5f;
            }
            // Check if the device has input
            if (hasInput)
            {
                currentInputDevice = InputDeviceType.Gamepad;
            }
        }
        // Check if the device is a mouse or a keyboard
        else if (device is Mouse || device is Keyboard)
        {
            currentInputDevice = InputDeviceType.MouseKeyboard;
        }
    }

    // ---------------- START --------------------
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCam = Camera.main;
        animator = GetComponentInChildren<Animator>();
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
        // Get the direction of the movement
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
        bool isTryingToMove = direction.sqrMagnitude > 0.01f;

        // Check if the player is trying to move
        if (isTryingToMove)
        {
            // Set the stop buffer timer
            stopBufferTimer = stopBufferDuration;
        }
        else
        {
            // Decrease the stop buffer timer
            stopBufferTimer -= Time.deltaTime;
        }

        // Check if the player is truly stopped
        bool trulyStopped = stopBufferTimer <= 0f;
        if (trulyStopped)
        {
            wasMoving = false;
        }

        // Check if the player is trying to move and is not moving
        if (isTryingToMove && !wasMoving)
        {
            moveDelayTimer = startMoveDelay;
            wasMoving = true;
        }

        // Check if the player is moving with a delay
        if (moveDelayTimer > 0f)
        {
            moveDelayTimer -= Time.deltaTime;

            Vector3 localDirDelay = transform.InverseTransformDirection(direction);

            animator.SetFloat("Speed", 0.5f, 0.15f, Time.deltaTime);
            animator.SetFloat("Horizontal", localDirDelay.x, 0.15f, Time.deltaTime);
            animator.SetFloat("Vertical", localDirDelay.z, 0.15f, Time.deltaTime);

            return;
        }

        // Normalize the direction of the movement
        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        // Check if the player is going back
        float dot = Vector3.Dot(transform.forward, direction);
        bool isGoingBack = dot < -0.5f;

        // Get the current speed of the player
        float currentSpeed;

        // Check if the player is aiming
        if (isAiming)
        {
            currentSpeed = isGoingBack ? aimingMoveSpeed / 2f : aimingMoveSpeed;
        }
        // Check if the player is not aiming
        else
        {
            float baseSpeed = isRunning ? runMovementSpeed : walkMovementSpeed;
            currentSpeed = isGoingBack ? baseSpeed / 2f : baseSpeed;
        }

        // Get the local final direction of the movement
        Vector3 localFinalDir = transform.InverseTransformDirection(direction);

        // Get the animation speed
        float animSpeed = isRunning ? 1f : 0f;

        // Set the animation speed
        animator.SetFloat("Speed", animSpeed, 0.15f, Time.deltaTime);
        animator.SetFloat("Horizontal", localFinalDir.x, 0.15f, Time.deltaTime);
        animator.SetFloat("Vertical", localFinalDir.z, 0.15f, Time.deltaTime);

        // Move the player
        controller.Move(direction * currentSpeed * Time.deltaTime);
    }

    // ---------------- GRAVITY --------------------
    private void ApplyGravity()
    {
        // Check if the player is grounded
        if (controller.isGrounded)
        {
            // Check if the player is falling
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;
        }
        // Check if the player is not grounded
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Apply the gravity to the player
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    // ---------------- ROTATION HANDLER --------------------
    private void HandleRotation()
    {
        // Check if the player is using a gamepad
        if (currentInputDevice == InputDeviceType.Gamepad)
        {
            RotateWithGamepad();
        }
        // Check if the player is using a mouse or a keyboard
        else
        {
            RotateWithMouse();
        }

        // Apply the smooth rotation to the player
        ApplySmoothRotation();
    }

    // ---------------- ROTATE WITH GAMEPAD --------------------
    private void RotateWithGamepad()
    {
        // Get the direction of the look input
        Vector3 dir = new Vector3(lookInput.x, 0f, lookInput.y);

        // Check if the direction is valid
        if (dir.sqrMagnitude > 0.01f)
        {
            targetRotation = Quaternion.LookRotation(dir);
        }
    }

    // ---------------- ROTATE WITH MOUSE --------------------
    private void RotateWithMouse()
    {
        // Get the ray from the mouse position
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        // Check if the ray hits the walk area
        if (Physics.Raycast(ray, out RaycastHit hit, 300f, walkAreaLayer))
        {
            Vector3 dir = hit.point - transform.position;
            dir.y = 0;

            // Check if the direction is valid
            if (dir.sqrMagnitude > 0.01f)
            {
                targetRotation = Quaternion.LookRotation(dir);
            }
        }
    }

    // ---------------- APPLY SMOOTH ROTATION --------------------
    private void ApplySmoothRotation()
    {
        // Get the rotation speed
        float rotationSpeed =
            currentInputDevice == InputDeviceType.Gamepad
                ? gamepadRotationSpeed
                : mouseRotationSpeed;

        // Apply the smooth rotation to the player
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
