public interface IDialogueable
{
    /// <summary>
    /// Inicia el diálogo con el objeto
    /// </summary>
    void StartDialogue();

    /// <summary>
    /// Texto o diálogo que se mostrará
    /// </summary>
    string GetDialogueText();

    /// <summary>
    /// ¿Tiene diálogo disponible?
    /// </summary>
    bool HasDialogue { get; }

    /// <summary>
    /// Nombre o identificador del objeto que habla (opcional, para UI)
    /// </summary>
    string SpeakerName { get; }
}
