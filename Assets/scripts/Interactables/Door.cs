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
    
    private void Update()
    {
        // Rotación suave en eje Y local
        _currentAngle = Mathf.Lerp(_currentAngle, _targetAngle, Time.deltaTime * _openSpeed);
        transform.localRotation = Quaternion.Euler(0f, _currentAngle, 0f);
    }
    
    public void Open()
    {
        if (_isLocked)
        {
            Debug.Log("La puerta está bloqueada");
            return;
        }
        
        _isOpen = true;
        _targetAngle = _openAngle;
    }
    
    public void Close()
    {
        _isOpen = false;
        _targetAngle = 0f;
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