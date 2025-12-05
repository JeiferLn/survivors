using PrimeTween;   
using UnityEngine;

public class Door : MonoBehaviour, IOpenable, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private bool _isLocked = false;
    [SerializeField] private float _openAngle = 90f;
    [SerializeField] private float _openSpeed = 3f;
    
    private bool _isOpen = false;
    private float _currentAngle = 0f;
    private float _targetAngle = 0f;
    
    public bool IsOpen => _isOpen;
    public bool IsLocked => _isLocked;
    
    private void AnimateDoor(float targetAngle)
    {
        Tween.Custom(
            startValue: _currentAngle,
            endValue: targetAngle,
            duration: _openSpeed,
            ease: Ease.OutCirc,
            onValueChange: value => 
            {
                _currentAngle = value;
                transform.localRotation = Quaternion.Euler(0f, value, 0f);
            }
        ).OnComplete(() => 
        {
            Debug.Log("Door is open!");
            _isOpen = !_isOpen;
        });
    }
    
    public void Open()
    {
        if (_isLocked)
        {
            Debug.Log("La puerta está bloqueada");
            return;
        }
        
        _targetAngle = _openAngle;
        AnimateDoor(_targetAngle);
    }
    
    public void Close()
    {
        _targetAngle = 0f;
        AnimateDoor(_targetAngle);
    }
    
    public void Interact()
    {
        if (_isOpen)
            Close();
        else
            Open();
    }
    
    public string GetInteractionText()
    {
        if (_isLocked)
            return "Puerta bloqueada";
        
        return _isOpen ? "Cerrar puerta" : "Abrir puerta";
    }
}