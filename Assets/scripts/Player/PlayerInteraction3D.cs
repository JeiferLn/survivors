using UnityEngine;

public class PlayerInteraction3D : MonoBehaviour
{
    [SerializeField] private float _interactionRange = 3f;
    [SerializeField] private LayerMask _interactableLayer;
    
    private Camera _camera;
    
    private void Start()
    {
        _camera = Camera.main;
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }
    }
    
    private void TryInteract()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100f, _interactableLayer))
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            
            if (distance <= _interactionRange)
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
            else
            {
                Debug.Log("Muy lejos para interactuar");
            }
        }
    }
}