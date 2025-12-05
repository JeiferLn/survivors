using PrimeTween;
using UnityEngine;
using Sirenix.OdinInspector;

public class Door : MonoBehaviour, IOpenable, ILockable, IKeyLockable, IInteractable
{
    public enum DoorType
    {
        Normal,         // Puerta simple
        Locked,         // Bloqueada (se desbloquea con switch, evento, etc.)
        KeyRequired     // Requiere llave específica
    }
    
    // ══════════════════════════════════════════════════════════════
    // CONFIGURACIÓN PRINCIPAL
    // ══════════════════════════════════════════════════════════════
    
    [Title("Tipo de Puerta")]
    [EnumToggleButtons]
    [SerializeField] private DoorType _doorType = DoorType.Normal;
    
    [Title("Animación")]
    [SerializeField] private float _openAngle = 90f;
    [SerializeField, Range(0.1f, 3f)] private float _openSpeed = 0.5f;
    
    // ══════════════════════════════════════════════════════════════
    // CONFIGURACIÓN - PUERTA BLOQUEADA
    // ══════════════════════════════════════════════════════════════
    
    [Title("Configuración de Bloqueo")]
    [ShowIf("_doorType", DoorType.Locked)]
    [SerializeField] private bool _startsLocked = true;
    
    [ShowIf("_doorType", DoorType.Locked)]
    [SerializeField] private string _lockedMessage = "La puerta está bloqueada";
    
    // ══════════════════════════════════════════════════════════════
    // CONFIGURACIÓN - PUERTA CON LLAVE
    // ══════════════════════════════════════════════════════════════
    
    [Title("Configuración de Llave")]
    [ShowIf("_doorType", DoorType.KeyRequired)]
    [SerializeField] private string _requiredKeyId = "key_01";
    
    [ShowIf("_doorType", DoorType.KeyRequired)]
    [SerializeField] private string _noKeyMessage = "Necesitas una llave";
    
    [ShowIf("_doorType", DoorType.KeyRequired)]
    [SerializeField] private string _wrongKeyMessage = "Llave incorrecta";
    
    // ══════════════════════════════════════════════════════════════
    // ESTADO INTERNO
    // ══════════════════════════════════════════════════════════════
    
    [Title("Debug (Solo Lectura)")]
    [ShowInInspector, ReadOnly]
    private bool _isOpen = false;
    
    [ShowInInspector, ReadOnly]
    private bool _isLocked = false;
    
    private float _currentAngle = 0f;
    private Tween _currentTween;
    
    // ══════════════════════════════════════════════════════════════
    // PROPIEDADES PÚBLICAS
    // ══════════════════════════════════════════════════════════════
    
    public bool IsOpen => _isOpen;
    public bool IsLocked => _isLocked;
    public string RequiredKeyId => _requiredKeyId;
    public DoorType CurrentDoorType => _doorType;
    
    // ══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════
    
    private void Awake()
    {
        InitializeDoor();
    }
    
    private void InitializeDoor()
    {
        switch (_doorType)
        {
            case DoorType.Normal:
                _isLocked = false;
                break;
                
            case DoorType.Locked:
                _isLocked = _startsLocked;
                break;
                
            case DoorType.KeyRequired:
                _isLocked = true; // Siempre empieza bloqueada
                break;
        }
    }
    
    // ══════════════════════════════════════════════════════════════
    // ANIMACIÓN
    // ══════════════════════════════════════════════════════════════
    
    private void AnimateDoor(float targetAngle)
    {
        // Cancelar animación anterior si existe
        if (_currentTween.isAlive)
            _currentTween.Stop();
        
        _currentTween = Tween.Custom(
            startValue: _currentAngle,
            endValue: targetAngle,
            duration: _openSpeed,
            ease: Ease.OutCirc,
            onValueChange: value => 
            {
                _currentAngle = value;
                transform.localRotation = Quaternion.Euler(0f, value, 0f);
            }
        );
    }
    
    // ══════════════════════════════════════════════════════════════
    // IOpenable
    // ══════════════════════════════════════════════════════════════
    
    public void Open()
    {
        if (_isOpen) return;
        
        if (_isLocked)
        {
            OnDoorLocked();
            return;
        }
        
        _isOpen = true;
        AnimateDoor(_openAngle);
        Debug.Log("🚪 Puerta abierta");
    }
    
    public void Close()
    {
        if (!_isOpen) return;
        
        _isOpen = false;
        AnimateDoor(0f);
        Debug.Log("🚪 Puerta cerrada");
    }
    
    // ══════════════════════════════════════════════════════════════
    // ILockable
    // ══════════════════════════════════════════════════════════════
    
    public void Lock()
    {
        if (_doorType == DoorType.Normal) return;
        
        _isLocked = true;
        Debug.Log("🔒 Puerta bloqueada");
    }
    
    public void Unlock()
    {
        if (_doorType == DoorType.KeyRequired)
        {
            Debug.Log("❌ Esta puerta requiere una llave específica");
            return;
        }
        
        _isLocked = false;
        Debug.Log("🔓 Puerta desbloqueada");
    }
    
    // ══════════════════════════════════════════════════════════════
    // IKeyLockable
    // ══════════════════════════════════════════════════════════════
    
    public bool HasValidKey(string keyId)
    {
        return _doorType == DoorType.KeyRequired && keyId == _requiredKeyId;
    }
    
    public bool TryUnlock(string keyId)
    {
        if (_doorType != DoorType.KeyRequired)
        {
            Debug.Log("Esta puerta no requiere llave");
            return false;
        }
        
        if (HasValidKey(keyId))
        {
            _isLocked = false;
            Debug.Log($"🔓 ¡Llave correcta! Puerta desbloqueada");
            return true;
        }
        
        Debug.Log($"❌ {_wrongKeyMessage}");
        return false;
    }
    
    // ══════════════════════════════════════════════════════════════
    // IInteractable
    // ══════════════════════════════════════════════════════════════
    
    public void Interact()
    {
        if (_isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }
    
    /// <summary>
    /// Interactuar con una llave específica (para puertas KeyRequired)
    /// </summary>
    public void InteractWithKey(string keyId)
    {
        if (_doorType == DoorType.KeyRequired && _isLocked)
        {
            if (TryUnlock(keyId))
            {
                Open();
            }
        }
        else
        {
            Interact();
        }
    }
    
    public string GetInteractionText()
    {
        if (_isLocked)
        {
            return _doorType switch
            {
                DoorType.Locked => _lockedMessage,
                DoorType.KeyRequired => $"{_noKeyMessage} [{_requiredKeyId}]",
                _ => "Bloqueada"
            };
        }
        
        return _isOpen ? "Cerrar puerta" : "Abrir puerta";
    }
    
    // ══════════════════════════════════════════════════════════════
    // MÉTODOS PRIVADOS
    // ══════════════════════════════════════════════════════════════
    
    private void OnDoorLocked()
    {
        switch (_doorType)
        {
            case DoorType.Locked:
                Debug.Log($"🔒 {_lockedMessage}");
                break;
                
            case DoorType.KeyRequired:
                Debug.Log($"🔑 {_noKeyMessage}");
                break;
        }
    }
    
    // ══════════════════════════════════════════════════════════════
    // BOTONES DE DEBUG (ODIN)
    // ══════════════════════════════════════════════════════════════
    
    #if UNITY_EDITOR
    [Title("Testing")]
    [Button("Test Open"), ShowIf("@UnityEngine.Application.isPlaying")]
    private void TestOpen() => Open();
    
    [Button("Test Close"), ShowIf("@UnityEngine.Application.isPlaying")]
    private void TestClose() => Close();
    
    [Button("Test Unlock"), ShowIf("@UnityEngine.Application.isPlaying && _doorType == DoorType.Locked")]
    private void TestUnlock() => Unlock();
    
    [Button("Test Key Unlock"), ShowIf("@UnityEngine.Application.isPlaying && _doorType == DoorType.KeyRequired")]
    private void TestKeyUnlock() => TryUnlock(_requiredKeyId);
    #endif
}