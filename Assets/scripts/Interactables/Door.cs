using UnityEngine;
using PrimeTween;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Data")]  
    public DoorData doorData;
    private DoorType _doorType;
    private string _requiredKey;
    
    
    // Estados
    private bool _isOpen;
    private bool _isLocked;
    private float _currentAngle;
    private Tween _currentTween;

    [SerializeField] private float _openAngle = 90f; 
    [SerializeField] private float _openSpeed = 0.5f;
   
    public bool IsLocked => _isLocked;
    public DoorType CurrentDoorType => _doorType;

    private void Awake()
    {
        _requiredKey = doorData.openKey.ToString();
        _doorType = doorData.doorType;
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

        if (keyId != _requiredKey)
        {
            return;
        }

        _isLocked = false;
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
}