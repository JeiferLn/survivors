using UnityEngine;

public class EnemyActivation : MonoBehaviour
{
    // components to enable/disable
    private SpriteRenderer spr;
    private CapsuleCollider capsuleCollider;
    private Enemy enemy;


    private void Awake()
    {
        spr = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        enemy = GetComponent<Enemy>();
    }

    public void SetVisible(bool isVisible)
    {
        if (spr != null) spr.enabled = isVisible;
        if (capsuleCollider != null) capsuleCollider.enabled = isVisible;
        enemy.SetInZoneState();
    }
}