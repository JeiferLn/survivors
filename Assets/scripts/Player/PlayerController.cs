using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ------------- REFERENCES -------------
    private CharacterController controller;
    private Camera mainCam;

    // ------------- MOVEMENT VARIABLES -------------
    [Header("Movimiento")]
    [SerializeField]
    private float moveSpeed = 10f;

    [SerializeField]
    private float aimingMoveSpeed = 4f;

    // ------------- GRAVITY VARIABLES -------------
    [SerializeField]
    private float gravity = -9.81f;
    private float verticalVelocity;

    // ------------- CAMERA VARIABLES -------------
    [Header("Camara")]
    [SerializeField]
    private Transform cameraTransform;

    // ------------- LAYER MASK -------------
    [Header("Layer")]
    [SerializeField]
    private LayerMask walkAreaLayer;

    // ------------- INPUT VARIABLES -------------
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isAiming;

    // --------------------------------------------------------
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCam = Camera.main;
    }

    // ---------------- INPUTS --------------------
    public void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();

    public void OnLook(InputAction.CallbackContext ctx) => lookInput = ctx.ReadValue<Vector2>();

    // ---------------- SET --------------------
    public void SetAiming(bool aiming) => isAiming = aiming;

    // ---------------- UPDATE LOOP --------------------
    private void Update()
    {
        MovePlayer();
        ApplyGravity();
        HandleRotation();
    }

    // ---------------- MOVEMENT --------------------
    private void MovePlayer()
    {
        float currentSpeed = isAiming ? aimingMoveSpeed : moveSpeed;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        Vector3 direction = forward * moveInput.y + right * moveInput.x;
        direction = direction.sqrMagnitude > 1f ? direction.normalized : direction;

        controller.Move(direction * currentSpeed * Time.deltaTime);
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

    // ---------------- ROTATION HANDLER --------------------
    private void HandleRotation()
    {
        bool usingGamepad = Gamepad.current != null && lookInput.sqrMagnitude > 0.1f;

        if (usingGamepad)
            RotateWithGamepad();
        else
            RotateWithMouse();
    }

    // ---------------- ROTATION (GAMEPAD) --------------------
    private void RotateWithGamepad()
    {
        Vector3 dir = new Vector3(lookInput.x, 0f, lookInput.y);

        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    // ---------------- ROTATION (MOUSE) --------------------
    private void RotateWithMouse()
    {
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 300f, walkAreaLayer))
            RotateTowards(hit.point);
    }

    // ---------------- FINAL ROTATE FUNCTION --------------------
    private void RotateTowards(Vector3 worldPoint)
    {
        Vector3 dir = worldPoint - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}
