using UnityEngine;
using Sirenix.OdinInspector;

public class KeyPickup : MonoBehaviour, IInteractable
{
    [Title("Configuración")] 
    [SerializeField] private string _keyId = "key_01";
    [SerializeField] private bool _destroyOnPickup = true;

    [Title("Outline Shader Referencia")]
    [SerializeField] private Outline outline;

    public void Interact()
    {
        // Buscar al jugador
        PlayerInteraction3D player = FindFirstObjectByType<PlayerInteraction3D>();

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