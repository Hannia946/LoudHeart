using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioAmbiente3D : MonoBehaviour
{
    public enum TipoAmbiente
    {
        Viento,
        Insectos,
        Agua,
        Crujidos,
        HojasRamas
    }

    [Header("TIPO DE AMBIENTE")]
    public TipoAmbiente tipoAmbiente;

    [Header("SONIDO")]
    public AudioClip sonidoAmbiente;

    [Header("CONFIGURACIÓN 3D")]
    [Range(0f, 1f)]
    public float volumen = 0.7f;

    [Min(0.1f)]
    public float distanciaMinima = 5f;

    [Min(1f)]
    public float distanciaMaxima = 30f;

    [Header("REPRODUCCIÓN")]
    public bool reproducirEnBucle = true;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        ConfigurarAudio3D();
    }

    private void Start()
    {
        if (sonidoAmbiente == null)
        {
            Debug.LogWarning(
                "[AUDIO BOSQUE] No hay AudioClip asignado en " +
                gameObject.name
            );

            return;
        }

        audioSource.clip = sonidoAmbiente;
        audioSource.loop = reproducirEnBucle;

        audioSource.Play();

        Debug.Log(
            "[AUDIO BOSQUE] " +
            tipoAmbiente +
            " iniciado en 3D."
        );
    }

    private void ConfigurarAudio3D()
    {
        audioSource.playOnAwake = false;

        // Sonido completamente tridimensional.
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

    public void ReproducirUnaVez()
    {
        if (sonidoAmbiente == null)
        {
            Debug.LogWarning(
                "[AUDIO BOSQUE] No hay sonido asignado."
            );

            return;
        }

        audioSource.PlayOneShot(
            sonidoAmbiente
        );
    }

    public void DetenerAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}