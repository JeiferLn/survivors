
using UnityEngine;
using PrimeTween;

public class BillboardFadeInOut : MonoBehaviour
{
    private Material parentMaterial;

    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float fadeAmount = 0.5f;

    private void Awake()
    {
        parentMaterial = GetComponent<MeshRenderer>().material;
    }
    
    private void OnEnable()
    {
        OnFadeIn();
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