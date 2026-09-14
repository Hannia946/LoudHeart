using System.Collections;
using UnityEngine;

public class SpeedBoostEffect : MonoBehaviour
{
    [Header("Referencias de Cámara")]
    [SerializeField] private Camera mainCamera;

    [Header("Efecto Visual de Pantalla (Overlay)")]
    [SerializeField] private CanvasGroup screenOverlayCanvasGroup;
    [SerializeField][Range(0.1f, 1f)] private float maxOverlayAlpha = 0.35f; // Controla la transparencia máxima

    [Header("Configuración de Velocidad (FOV)")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float boostFOV = 80f;
    [SerializeField] private float transitionSpeed = 5f;

    [Header("Duración")]
    [SerializeField] private float boostDuration = 3.5f;

    private Coroutine boostCoroutine;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            normalFOV = mainCamera.fieldOfView;
        }

        // Ocultar la imagen al iniciar
        if (screenOverlayCanvasGroup != null)
        {
            screenOverlayCanvasGroup.alpha = 0f;
        }
    }

    [ContextMenu("Probar Efecto Hongo")]
    public void ActivarEfectoVelocidad()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Debes estar en Play Mode para probar la animación.");
            return;
        }

        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
        }

        boostCoroutine = StartCoroutine(EfectoHongoRoutine());
    }

    private IEnumerator EfectoHongoRoutine()
    {
        if (mainCamera == null) yield break;

        // 1. Aumentar FOV y mostrar la imagen progresivamente
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            mainCamera.fieldOfView = Mathf.Lerp(normalFOV, boostFOV, t);

            if (screenOverlayCanvasGroup != null)
            {
                screenOverlayCanvasGroup.alpha = Mathf.Lerp(0f, maxOverlayAlpha, t);
            }
            yield return null;
        }

        // 2. Mantener la velocidad y el tinte morado activo
        yield return new WaitForSeconds(boostDuration);

        // 3. Volver FOV a la normalidad y desvanecer la imagen
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            mainCamera.fieldOfView = Mathf.Lerp(boostFOV, normalFOV, t);

            if (screenOverlayCanvasGroup != null)
            {
                screenOverlayCanvasGroup.alpha = Mathf.Lerp(maxOverlayAlpha, 0f, t);
            }
            yield return null;
        }
    }
}