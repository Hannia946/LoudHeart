using System.Collections;
using UnityEngine;

public class SpeedBoostEffect : MonoBehaviour
{
    [Header("Referencias de Cámara y Post-Processing")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Arrastra aquí el objeto Volumen_HongoSpeed de la Hierarchy")]
    [SerializeField] private GameObject volumenHongoSpeed;

    [Header("Configuración de Velocidad (FOV)")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float boostFOV = 80f;
    [SerializeField] private float transitionSpeed = 5f;

    [Header("Duración")]
    [SerializeField] private float boostDuration = 5f; // Duración de 5 segundos

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

        // Asegurarnos de que el volumen inicie apagado
        if (volumenHongoSpeed != null)
        {
            volumenHongoSpeed.SetActive(false);
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

        // 1. Activar los efectos de la cámara (Motion Blur, Lens Distortion, Aberración Chromática)
        if (volumenHongoSpeed != null)
        {
            volumenHongoSpeed.SetActive(true);
        }

        // 2. Aumentar el FOV de la cámara suavemente para acentuar el impulso
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            mainCamera.fieldOfView = Mathf.Lerp(normalFOV, boostFOV, t);
            yield return null;
        }

        // 3. Mantener la velocidad activa durante la duración configurada
        yield return new WaitForSeconds(boostDuration);

        // 4. Regresar el FOV a la normalidad suavemente
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            mainCamera.fieldOfView = Mathf.Lerp(boostFOV, normalFOV, t);
            yield return null;
        }

        // 5. Apagar los efectos visuales
        if (volumenHongoSpeed != null)
        {
            volumenHongoSpeed.SetActive(false);
        }

        boostCoroutine = null;
    }
}