using UnityEngine;
using Sirenix.OdinInspector;

public class KeyPickup : MonoBehaviour, IInteractable
{
    [Title("Configuración")]
    [SerializeField] private string _keyId = "key_01";
    [SerializeField] private bool _destroyOnPickup = true;
    
    
    public void Interact()
    {
        // Buscar al jugador
        PlayerInteraction3D player = FindObjectOfType<PlayerInteraction3D>();
        
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