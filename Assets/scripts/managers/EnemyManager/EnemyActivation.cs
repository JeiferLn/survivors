using UnityEngine;

public class EnemyActivation : MonoBehaviour
{
    // components to enable/disable
    private MeshRenderer mesh;
    private Enemy enemy;


    private void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
        enemy = GetComponent<Enemy>();
    }

    public void SetVisible(bool isVisible)
    {
        if (mesh != null) mesh.enabled = isVisible;
        enemy.SetInZoneState();
    }
}