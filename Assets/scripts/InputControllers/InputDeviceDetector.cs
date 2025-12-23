using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputDeviceDetector : MonoBehaviour
{
    [SerializeField]
    private TMP_Text promptText;

    [SerializeField]
    private string actionName = "Interact"; // Nombre de tu acción

    private PlayerInput playerInput;
    private InputAction targetAction;

    void Start()
    {
        playerInput = FindObjectOfType<PlayerInput>();

        if (playerInput != null)
        {
            targetAction = playerInput.actions[actionName];
            playerInput.onControlsChanged += OnControlsChanged;
            UpdatePromptDisplay();
        }
    }

    void OnControlsChanged(PlayerInput input)
    {
        UpdatePromptDisplay();
    }

    void UpdatePromptDisplay()
    {
        if (targetAction == null)
            return;

        // Obtener el esquema de control actual
        string controlScheme = playerInput.currentControlScheme;

        // Obtener el binding correcto según el dispositivo
        string displayString = GetBindingForCurrentScheme();

        promptText.text = $"[{displayString}] Interactuar";
    }

    string GetBindingForCurrentScheme()
    {
        // Buscar el binding que coincida con el control scheme actual
        for (int i = 0; i < targetAction.bindings.Count; i++)
        {
            var binding = targetAction.bindings[i];

            // Verificar si el binding pertenece al esquema actual
            if (
                string.IsNullOrEmpty(binding.groups)
                || binding.groups.Contains(playerInput.currentControlScheme)
            )
            {
                return targetAction.GetBindingDisplayString(i);
            }
        }

        return "?";
    }

    void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.onControlsChanged -= OnControlsChanged;
        }
    }
}
