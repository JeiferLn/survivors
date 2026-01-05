using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

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

    private Coroutine _dialogueSequenceCoroutine;

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

        // Detener cualquier secuencia de diálogo anterior
        if (_dialogueSequenceCoroutine != null)
        {
            StopCoroutine(_dialogueSequenceCoroutine);
        }

        // Iniciar la secuencia completa de diálogo
        _dialogueSequenceCoroutine = StartCoroutine(HandleInteractionSequence());
    }

    private IEnumerator HandleInteractionSequence()
    {
        UpdateQuestStatus();

        // Si la misión está Locked o Available, mostrar diálogos "before" primero
        if (
            _currentQuestStatus == QuestStatus.Locked
            || _currentQuestStatus == QuestStatus.Available
        )
        {
            // Mostrar diálogos "before quest"
            yield return StartCoroutine(ShowDialogueSequence(_dialogueBeforeQuest));

            // Actualizar estado después de mostrar los diálogos
            UpdateQuestStatus();

            // Intentar activar la misión después de mostrar los diálogos "before"
            if (
                _currentQuestStatus == QuestStatus.Locked
                || _currentQuestStatus == QuestStatus.Available
            )
            {
                if (QuestManager.Instance != null && (!_oneTimeOnly || !_questActivated))
                {
                    bool activated = QuestManager.Instance.TryActivateQuest(questToGive);
                    if (activated)
                    {
                        _questActivated = true;
                        UpdateQuestStatus();
                        Debug.Log($"✅ Misión activada: {questToGive.QuestName}");

                        // Mostrar diálogos "after quest" si se activó correctamente
                        yield return StartCoroutine(ShowDialogueSequence(_dialogueAfterQuest));
                    }
                }
            }
        }
        else
        {
            // Para otros estados, mostrar el diálogo correspondiente
            string[] dialogueToShow = GetDialogueLines();
            yield return StartCoroutine(ShowDialogueSequence(dialogueToShow));
        }

        _dialogueSequenceCoroutine = null;
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

        // Detener cualquier secuencia de diálogo anterior
        if (_dialogueSequenceCoroutine != null)
        {
            StopCoroutine(_dialogueSequenceCoroutine);
        }

        string[] dialogueToShow = GetDialogueLines();
        _dialogueSequenceCoroutine = StartCoroutine(ShowDialogueSequence(dialogueToShow));
    }

    private IEnumerator ShowDialogueSequence(string[] dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
            yield break;

        // Suscribirse al evento de fin de diálogo
        bool dialogueFinished = false;
        UnityAction onDialogueEnd = () =>
        {
            dialogueFinished = true;
        };

        foreach (string line in dialogueLines)
        {
            if (string.IsNullOrEmpty(line))
                continue;

            dialogueFinished = false;

            // Suscribirse al evento
            DialogueSystem.Instance.OnDialogueEnd.AddListener(onDialogueEnd);

            // Enviar el mensaje actual
            DialogueSystem.Instance.SendText(line);

            // Esperar a que termine el diálogo (se cierre automáticamente o manualmente)
            yield return new WaitUntil(() =>
                !DialogueSystem.Instance.IsDialogueActive || dialogueFinished
            );

            // Desuscribirse del evento
            DialogueSystem.Instance.OnDialogueEnd.RemoveListener(onDialogueEnd);

            // Pequeña pausa entre mensajes
            yield return new WaitForSeconds(0.1f);
        }

        _dialogueSequenceCoroutine = null;
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
