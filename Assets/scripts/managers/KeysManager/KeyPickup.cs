using Sirenix.OdinInspector;
using UnityEngine;

public class KeyPickup : MonoBehaviour, IInteractable
{
    [Title("Configuración")]
    [SerializeField]
    private string _keyId = "key_01";

    [SerializeField]
    private bool _destroyOnPickup = true;

    [Title("Outline Shader Referencia")]
    [SerializeField]
    private Outline outline;

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
