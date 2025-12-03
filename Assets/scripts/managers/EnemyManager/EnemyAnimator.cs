using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controlador de animaciones para enemigos.
/// Maneja transiciones con CrossFadeInFixedTime y previene animaciones duplicadas.
/// </summary>
public class EnemyAnimator : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Animator animator;
    [SerializeField] private int animationLayer = 0;
    
    [Header("Transition Settings")]
    [SerializeField] private float defaultTransitionDuration = 0.15f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    // Tracking de animaciones
    private int currentAnimationHash;
    private int previousAnimationHash;
    private string currentAnimationName = string.Empty;
    private string previousAnimationName = string.Empty;
    
    // Cache de hashes para optimización
    private readonly Dictionary<string, int> animationHashCache = new();
    
    // Propiedades públicas
    public Animator Animator => animator;
    public string CurrentAnimation => currentAnimationName;
    public string PreviousAnimation => previousAnimationName;
    public bool IsTransitioning => animator != null && animator.IsInTransition(animationLayer);
    public bool IsInitialized => animator != null;

    
    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    #region ══════════ MÉTODOS PRINCIPALES ══════════

    /// <summary>
    /// Cambia a una nueva animación si es diferente a la actual.
    /// </summary>
    /// <param name="animationName">Nombre de la animación</param>
    /// <param name="transitionDuration">Duración de la transición (-1 = usar default)</param>
    /// <param name="normalizedTime">Tiempo normalizado donde inicia (0-1)</param>
    /// <returns>True si la animación cambió, False si ya estaba reproduciéndose</returns>
    public bool SetAnimation(string animationName, float transitionDuration = -1f, float normalizedTime = 0f)
    {
        if (!ValidateAnimator() || string.IsNullOrEmpty(animationName))
            return false;

        int newHash = GetAnimationHash(animationName);
        
        // Verificar si ya está en esta animación
        if (!HasAnimationChanged(newHash))
        {
            LogDebug($"Animación '{animationName}' ya activa, ignorando...");
            return false;
        }

        // Guardar estado previo
        StorePreviousState();
        
        // Actualizar estado actual
        currentAnimationHash = newHash;
        currentAnimationName = animationName;

        // Ejecutar transición
        float duration = transitionDuration < 0 ? defaultTransitionDuration : transitionDuration;
        animator.CrossFadeInFixedTime(newHash, duration, animationLayer, normalizedTime);

        LogDebug($"Animación: '{previousAnimationName}' → '{currentAnimationName}'");
        return true;
    }

    /// <summary>
    /// Fuerza una animación aunque ya esté reproduciéndose (reinicia).
    /// </summary>
    public bool ForceAnimation(string animationName, float transitionDuration = -1f)
    {
        if (!ValidateAnimator() || string.IsNullOrEmpty(animationName))
            return false;

        StorePreviousState();
        
        // Resetear para permitir el cambio
        currentAnimationHash = 0;
        currentAnimationName = string.Empty;
        
        return SetAnimation(animationName, transitionDuration);
    }

    #endregion

    #region ══════════ MÉTODOS DE VERIFICACIÓN ══════════

    /// <summary>
    /// Verifica si la animación actual es diferente al hash proporcionado.
    /// </summary>
    public bool HasAnimationChanged(int targetHash)
    {
        return currentAnimationHash != targetHash;
    }

    /// <summary>
    /// Verifica si la animación actual es diferente al nombre proporcionado.
    /// </summary>
    public bool HasAnimationChanged(string animationName)
    {
        return currentAnimationName != animationName;
    }

    /// <summary>
    /// Verifica si la animación previa era la especificada.
    /// </summary>
    public bool WasPreviousAnimation(string animationName)
    {
        return previousAnimationName == animationName;
    }

    /// <summary>
    /// Verifica si la animación actual es la especificada.
    /// </summary>
    public bool IsCurrentAnimation(string animationName)
    {
        return currentAnimationName == animationName;
    }

    /// <summary>
    /// Verifica si la animación actual terminó.
    /// </summary>
    public bool IsCurrentAnimationFinished(float threshold = 0.95f)
    {
        if (!ValidateAnimator() || IsTransitioning) 
            return false;
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(animationLayer);
        return stateInfo.normalizedTime >= threshold && !stateInfo.loop;
    }

    /// <summary>
    /// Obtiene el progreso de la animación actual (0-1).
    /// </summary>
    public float GetAnimationProgress()
    {
        if (!ValidateAnimator()) return 0f;
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(animationLayer);
        return Mathf.Clamp01(stateInfo.normalizedTime % 1f);
    }

    #endregion

    #region ══════════ MÉTODOS DE CONVENIENCIA ══════════

    /// <summary>
    /// Cambia animación solo si la actual coincide con la requerida.
    /// </summary>
    public bool SetAnimationIf(string requiredCurrent, string newAnimation, float transitionDuration = -1f)
    {
        if (currentAnimationName != requiredCurrent)
            return false;

        return SetAnimation(newAnimation, transitionDuration);
    }

    /// <summary>
    /// Cambia animación solo si la actual NO es la especificada.
    /// </summary>
    public bool SetAnimationIfNot(string excludedAnimation, string newAnimation, float transitionDuration = -1f)
    {
        if (currentAnimationName == excludedAnimation)
            return false;

        return SetAnimation(newAnimation, transitionDuration);
    }

    /// <summary>
    /// Resetea el tracking de animaciones (útil al reactivar enemigos del pool).
    /// </summary>
    public void ResetAnimationTracking()
    {
        currentAnimationHash = 0;
        previousAnimationHash = 0;
        currentAnimationName = string.Empty;
        previousAnimationName = string.Empty;
    }

    #endregion

    #region ══════════ UTILIDADES INTERNAS ══════════

    private void StorePreviousState()
    {
        previousAnimationHash = currentAnimationHash;
        previousAnimationName = currentAnimationName;
    }

    private bool ValidateAnimator()
    {
        if (animator == null)
        {
            LogWarning("Animator no asignado");
            return false;
        }
        return true;
    }

    private int GetAnimationHash(string animationName)
    {
        if (animationHashCache.TryGetValue(animationName, out int cachedHash))
            return cachedHash;

        int newHash = Animator.StringToHash(animationName);
        animationHashCache[animationName] = newHash;
        return newHash;
    }

    private void LogDebug(string message)
    {
        if (showDebugLogs)
            Debug.Log($"[EnemyAnimator] {message}", this);
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[EnemyAnimator] {message}", this);
    }

    #endregion
}