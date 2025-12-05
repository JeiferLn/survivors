using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class PlayerInteraction3D : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════
    // CONFIGURACIÓN DE INTERACCIÓN
    // ══════════════════════════════════════════════════════════════
    
    [Title("Interacción")]
    [SerializeField] private float _interactionRange = 3f;
    [SerializeField] private LayerMask _interactableLayer;
    
    // ══════════════════════════════════════════════════════════════
    // INVENTARIO DE LLAVES
    // ══════════════════════════════════════════════════════════════
    
    [Title("Llaves")]
    [SerializeField]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = false)]
    private List<string> _keys = new List<string>();
    
    [ShowInInspector, ReadOnly]
    private int KeyCount => _keys.Count;
    
    // ══════════════════════════════════════════════════════════════
    // DEBUG
    // ══════════════════════════════════════════════════════════════
    
    [Title("Debug")]
    [ShowInInspector, ReadOnly]
    private string _lastInteractionText = "Ninguno";
    
    [ShowInInspector, ReadOnly]
    private IInteractable _currentTarget;
    
    private Camera _camera;
    
    // ══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════
    
    private void Start()
    {
        _camera = Camera.main;
    }
    
    private void Update()
    {
        UpdateCurrentTarget();
        
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }
    }
    
    // ══════════════════════════════════════════════════════════════
    // DETECCIÓN DE OBJETIVO
    // ══════════════════════════════════════════════════════════════
    
    private void UpdateCurrentTarget()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _interactableLayer))
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            
            if (distance <= _interactionRange)
            {
                _currentTarget = hit.collider.GetComponent<IInteractable>();
                _lastInteractionText = _currentTarget?.GetInteractionText() ?? "No interactuable";
            }
            else
            {
                _currentTarget = null;
                _lastInteractionText = "Muy lejos";
            }
        }
        else
        {
            _currentTarget = null;
            _lastInteractionText = "Ninguno";
        }
    }
    
    // ══════════════════════════════════════════════════════════════
    // INTERACCIÓN
    // ══════════════════════════════════════════════════════════════
    
    private void TryInteract()
    {
        if (_currentTarget == null) return;
        
        // Verificar si es una puerta con llave
        if (_currentTarget is Door door)
        {
            InteractWithDoor(door);
        }
        else
        {
            // Interacción normal para otros objetos
            _currentTarget.Interact();
        }
    }
    
    private void InteractWithDoor(Door door)
    {
        // Si la puerta requiere llave y está bloqueada
        if (door.CurrentDoorType == Door.DoorType.KeyRequired && door.IsLocked)
        {
            // Buscar si tenemos la llave correcta
            string requiredKey = door.RequiredKeyId;
            
            if (HasKey(requiredKey))
            {
                door.InteractWithKey(requiredKey);
                Debug.Log($"🔑 Usaste la llave: {requiredKey}");
            }
            else
            {
                Debug.Log($"❌ No tienes la llave: {requiredKey}");
                // Aquí podrías mostrar UI, reproducir sonido, etc.
            }
        }
        else
        {
            // Puerta normal o ya desbloqueada
            door.Interact();
        }
    }
    
    // ══════════════════════════════════════════════════════════════
    // SISTEMA DE LLAVES
    // ══════════════════════════════════════════════════════════════
    
    public bool HasKey(string keyId)
    {
        return _keys.Contains(keyId);
    }
    
    public void AddKey(string keyId)
    {
        if (!HasKey(keyId))
        {
            _keys.Add(keyId);
            Debug.Log($"🔑 Llave obtenida: {keyId}");
        }
    }
    
    public bool RemoveKey(string keyId)
    {
        if (_keys.Remove(keyId))
        {
            Debug.Log($"🔑 Llave removida: {keyId}");
            return true;
        }
        return false;
    }
    
    public void ClearKeys()
    {
        _keys.Clear();
    }
    
    // ══════════════════════════════════════════════════════════════
    // GETTERS PÚBLICOS
    // ══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Texto de interacción actual (útil para UI)
    /// </summary>
    public string GetCurrentInteractionText()
    {
        return _lastInteractionText;
    }
    
    /// <summary>
    /// ¿Hay un objetivo válido?
    /// </summary>
    public bool HasValidTarget()
    {
        return _currentTarget != null;
    }
    
    // ══════════════════════════════════════════════════════════════
    // BOTONES DE DEBUG (ODIN)
    // ══════════════════════════════════════════════════════════════
    
    #if UNITY_EDITOR
    [Title("Testing")]
    [Button("Agregar Llave de Prueba")]
    private void AddTestKey()
    {
        AddKey($"key_test_{_keys.Count + 1}");
    }
    
    [Button("Agregar Llave Específica")]
    private void AddSpecificKey(string keyId = "key_01")
    {
        AddKey(keyId);
    }
    
    [Button("Limpiar Llaves")]
    private void ClearAllKeys()
    {
        ClearKeys();
    }
    #endif
}