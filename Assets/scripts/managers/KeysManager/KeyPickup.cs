using Sirenix.OdinInspector;
using UnityEngine;

public class KeyPickup : MonoBehaviour, IInteractable
{
    [Title("Configuración")]
    private ItemInfo objectData;

    private string _keyId;
    private string _description;
    private string _ItemName;


    [SerializeField]
    private bool _destroyOnPickup = true;

    [Title("Outline Shader Referencia")]
    [SerializeField]
    private Outline outline;

    private void Start()
    {
        objectData = GetComponent<ItemInfo>();
        _keyId = objectData.itemData.itemID.ToString();
        _ItemName = objectData.itemData.itemName;   
        _description = objectData.itemData.itemDescription;
    }

    public void Interact()
    {
        // Buscar al jugador
        PlayerInteraction player = FindFirstObjectByType<PlayerInteraction>();
        // Resaltar objeto de importancia
        string importantColor = ColorUtility.ToHtmlStringRGBA(DialogueSystem.Instance.textImportantColor);
        
        if (player != null)
        {
            player.AddKey(_keyId);
            DialogueSystem.Instance.SendText($"Obtuviste <color=#{importantColor}>{_ItemName}</color>,\n" + _description);
            if (_destroyOnPickup) Destroy(gameObject);
        }
    }
}