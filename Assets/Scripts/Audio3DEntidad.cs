using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Audio3DEntidad : MonoBehaviour
{
    [Header("SONIDOS DE LA ENTIDAD")]
    public AudioClip respiracionGruñido;
    public AudioClip sonidoPasos;
    public AudioClip sonidoDeteccion;
    public AudioClip sonidoAtaque;

    [Header("CONFIGURACIÓN 3D")]
    [Range(0f, 1f)]
    public float volumen = 1f;

    [Min(0.1f)]
    public float distanciaMinima = 3f;

    [Min(1f)]
    public float distanciaMaxima = 25f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        ConfigurarAudio3D();
    }

    private void Start()
    {
        IniciarRespiracion();
    }

    private void ConfigurarAudio3D()
    {
        audioSource.playOnAwake = false;

        // 0 = sonido 2D
        // 1 = sonido completamente 3D
        audioSource.spatialBlend = 1f;

        audioSource.volume = volumen;

        audioSource.rolloffMode =
            AudioRolloffMode.Logarithmic;

        audioSource.minDistance =
            distanciaMinima;

        audioSource.maxDistance =
            distanciaMaxima;

        audioSource.dopplerLevel = 0f;
    }

    // ============================================================
    // RESPIRACIÓN / GRUÑIDO
    // ============================================================

    public void IniciarRespiracion()
    {
        if (respiracionGruñido == null)
        {
            Debug.LogWarning(
                "[AUDIO ENTIDAD] No hay sonido de respiración asignado."
            );

            return;
        }

        audioSource.clip = respiracionGruñido;
        audioSource.loop = true;

        audioSource.Play();

        Debug.Log(
            "[AUDIO ENTIDAD] Respiración 3D iniciada."
        );
    }

    // ============================================================
    // PASOS
    // ============================================================

    public void ReproducirPaso()
    {
        ReproducirSonido(
            sonidoPasos,
            "PASO"
        );
    }

    // ============================================================
    // DETECCIÓN
    // ============================================================

    public void ReproducirDeteccion()
    {
        ReproducirSonido(
            sonidoDeteccion,
            "DETECCIÓN"
        );
    }

    // ============================================================
    // ATAQUE
    // ============================================================

    public void ReproducirAtaque()
    {
        ReproducirSonido(
            sonidoAtaque,
            "ATAQUE"
        );
    }

    // ============================================================
    // SONIDO PUNTUAL
    // ============================================================

    private void ReproducirSonido(
        AudioClip clip,
        string nombreSonido
    )
    {
        if (clip == null)
        {
            Debug.LogWarning(
                "[AUDIO ENTIDAD] No hay clip asignado para " +
                nombreSonido + "."
            );

            return;
        }

        audioSource.PlayOneShot(clip);

        Debug.Log(
            "[AUDIO ENTIDAD] " +
            nombreSonido +
            " reproducido en 3D."
        );
    }
}