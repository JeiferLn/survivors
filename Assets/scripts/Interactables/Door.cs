using PrimeTween;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public enum DoorType
    {
        Normal,
        Locked,
        KeyRequired
    }

    [SerializeField]
    private DoorType _doorType;

    [SerializeField]
    private float _openAngle = 90f;

    [SerializeField]
    private float _openSpeed = 0.5f;

    public string _requiredKeyId = "key_01";
    public string _lockedMessage = "La puerta está bloqueada";
    public string _noKeyMessage = "Necesitas una llave";

    private bool _isOpen;
    private bool _isLocked;
    private float _currentAngle;
    private Tween _currentTween;

    public bool IsLocked => _isLocked;
    public string RequiredKeyId => _requiredKeyId;
    public DoorType CurrentDoorType => _doorType;

    private void Awake()
    {
        _isLocked = _doorType != DoorType.Normal;
    }

    public void Interact()
    {
        if (_isLocked)
            return;

        ToggleDoor();
    }

    public void InteractWithKey(string keyId)
    {
        if (_doorType != DoorType.KeyRequired)
        {
            ToggleDoor();
            return;
        }

        if (keyId != _requiredKeyId)
        {
            Debug.Log(_noKeyMessage);
            return;
        }

        _isLocked = false;
        Debug.Log($"Usaste la llave {_requiredKeyId}");
        ToggleDoor();
    }

    private void ToggleDoor()
    {
        _isOpen = !_isOpen;
        AnimateDoor(_isOpen ? _openAngle : 0f);
    }

    private void AnimateDoor(float targetAngle)
    {
        if (_currentTween.isAlive)
            _currentTween.Stop();

        _currentTween = Tween.Custom(
            _currentAngle,
            targetAngle,
            _openSpeed,
            value =>
            {
                _currentAngle = value;
                transform.localRotation = Quaternion.Euler(0f, value, 0f);
            }
        );
    }

    public string GetInteractionText()
    {
        if (_isLocked)
            return _doorType == DoorType.KeyRequired
                ? $"{_noKeyMessage} [{_requiredKeyId}]"
                : _lockedMessage;

        return _isOpen ? "Cerrar puerta" : "Abrir puerta";
    }
}