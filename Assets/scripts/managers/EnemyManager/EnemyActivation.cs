using UnityEngine;

public class EnemyActivation : MonoBehaviour
{
    // components to enable/disable
    [SerializeField] private GameObject enemyModel;
    private Enemy enemy;


    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    public void SetVisible(bool isVisible)
    {
        if (enemyModel != null) enemyModel.SetActive(isVisible);
        enemy.SetInZoneState();
    }
}