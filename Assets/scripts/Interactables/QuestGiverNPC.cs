using Sirenix.OdinInspector;
using UnityEngine;

public class QuestGiverNPC : MonoBehaviour, IInteractable, IDialogueable
{
    [Title("Misión")]
    [SerializeField, Tooltip("La misión que este NPC puede dar al jugador")]
    private QuestDefinition questToGive;

    [SerializeField, Tooltip("Si es true, la misión solo se puede activar una vez")]
    private bool _oneTimeOnly = true;

    // ══════════════════════════════════════════════════════════════
    // CONFIGURACIÓN DE DIÁLOGO
    // ══════════════════════════════════════════════════════════════

    [Title("Diálogo")]
    [SerializeField, Tooltip("Nombre del NPC (se mostrará en el diálogo)")]
    private string _npcName = "NPC";

    [SerializeField, TextArea(3, 5), Tooltip("Texto de diálogo antes de activar la misión")]
    private string[] _dialogueBeforeQuest = new string[] { "Hola, ¿puedes ayudarme?" };

    [SerializeField, TextArea(3, 5), Tooltip("Texto de diálogo después de activar la misión")]
    private string[] _dialogueAfterQuest = new string[] { "¡Gracias! Buena suerte con la misión." };

    [SerializeField, TextArea(3, 5), Tooltip("Texto si la misión ya está activa")]
    private string[] _dialogueQuestActive = new string[]
    {
        "Ya te di esa misión. ¡Ve a completarla!",
    };

    [SerializeField, TextArea(3, 5), Tooltip("Texto si la misión ya está completada")]
    private string[] _dialogueQuestCompleted = new string[] { "¡Gracias por completar la misión!" };

    [
        SerializeField,
        TextArea(3, 5),
        Tooltip("Texto si la misión no se puede activar (requisitos no cumplidos)")
    ]
    private string[] _dialogueQuestLocked = new string[] { "Aún no puedes recibir esta misión." };

    // ══════════════════════════════════════════════════════════════
    // ESTADO INTERNO
    // ══════════════════════════════════════════════════════════════

    [Title("Debug (Solo Lectura)")]
    [ShowInInspector, ReadOnly]
    private bool _questActivated = false;

    [ShowInInspector, ReadOnly]
    private QuestStatus _currentQuestStatus = QuestStatus.Locked;

    // ══════════════════════════════════════════════════════════════
    // PROPIEDADES PÚBLICAS
    // ══════════════════════════════════════════════════════════════

    public bool HasDialogue => true;
    public string SpeakerName => _npcName;

    // ══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════

    private void Start()
    {
        UpdateQuestStatus();
    }

    private void UpdateQuestStatus()
    {
        if (questToGive == null || QuestManager.Instance == null)
            return;

        var state = QuestManager.Instance.GetQuestState(questToGive.QuestId);
        _currentQuestStatus = state.Status;
    }

    // ══════════════════════════════════════════════════════════════
    // IInteractable
    // ══════════════════════════════════════════════════════════════

    public void Interact()
    {
        if (questToGive == null)
        {
            Debug.LogWarning($"[QuestGiverNPC] {_npcName} no tiene una misión asignada.");
            StartDialogue();
            return;
        }

        UpdateQuestStatus();

        // Intentar activar la misión si es posible
        if (_currentQuestStatus == QuestStatus.Locked && (!_oneTimeOnly || !_questActivated))
        {
            if (QuestManager.Instance != null)
            {
                bool activated = QuestManager.Instance.TryActivateQuest(questToGive);
                if (activated)
                {
                    _questActivated = true;
                    UpdateQuestStatus();
                    Debug.Log($"✅ Misión activada: {questToGive.QuestName}");
                }
            }
        }

        // Mostrar diálogo apropiado
        StartDialogue();
    }

    public string GetInteractionText()
    {
        if (questToGive == null)
            return $"Hablar con {_npcName}";

        UpdateQuestStatus();

        return _currentQuestStatus switch
        {
            QuestStatus.Locked => $"Hablar con {_npcName}",
            QuestStatus.Active => $"Hablar con {_npcName} (Misión activa)",
            QuestStatus.Completed => $"Hablar con {_npcName} (Misión completada)",
            _ => $"Hablar con {_npcName}",
        };
    }

    // ══════════════════════════════════════════════════════════════
    // IDialogueable
    // ══════════════════════════════════════════════════════════════

    public void StartDialogue()
    {
        if (DialogueSystem.Instance == null)
        {
            Debug.LogWarning("[QuestGiverNPC] DialogueSystem.Instance es null.");
            return;
        }

        string[] dialogueToShow = GetDialogueLines();
        DialogueSystem.Instance.SendText(dialogueToShow);
    }

    public string GetDialogueText()
    {
        string[] lines = GetDialogueLines();
        return string.Join("\n", lines);
    }

    private string[] GetDialogueLines()
    {
        if (questToGive == null)
            return _dialogueBeforeQuest;

        UpdateQuestStatus();

        return _currentQuestStatus switch
        {
            QuestStatus.Locked => _dialogueQuestLocked,
            QuestStatus.Active => _dialogueQuestActive,
            QuestStatus.Completed => _dialogueQuestCompleted,
            _ => _dialogueBeforeQuest,
        };
    }

    // ══════════════════════════════════════════════════════════════
    // BOTONES DE DEBUG (ODIN)
    // ══════════════════════════════════════════════════════════════

#if UNITY_EDITOR
    [Title("Testing")]
    [Button("Test Interact"), ShowIf("@UnityEngine.Application.isPlaying")]
    private void TestInteract() => Interact();

    [Button("Test Dialogue"), ShowIf("@UnityEngine.Application.isPlaying")]
    private void TestDialogue() => StartDialogue();

    [Button("Update Quest Status"), ShowIf("@UnityEngine.Application.isPlaying")]
    private void TestUpdateStatus() => UpdateQuestStatus();
#endif
}
