using UnityEngine;
using UnityEngine.Animations.Rigging;

public class UpperBodyLayerController : MonoBehaviour
{
    // ------------- REFERENCES -------------
    private Animator animator;
    private RigBuilder rigBuilder;
    private int upperBodyLayerIndex = 1;

    // ------------- START -------------
    private void Start()
    {
        animator = GetComponent<Animator>();
        rigBuilder = GetComponent<RigBuilder>();
    }

    // ------------- UPDATE -------------
    private void Update()
    {
        bool hasWeapon = animator.GetBool("hasWeaponEquipped");

        bool isMoving = animator.GetFloat("Speed") > 0.1f;

        float targetWeight = hasWeapon ? 1f : 0f;

        animator.SetLayerWeight(upperBodyLayerIndex, targetWeight);
        rigBuilder.enabled = hasWeapon;
    }
}
