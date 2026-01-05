using System;
using System.Collections.Generic;
using Febucci.UI;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════
    // REFERENCIAS
    // ══════════════════════════════════════════════════════════════

    public static DialogueSystem Instance { get; private set; }

    private TextMeshProUGUI _tmpText;
    private TextAnimatorPlayer _textAnimatorPlayer;

    [SerializeField]
    private GameObject _dialoguePanel;

    [SerializeField, Tooltip("Indicador visual de 'presiona para continuar'")]
    private GameObject _continueIndicator;

    [SerializeField, Tooltip("Imagen de fondo del cuadro de dialogo")]
    private Image backImage;

    // ══════════════════════════════════════════════════════════════
    // CONFIGURACIÓN
    // ══════════════════════════════════════════════════════════════

    [Title("Configuración de Texto")]
    [SerializeField, Range(1, 4)]
    private int _maxLinesPerPage = 2;

    [SerializeField, Range(10, 100), Tooltip("Caracteres máximos por línea (aproximado)")]
    private int _maxCharsPerLine = 50;

    [Title("Controles")]
    [SerializeField]
    private KeyCode _continueKey = KeyCode.Space;

    [SerializeField, Tooltip("También permitir click para continuar")]
    private bool _allowMouseClick = true;

    // ══════════════════════════════════════════════════════════════
    // EVENTOS DE UNITY
    // ══════════════════════════════════════════════════════════════

    [Title("Eventos")]
    [FoldoutGroup("Unity Events")]
    [LabelText("Al Recibir Texto")]
    public UnityEvent<string> OnReceiveText;

    [FoldoutGroup("Unity Events")]
    [LabelText("Al Iniciar Diálogo")]
    public UnityEvent OnDialogueStart;

    [FoldoutGroup("Unity Events")]
    [LabelText("Al Terminar Diálogo")]
    public UnityEvent OnDialogueEnd;

    [FoldoutGroup("Unity Events")]
    [LabelText("Al Completar Página")]
    public UnityEvent OnPageComplete;

    [FoldoutGroup("Unity Events")]
    [LabelText("Al Cambiar Página")]
    public UnityEvent<int, int> OnPageChanged; // (currentPage, totalPages)

    // ══════════════════════════════════════════════════════════════
    // ESTADO INTERNO
    // ══════════════════════════════════════════════════════════════

    [Title("Debug (Solo Lectura)")]
    [ShowInInspector, ReadOnly]
    private bool _isDialogueActive = false;

    [ShowInInspector, ReadOnly]
    private bool _isTyping = false;

    [ShowInInspector, ReadOnly, LabelText("Página Actual")]
    private int _currentPageIndex = 0;

    [ShowInInspector, ReadOnly, LabelText("Total Páginas")]
    private int _totalPages = 0;

    [ShowInInspector, ReadOnly, ProgressBar(0, 1)]
    private float _progress = 0f;

    private List<string> _pages = new List<string>();

    // ══════════════════════════════════════════════════════════════
    // PROPIEDADES PÚBLICAS
    // ══════════════════════════════════════════════════════════════

    public bool IsDialogueActive => _isDialogueActive;
    public bool IsTyping => _isTyping;
    public int CurrentPage => _currentPageIndex + 1;
    public int TotalPages => _totalPages;
    public float Progress => _progress;

    // ══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════

    private void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        _tmpText = GetComponent<TextMeshProUGUI>();
        _textAnimatorPlayer = GetComponent<TextAnimatorPlayer>();

        ValidateReferences();
        SetupTextAnimatorEvents();
        SetPanelActive(false);
    }

    private void OnDestroy()
    {
        CleanupTextAnimatorEvents();
    }

    private void Update()
    {
        if (!_isDialogueActive)
            return;

        HandleInput();
    }

    // ══════════════════════════════════════════════════════════════
    // SETUP
    // ══════════════════════════════════════════════════════════════

    private void ValidateReferences()
    {
        if (_tmpText == null)
            _tmpText = GetComponent<TextMeshProUGUI>();

        if (_textAnimatorPlayer == null && _tmpText != null)
            _textAnimatorPlayer = _tmpText.GetComponent<TextAnimatorPlayer>();
    }

    private void SetupTextAnimatorEvents()
    {
        if (_textAnimatorPlayer != null)
        {
            _textAnimatorPlayer.onTextShowed.AddListener(OnTypewriterComplete);
            _textAnimatorPlayer.onTypewriterStart.AddListener(OnTypewriterStart);
        }
    }

    private void CleanupTextAnimatorEvents()
    {
        if (_textAnimatorPlayer != null)
        {
            _textAnimatorPlayer.onTextShowed.RemoveListener(OnTypewriterComplete);
            _textAnimatorPlayer.onTypewriterStart.RemoveListener(OnTypewriterStart);
        }
    }

    // ══════════════════════════════════════════════════════════════
    // INPUT
    // ══════════════════════════════════════════════════════════════

    private void HandleInput()
    {
        bool continuePressed = Input.GetKeyDown(_continueKey);
        bool mouseClicked = _allowMouseClick && Input.GetMouseButtonDown(0);

        if (continuePressed || mouseClicked)
        {
            HandleContinue();
        }
    }

    private void HandleContinue()
    {
        if (_isTyping)
        {
            SkipCurrentPage();
        }
        else
        {
            NextPage();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // API PÚBLICA - INICIAR DIÁLOGO
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Inicia el diálogo con un texto. Divide automáticamente en páginas.
    /// </summary>
    [Button("Enviar Texto", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0.4f)]
    public void SendText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        OnReceiveText?.Invoke(text);
        StartDialogue(text);
    }

    /// <summary>
    /// Inicia el diálogo con múltiples líneas.
    /// </summary>
    public void SendText(string[] lines)
    {
        if (lines == null || lines.Length == 0)
            return;

        string combined = string.Join("\n", lines);
        SendText(combined);
    }

    private void StartDialogue(string text)
    {
        // Limpiar estado anterior
        _pages.Clear();
        _currentPageIndex = 0;

        // Dividir texto en páginas
        _pages = SplitTextIntoPages(text);
        _totalPages = _pages.Count;

        // Activar diálogo
        _isDialogueActive = true;
        SetPanelActive(true);
        UpdateProgress();

        OnDialogueStart?.Invoke();

        // Mostrar primera página
        ShowCurrentPage();
    }

    // ══════════════════════════════════════════════════════════════
    // API PÚBLICA - CONTROL
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Salta la animación de la página actual.
    /// </summary>
    [Button("Saltar Página")]
    public void SkipCurrentPage()
    {
        if (_textAnimatorPlayer != null && _isTyping)
        {
            _textAnimatorPlayer.SkipTypewriter();
        }
    }

    /// <summary>
    /// Avanza a la siguiente página.
    /// </summary>
    [Button("Siguiente Página")]
    public void NextPage()
    {
        if (!_isDialogueActive || _isTyping)
            return;

        _currentPageIndex++;

        if (_currentPageIndex >= _totalPages)
        {
            EndDialogue();
        }
        else
        {
            UpdateProgress();
            OnPageChanged?.Invoke(_currentPageIndex + 1, _totalPages);
            ShowCurrentPage();
        }
    }

    /// <summary>
    /// Fuerza el cierre del diálogo.
    /// </summary>
    [Button("Cerrar Diálogo"), GUIColor(0.8f, 0.4f, 0.4f)]
    public void ForceEndDialogue()
    {
        EndDialogue();
    }

    // ══════════════════════════════════════════════════════════════
    // LÓGICA DE PAGINACIÓN
    // ══════════════════════════════════════════════════════════════

    private List<string> SplitTextIntoPages(string text)
    {
        List<string> pages = new List<string>();

        // Primero dividir por saltos de línea explícitos
        string[] paragraphs = text.Split(new[] { "\n", "\\n" }, StringSplitOptions.None);

        List<string> allLines = new List<string>();

        // Procesar cada párrafo
        foreach (string paragraph in paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph))
            {
                allLines.Add(""); // Mantener líneas vacías como separadores
                continue;
            }

            // Si el párrafo es muy largo, dividirlo
            List<string> wrappedLines = WrapText(paragraph);
            allLines.AddRange(wrappedLines);
        }

        // Agrupar líneas en páginas
        List<string> currentPageLines = new List<string>();

        foreach (string line in allLines)
        {
            currentPageLines.Add(line);

            if (currentPageLines.Count >= _maxLinesPerPage)
            {
                pages.Add(string.Join("\n", currentPageLines));
                currentPageLines.Clear();
            }
        }

        // Agregar líneas restantes
        if (currentPageLines.Count > 0)
        {
            pages.Add(string.Join("\n", currentPageLines));
        }

        return pages;
    }

    private List<string> WrapText(string text)
    {
        List<string> lines = new List<string>();

        if (text.Length <= _maxCharsPerLine)
        {
            lines.Add(text);
            return lines;
        }

        // Dividir por palabras
        string[] words = text.Split(' ');
        string currentLine = "";

        foreach (string word in words)
        {
            string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;

            if (testLine.Length > _maxCharsPerLine && !string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine.Trim());
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine.Trim());
        }

        return lines;
    }

    // ══════════════════════════════════════════════════════════════
    // MOSTRAR TEXTO
    // ══════════════════════════════════════════════════════════════

    private void ShowCurrentPage()
    {
        if (_currentPageIndex >= _pages.Count)
            return;

        string pageText = _pages[_currentPageIndex];

        SetContinueIndicator(false);

        // Usar Text Animator para mostrar el texto
        if (_textAnimatorPlayer != null)
        {
            _tmpText.text = pageText;
            _textAnimatorPlayer.ShowText(pageText);
        }
        else
        {
            _tmpText.text = pageText;
            OnTypewriterComplete(); // Si no hay animator, marcar como completo
        }
    }

    // ══════════════════════════════════════════════════════════════
    // CALLBACKS DE TEXT ANIMATOR
    // ══════════════════════════════════════════════════════════════

    private void OnTypewriterStart()
    {
        _isTyping = true;
        SetContinueIndicator(false);
    }

    private void OnTypewriterComplete()
    {
        _isTyping = false;
        SetContinueIndicator(true);
        OnPageComplete?.Invoke();
    }

    // ══════════════════════════════════════════════════════════════
    // FINALIZAR DIÁLOGO
    // ══════════════════════════════════════════════════════════════

    private void EndDialogue()
    {
        _isDialogueActive = false;
        _isTyping = false;
        _pages.Clear();
        _currentPageIndex = 0;
        _totalPages = 0;
        _progress = 0f;

        _tmpText.text = "";
        SetPanelActive(false);
        SetContinueIndicator(false);

        OnDialogueEnd?.Invoke();
    }

    // ══════════════════════════════════════════════════════════════
    // UTILIDADES UI
    // ══════════════════════════════════════════════════════════════

    private void SetPanelActive(bool active)
    {
        if (_dialoguePanel != null)
            _dialoguePanel.SetActive(active);

        UpdateOpacityOfBackImage(active);
    }

    private void UpdateOpacityOfBackImage(bool active)
    {
        float defOpacity = active ? 0.5f : 0;
        float defDuration = active ? 0.8f : 0f;

        Tween.Alpha(backImage, defOpacity, defDuration).SetEase(Ease.Linear);
    }

    private void SetContinueIndicator(bool visible)
    {
        if (_continueIndicator != null)
            _continueIndicator.SetActive(visible);
    }

    private void UpdateProgress()
    {
        _progress = _totalPages > 0 ? (float)(_currentPageIndex + 1) / _totalPages : 0f;
    }

    // ══════════════════════════════════════════════════════════════
    // TESTING
    // ══════════════════════════════════════════════════════════════

#if UNITY_EDITOR
    [Title("Testing")]
    [TextArea(3, 6)]
    [SerializeField]
    private string _testText =
        "Bienvenido aventurero a nuestra humilde tienda.\nTenemos los mejores productos de toda la región.\n¿Qué te gustaría comprar hoy?\nTenemos espadas, escudos, pociones y mucho más.";

    [Button("Probar Texto", ButtonSizes.Large), GUIColor(0.3f, 0.7f, 1f)]
    private void TestDialogue()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Solo funciona en Play Mode");
            return;
        }

        SendText(_testText);
    }

    [Button("Simular Páginas (Editor)")]
    private void PreviewPages()
    {
        var pages = SplitTextIntoPages(_testText);
        Debug.Log($"═══ Vista previa: {pages.Count} páginas ═══");

        for (int i = 0; i < pages.Count; i++)
        {
            Debug.Log($"📄 Página {i + 1}:\n{pages[i]}\n");
        }
    }
#endif
}
