
using UnityEngine;
using PrimeTween;

public class BillboardFadeInOut : MonoBehaviour
{
    
    [SerializeField] private bool isCrosshair;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float fadeAmount = 0.5f;
    
    private Material parentMaterial;
    
    private void Awake()
    {
        parentMaterial = GetComponent<MeshRenderer>().material;
        if (isCrosshair) BillboardManager.Instance.Register(gameObject.transform);
    }
    
    private void OnEnable()
    {
        OnFadeIn();
    }

    private void OnDisable()
    {
        if (isCrosshair) BillboardManager.Instance.Unregister(gameObject.transform);
    }

    private void OnFadeIn()
    {
        Tween.MaterialAlpha(parentMaterial, fadeAmount, fadeDuration)
            .SetEase(Ease.Linear)
            .Play();
    }
    
    public void OnFadeOut()
    {
        Tween.MaterialAlpha(parentMaterial, 0f, fadeDuration/2)
            .SetEase(Ease.Linear)
            .Play()
            .OnComplete(() => gameObject.SetActive(false));
    }
}