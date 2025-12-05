
using Febucci.UI;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class DescriptionTexts: MonoBehaviour
{
    
    TextMeshProUGUI tmpText;
    private string _descriptionText;
    private TextAnimatorPlayer txtAnimatorPlayer;
    
    private void Start()
    {
        _descriptionText = "La puerta está cerrada";
        tmpText = GetComponent<TextMeshProUGUI>();
        txtAnimatorPlayer = tmpText.GetComponent<TextAnimatorPlayer>(); 
    }

    [Button("Skip Descripción")]   
    private void SkipDescription()
    {
        txtAnimatorPlayer.SkipTypewriter();
    }
    
    [Button("Mostrar Descripción")]   
    private void ShowDescription()
    {
        tmpText.text = _descriptionText;
    }
    
    [Button("Ocultar Descripción")] 
    private void HideDescription()
    {
        tmpText.text = "";
    }
}
