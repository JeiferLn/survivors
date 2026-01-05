using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
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
    // SISTEMA DE EQUIPAMIENTO
    // ══════════════════════════════════════════════════════════════

    [Title("Equipamiento")]
    private PlayerEquipmentController _equipmentController;

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

    [ShowInInspector, ReadOnly]
    private IEquipable _currentEquipableTarget;

    private Camera _camera;
    private Outline _lastOutline;

    // ══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════

    private void Awake()
    {
        _equipmentController = GetComponent<PlayerEquipmentController>();
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        UpdateCurrentTarget();
    }

    // ══════════════════════════════════════════════════════════════
    // INPUT SYSTEM - INTERACCIÓN
    // ══════════════════════════════════════════════════════════════

    public void OnInteraction(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        TryInteract();
    }

    // ══════════════════════════════════════════════════════════════
    // DETECCIÓN DE OBJETIVO (INTERACCIÓN 3D)
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
            _currentEquipableTarget = null;
            _lastInteractionText = "Ninguno";
            return;
        }

        // Guardamos todos los outlines dentro del rango
        List<Outline> outlinesInRange = new List<Outline>();

        float bestDist = float.MaxValue;
        IInteractable bestInteractable = null;
        IEquipable bestEquipable = null;
        Outline bestOutline = null;

        foreach (var hit in hits)
        {
            // Buscar componentes sin depender del Outline
            IInteractable interactable = hit.GetComponent<IInteractable>();
            IEquipable equipable = hit.GetComponent<IEquipable>();
            Outline outline = hit.GetComponent<Outline>();

            // Si tiene outline, agregarlo a la lista
            if (outline != null)
                outlinesInRange.Add(outline);

            // Solo considerar objetos que tienen IInteractable o IEquipable
            if (interactable == null && equipable == null)
                continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < bestDist)
            {
                // Prioridad: IInteractable sobre IEquipable
                if (interactable != null)
                {
                    bestDist = dist;
                    bestInteractable = interactable;
                    bestEquipable = null; // Limpiar equipable si hay interactable
                    bestOutline = outline;
                }
                else if (equipable != null && bestInteractable == null)
                {
                    // Solo considerar equipable si no hay interactable
                    bestDist = dist;
                    bestEquipable = equipable;
                    bestOutline = outline;
                }
            }
        }

        // Set targets actuales
        _currentTarget = bestInteractable;
        _currentEquipableTarget = bestEquipable;

        // Actualizar texto de interacción
        if (_currentTarget != null)
        {
            _lastInteractionText = _currentTarget.GetInteractionText();
        }
        else if (_currentEquipableTarget != null)
        {
            _lastInteractionText = "Equipar arma";
        }
        else
        {
            _lastInteractionText = "No interactuable";
        }

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
        // Prioridad: IInteractable sobre IEquipable
        if (_currentTarget != null)
        {
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
        else if (_currentEquipableTarget != null && _equipmentController != null)
        {
            // Equipar arma si no hay nada interactuable
            _currentEquipableTarget.Equip(_equipmentController);
            _currentEquipableTarget = null;
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
        return _currentTarget != null || _currentEquipableTarget != null;
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
