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

    private IInteractable _currentTarget;
    private IEquipable _currentEquipableTarget;
    private Outline _lastOutline;

    // ══════════════════════════════════════════════════════════════
    // CONTROL DE DIÁLOGO
    // ══════════════════════════════════════════════════════════════

    private float _timeSinceTextFinished = 0f;

    // ══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════

    private void Awake()
    {
        _equipmentController = GetComponent<PlayerEquipmentController>();
    }

    private void Update()
    {
        UpdateCurrentTarget();
        UpdateDialogueTimer();
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

        if (hits.Length == 0)
        {
            DisableLastTargetEffects();
            _currentTarget = null;
            _currentEquipableTarget = null;
            return;
        }

        List<Outline> outlinesInRange = new List<Outline>();

        float bestDist = float.MaxValue;
        IInteractable bestInteractable = null;
        IEquipable bestEquipable = null;
        Outline bestOutline = null;

        foreach (var hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            IEquipable equipable = hit.GetComponent<IEquipable>();
            Outline outline = hit.GetComponent<Outline>();

            if (outline != null)
                outlinesInRange.Add(outline);

            if (interactable == null && equipable == null)
                continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < bestDist)
            {
                if (interactable != null)
                {
                    bestDist = dist;
                    bestInteractable = interactable;
                    bestEquipable = null;
                    bestOutline = outline;
                }
                else if (equipable != null && bestInteractable == null)
                {
                    bestDist = dist;
                    bestEquipable = equipable;
                    bestOutline = outline;
                }
            }
        }

        _currentTarget = bestInteractable;
        _currentEquipableTarget = bestEquipable;

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
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive)
        {
            HandleDialogueInteraction();
            return;
        }

        if (_currentTarget != null)
        {
            Debug.Log($"[PlayerInteraction] Interactuando con: {_currentTarget.GetType().Name}");
            if (_currentTarget is Door door)
            {
                InteractWithDoor(door);
            }
            else
            {
                _currentTarget.Interact();
            }
        }
        else if (_currentEquipableTarget != null && _equipmentController != null)
        {
            _currentEquipableTarget.Equip(_equipmentController);
            _currentEquipableTarget = null;
        }
        else
        {
            Debug.Log("[PlayerInteraction] No hay objetivo válido para interactuar.");
        }
    }

    private void HandleDialogueInteraction()
    {
        DialogueSystem dialogueSystem = DialogueSystem.Instance;

        if (dialogueSystem == null)
            return;

        if (dialogueSystem.IsTyping)
        {
            dialogueSystem.SkipCurrentPage();
            _timeSinceTextFinished = 0f;
        }
        else if (!dialogueSystem.IsTyping)
        {
            dialogueSystem.ForceEndDialogue();
            _timeSinceTextFinished = 0f;
        }
    }

    private void UpdateDialogueTimer()
    {
        if (
            DialogueSystem.Instance != null
            && DialogueSystem.Instance.IsDialogueActive
            && !DialogueSystem.Instance.IsTyping
        )
        {
            _timeSinceTextFinished += Time.deltaTime;

            if (_timeSinceTextFinished >= 1f)
            {
                DialogueSystem.Instance.ForceEndDialogue();
                _timeSinceTextFinished = 0f;
            }
        }
        else
        {
            _timeSinceTextFinished = 0f;
        }
    }

    private void InteractWithDoor(Door door)
    {
        if (door is null)
            return;

        string msgDoorLocked = door._lockedMessage;
        string msgDoorNeedKey = door._noKeyMessage;

        if (door.CurrentDoorType == Door.DoorType.Locked)
        {
            Debug.Log(msgDoorLocked);
            return;
        }

        if (door.CurrentDoorType == Door.DoorType.KeyRequired && door.IsLocked)
        {
            string requiredKey = door.RequiredKeyId;

            if (HasKey(requiredKey))
            {
                door.InteractWithKey(requiredKey);
            }
            else
            {
                if (door.IsLocked)
                {
                    Debug.Log(msgDoorNeedKey);
                }
            }
        }
        else
        {
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
        }
    }

    public bool RemoveKey(string keyId)
    {
        return _keys.Remove(keyId);
    }

    public void ClearKeys()
    {
        _keys.Clear();
    }

    // ══════════════════════════════════════════════════════════════
    // GETTERS PÚBLICOS
    // ══════════════════════════════════════════════════════════════

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
}
