using UnityEngine;

public class EnemyActivation : MonoBehaviour
{
    // components to enable/disable
    private SpriteRenderer spr;
    private BoxCollider2D boxColl2D;
    private Enemy enemy;


    private void Awake()
    {
        spr = GetComponent<SpriteRenderer>();
        boxColl2D = GetComponent<BoxCollider2D>();
        enemy = GetComponent<Enemy>();
    }

    public void SetVisible(bool isVisible)
    {
        if (spr != null) spr.enabled = isVisible;
        if (boxColl2D != null) boxColl2D.enabled = isVisible;
        enemy.SetInZoneState();
    }
}