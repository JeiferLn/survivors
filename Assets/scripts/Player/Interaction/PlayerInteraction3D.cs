using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerInteraction3D : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════
    // CONFIGURACIÓN DE INTERACCIÓN
    // ══════════════════════════════════════════════════════════════

    [Title("Interacción")]
    [SerializeField]
    private float _interactionRange = 3f;

    [SerializeField]
    private LayerMask _interactableLayer;

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
    private Outline _lastOutline;

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

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // DETECCIÓN DE OBJETIVO
    // ══════════════════════════════════════════════════════════════

    private void UpdateCurrentTarget()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            _interactionRange,
            _interactableLayer
        );

        // Si no hay nada → apagar outline previo
        if (hits.Length == 0)
        {
            DisableLastTargetEffects();
            _currentTarget = null;
            _lastInteractionText = "Ninguno";
            return;
        }

        // Guardamos todos los outlines dentro del rango
        List<Outline> outlinesInRange = new List<Outline>();

        float bestDist = float.MaxValue;
        IInteractable bestTarget = null;
        Outline bestOutline = null;

        foreach (var hit in hits)
        {
            Outline outline = hit.GetComponent<Outline>();
            if (outline != null)
                outlinesInRange.Add(outline);

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestTarget = hit.GetComponent<IInteractable>();
                bestOutline = outline;
            }
        }

        // Set target actual
        _currentTarget = bestTarget;

        if (_currentTarget != null)
            _lastInteractionText = _currentTarget.GetInteractionText();
        else
            _lastInteractionText = "No interactuable";

        // Primero desactivamos todos los outlines excepto el mejor
        foreach (var outline in outlinesInRange)
        {
            if (outline != bestOutline)
            {
                outline.enabled = false;

                Transform billboard = outline.transform.GetChild(0);
                billboard.gameObject.SetActive(false);
                BillboardManager.Instance.Unregister(billboard);
            }
        }

        // Solo activamos al más cercano
        if (bestOutline != null)
        {
            if (_lastOutline != null && _lastOutline != bestOutline)
            {
                DisableLastTargetEffects();
            }

            bestOutline.enabled = true;
            _lastOutline = bestOutline;

            Transform billboard = bestOutline.transform.GetChild(0);
            billboard.gameObject.SetActive(true);
            BillboardManager.Instance.Register(billboard);
        }
    }

    // ══════════════════════════════════════════════════════════════
    // INTERACCIÓN
    // ══════════════════════════════════════════════════════════════

    private void TryInteract()
    {
        if (_currentTarget == null)
            return;

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
            // Debug.Log($"🔑 Llave obtenida: {keyId}");
            DialogueSystem.Instance.SendText($"Has obtenido llaves {keyId}");
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

    private void DisableLastTargetEffects()
    {
        if (_lastOutline == null)
            return;

        Transform billboard = _lastOutline.transform.GetChild(0);
        billboard.GetComponent<BillboardFadeInOut>()?.OnFadeOut();
        BillboardManager.Instance.Unregister(billboard);
        billboard.gameObject.SetActive(false);

        _lastOutline.enabled = false;
        _lastOutline = null;
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
