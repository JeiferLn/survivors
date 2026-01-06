using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class QuestGiverNPC : MonoBehaviour, IInteractable, IDialogueable
{
    [Title("Misión")]
    [SerializeField]
    private NpcId npcID;

    [SerializeField]
    private QuestDefinition questID;

    [SerializeField]
    private bool _oneTimeOnly = true;

    // ══════════════════════════════════════════════════════════════
    // CONFIGURACIÓN DE DIÁLOGO
    // ══════════════════════════════════════════════════════════════

    [Title("Diálogo")]
    [SerializeField]
    private string _npcName;

    [SerializeField, TextArea(3, 5)]
    private string[] _dialogueBeforeQuest = new string[] { };

    [SerializeField, TextArea(3, 5)]
    private string[] _dialogueAfterQuest = new string[] { };

    [SerializeField, TextArea(3, 5)]
    private string[] _dialogueQuestActive = new string[] { };

    [SerializeField, TextArea(3, 5)]
    private string[] _dialogueQuestCompleted = new string[] { };

    [SerializeField, TextArea(3, 5)]
    private string[] _dialogueQuestLocked = new string[] { };

    // ══════════════════════════════════════════════════════════════
    // ESTADO INTERNO
    // ══════════════════════════════════════════════════════════════

    private bool _questActivated = false;
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
        if (questID == null || QuestManager.Instance == null)
            return;

        var state = QuestManager.Instance.GetQuestState(questID.QuestId);
        _currentQuestStatus = state.Status;
    }

    // ══════════════════════════════════════════════════════════════
    // IInteractable
    // ══════════════════════════════════════════════════════════════

    public void Interact()
    {
        if (questID == null && npcID == null)
        {
            Debug.LogWarning(
                $"[QuestGiverNPC] {_npcName} no tiene una misión asignada ni un NPC asignado."
            );
            StartDialogue();
            return;
        }

        UpdateQuestStatus();

        if (_dialogueSequenceCoroutine != null)
        {
            StopCoroutine(_dialogueSequenceCoroutine);
        }

        _dialogueSequenceCoroutine = StartCoroutine(HandleInteractionSequence());
    }

    private IEnumerator HandleInteractionSequence()
    {
        UpdateQuestStatus();

        if (npcID != null && QuestManager.Instance != null)
        {
            QuestManager.Instance.DispatchEvent(QuestType.TalkToNPC, npcID);
        }

        if (
            _currentQuestStatus == QuestStatus.Locked
            || _currentQuestStatus == QuestStatus.Available
        )
        {
            yield return StartCoroutine(ShowDialogueSequence(_dialogueBeforeQuest));
            UpdateQuestStatus();

            if (
                _currentQuestStatus == QuestStatus.Locked
                || _currentQuestStatus == QuestStatus.Available
            )
            {
                if (QuestManager.Instance != null && (!_oneTimeOnly || !_questActivated))
                {
                    bool activated = QuestManager.Instance.TryActivateQuest(questID);
                    if (activated)
                    {
                        _questActivated = true;
                        UpdateQuestStatus();

                        yield return StartCoroutine(ShowDialogueSequence(_dialogueAfterQuest));
                    }
                }
            }
        }
        else
        {
            string[] dialogueToShow = GetDialogueLines();
            yield return StartCoroutine(ShowDialogueSequence(dialogueToShow));
        }

        _dialogueSequenceCoroutine = null;
    }

    public string GetInteractionText()
    {
        if (questID == null)
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

            DialogueSystem.Instance.OnDialogueEnd.AddListener(onDialogueEnd);

            DialogueSystem.Instance.SendText(line);

            yield return new WaitUntil(() =>
                !DialogueSystem.Instance.IsDialogueActive || dialogueFinished
            );

            DialogueSystem.Instance.OnDialogueEnd.RemoveListener(onDialogueEnd);

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
        if (questID == null)
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
}
