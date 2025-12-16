using UnityEngine;

public class WeaponAnimationController : MonoBehaviour
{
    private Animator animator;

    [Header("Layer Configuration")]
    [SerializeField] private int upperBodyLayerIndex = 1;

    [Header("Transition Speed")]
    [SerializeField] private float layerTransitionSpeed = 10f;

    private static readonly int IsAiming = Animator.StringToHash("isAiming");
    private static readonly int Speed = Animator.StringToHash("speed");
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
        bool hasWeapon = animator.GetBool(HasOneHandWeapon);
        float isAiming = animator.GetFloat(IsAiming);
        float speed = animator.GetFloat(Speed);

        float targetWeight = 0f;

        if (hasWeapon && isAiming < 0.5f && speed > 0.1f)
        {
            targetWeight = 1f;
            animator.SetBool(HasOneHandWeapon, true);
        }
        else
        {
            targetWeight = 0f;
            animator.SetBool(HasOneHandWeapon, false);
        }

        float currentWeight = animator.GetLayerWeight(upperBodyLayerIndex);
        float newWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * layerTransitionSpeed);
        animator.SetLayerWeight(upperBodyLayerIndex, newWeight);
    }
}