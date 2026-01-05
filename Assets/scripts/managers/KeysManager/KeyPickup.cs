using Sirenix.OdinInspector;
using UnityEngine;

public class KeyPickup : MonoBehaviour, IInteractable
{
    [Title("Configuración")]
    private string _keyId;
    private string _description;
    
    [SerializeField]
    private ItemInfo objectData;
    [SerializeField]
    private bool _destroyOnPickup = true;

    [Title("Outline Shader Referencia")]
    [SerializeField]
    private Outline outline;

    private void Start()
    {
        objectData = GetComponent<ItemInfo>();
        _keyId = objectData.itemData.itemID;
    }

    public void Interact()
    {
        // Buscar al jugador
        PlayerInteraction player = FindFirstObjectByType<PlayerInteraction>();

        if (player != null)
        {
            player.AddKey(_keyId);

            if (_destroyOnPickup)
                Destroy(gameObject);
        }
    }

    public string GetInteractionText()
    {
        return $"Recoger llave [{_keyId}]";
    }
}