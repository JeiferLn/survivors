using UnityEngine;

public class WeaponAnimationController : MonoBehaviour
{
    private Animator animator;

    [Header("Layer Configuration")]
    [SerializeField]
    private int upperBodyLayerIndex = 1;

    [Header("Transition Speed")]
    [SerializeField]
    private float layerTransitionSpeed = 10f;

    private static readonly int IsAiming = Animator.StringToHash("isAiming");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int HasOneHandWeapon = Animator.StringToHash("hasOneHandWeapon");

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateUpperBodyLayer();
    }

    void UpdateUpperBodyLayer()
    {
        bool hasOneWeapon = animator.GetBool(HasOneHandWeapon);
        float isAiming = animator.GetFloat(IsAiming);
        float speed = animator.GetFloat(Speed);

        float targetWeight = 0f;

        // Activate upper body layer when:
        // 1. Has one weapon
        // 2. Aiming and running
        // 3. Not aiming but running or walking
        if (
            hasOneWeapon && ((isAiming > 0.5f && speed > 0.5f) || (isAiming < 0.5f && speed > 0.1f))
        )
        {
            targetWeight = 1f;
        }

        float currentWeight = animator.GetLayerWeight(upperBodyLayerIndex);

        animator.SetLayerWeight(
            upperBodyLayerIndex,
            Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * layerTransitionSpeed)
        );
    }
}
