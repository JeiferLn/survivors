
using PrimeTween;
using UnityEngine;

public class ShowUI : MonoBehaviour
{
    private Tween blurEffect;
    private CanvasGroup cGroup;
    
    private void OnEnable()
    {
        cGroup = GetComponent<CanvasGroup>();
        blurEffect.Kill();
        cGroup.alpha = 0;
        blurEffect = Tween.Alpha(cGroup, 1f, 1f, Ease.OutBack);
    }

    [ContextMenu("Hide")]
    private void HideUI()
    {
        blurEffect.Kill();
        blurEffect = Tween.Alpha(cGroup, 0f, 0.5f, Ease.InBack);
        blurEffect.OnComplete(HideObject);
    }

    private void HideObject()
    {
        gameObject.SetActive(false);
    }
}
