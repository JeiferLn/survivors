using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    // ---------- REFERENCES ----------
    private Animator animator;
    private PlayerState playerState;
    private PlayerController playerController;
    private PlayerEquipmentController equipmentController;
    private RigBuilder rigBuilder;

    [Header("Animator Layers")]
    [SerializeField]
    private int upperBodyLayerIndex = 1;

    [Header("Damping")]
    [SerializeField]
    private float damping = 0.15f;

    private WeaponType lastWeaponType = WeaponType.isEmptyWeapon;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigBuilder = GetComponent<RigBuilder>();

        playerState = GetComponentInParent<PlayerState>();
        playerController = GetComponentInParent<PlayerController>();
        equipmentController = GetComponentInParent<PlayerEquipmentController>();
    }

    private void Update()
    {
        UpdateLocomotionAnimations();
        UpdateCombatAnimations();
        UpdateUpperBodyLayer();
        UpdateWeaponTypeAnimation();
    }

    // ---------------- LOCOMOTION ----------------
    private void UpdateLocomotionAnimations()
    {
        Vector3 localMoveDir = playerController.LocalMoveDirection;

        float speed =
            playerState.IsRunning ? 1f
            : playerState.IsMoving ? 0.5f
            : 0f;

        animator.SetFloat("Speed", speed, damping, Time.deltaTime);
        animator.SetFloat("Horizontal", localMoveDir.x, damping, Time.deltaTime);
        animator.SetFloat("Vertical", localMoveDir.z, damping, Time.deltaTime);
    }

    // ---------------- COMBAT ----------------
    private void UpdateCombatAnimations()
    {
        bool hasWeapon = playerState.Combat != PlayerCombatState.None;

        animator.SetBool("hasWeaponEquipped", hasWeapon);
        animator.SetFloat("isAiming", playerState.IsAiming ? 1f : 0f, damping, Time.deltaTime);
    }

    // ---------------- UPPER BODY / RIG ----------------
    private void UpdateUpperBodyLayer()
    {
        bool hasWeapon = playerState.Combat != PlayerCombatState.None;

        animator.SetLayerWeight(upperBodyLayerIndex, hasWeapon ? 1f : 0f);

        if (rigBuilder != null)
            rigBuilder.enabled = hasWeapon;
    }

    // ---------------- WEAPON TYPE (TRIGGERS) ----------------
    private void UpdateWeaponTypeAnimation()
    {
        WeaponType currentType =
            equipmentController.CurrentWeapon != null
                ? equipmentController.CurrentWeapon.weaponType
                : WeaponType.isEmptyWeapon;

        if (currentType == lastWeaponType)
            return;

        animator.ResetTrigger(lastWeaponType.ToString());

        animator.SetTrigger(currentType.ToString());

        lastWeaponType = currentType;
    }
}
