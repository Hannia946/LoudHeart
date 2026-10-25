using UnityEngine;
using System.Collections;

public class AudioAmbiente3D : MonoBehaviour
{
    [Header("AUDIOS DEL BOSQUE")]
    public AudioClip sonidoViento;
    public AudioClip sonidoBuho;
    public AudioClip sonidoRafaga;
    public AudioClip sonidoAgua;
    public AudioClip sonidoCrujido;

    [Header("VOLUMEN")]
    [Range(0f, 1f)]
    public float volumenViento = 0.45f;

    [Range(0f, 1f)]
    public float volumenBuho = 0.35f;

    [Range(0f, 1f)]
    public float volumenRafaga = 0.55f;

    [Range(0f, 1f)]
    public float volumenAgua = 0.60f;

    [Range(0f, 1f)]
    public float volumenCrujido = 0.80f;

    [Header("DISTANCIA AMBIENTE")]
    [Min(0.1f)]
    public float distanciaMinima = 5f;

    [Min(1f)]
    public float distanciaMaxima = 30f;

    [Header("DISTANCIA DEL AGUA")]
    [Min(0.1f)]
    public float distanciaMinimaAgua = 3f;

    [Min(1f)]
    public float distanciaMaximaAgua = 18f;

    [Header("BÚHO OCASIONAL")]
    public bool activarBuho = true;

    [Min(1f)]
    public float tiempoMinimoBuho = 20f;

    [Min(1f)]
    public float tiempoMaximoBuho = 40f;

    [Header("RÁFAGA OCASIONAL")]
    public bool activarRafaga = true;

    [Min(1f)]
    public float tiempoMinimoRafaga = 12f;

    [Min(1f)]
    public float tiempoMaximoRafaga = 25f;

    [Header("CRUJIDOS OCASIONALES")]
    public bool activarCrujidos = true;

    [Min(1f)]
    public float tiempoMinimoCrujido = 8f;

    [Min(1f)]
    public float tiempoMaximoCrujido = 20f;

    private AudioSource fuenteViento;
    private AudioSource fuenteBuho;
    private AudioSource fuenteRafaga;
    private AudioSource fuenteAgua;
    private AudioSource fuenteCrujido;

    private void Awake()
    {
        // VIENTO - CONTINUO
        if (sonidoViento != null)
        {
            fuenteViento = CrearAudioSource(
                sonidoViento,
                volumenViento,
                true,
                AudioRolloffMode.Logarithmic,
                distanciaMinima,
                distanciaMaxima
            );
        }

        // BÚHO - OCASIONAL
        if (sonidoBuho != null)
        {
            fuenteBuho = CrearAudioSource(
                sonidoBuho,
                volumenBuho,
                false,
                AudioRolloffMode.Logarithmic,
                distanciaMinima,
                distanciaMaxima
            );
        }

        // RÁFAGA - OCASIONAL
        if (sonidoRafaga != null)
        {
            fuenteRafaga = CrearAudioSource(
                sonidoRafaga,
                volumenRafaga,
                false,
                AudioRolloffMode.Logarithmic,
                distanciaMinima,
                distanciaMaxima
            );
        }

        // AGUA - CONTINUA Y LOCALIZADA
        if (sonidoAgua != null)
        {
            fuenteAgua = CrearAudioSource(
                sonidoAgua,
                volumenAgua,
                true,
                AudioRolloffMode.Linear,
                distanciaMinimaAgua,
                distanciaMaximaAgua
            );
        }

        // CRUJIDO - OCASIONAL
        if (sonidoCrujido != null)
        {
            fuenteCrujido = CrearAudioSource(
                sonidoCrujido,
                volumenCrujido,
                false,
                AudioRolloffMode.Logarithmic,
                distanciaMinima,
                distanciaMaxima
            );
        }
    }

    private void Start()
    {
        // VIENTO
        if (fuenteViento != null)
        {
            fuenteViento.Play();

            Debug.Log(
                "[AUDIO BOSQUE] Viento iniciado."
            );
        }

        // AGUA
        if (fuenteAgua != null)
        {
            fuenteAgua.Play();

            Debug.Log(
                "[AUDIO BOSQUE] Agua iniciada."
            );
        }

        // BÚHO
        if (activarBuho &&
            fuenteBuho != null)
        {
            StartCoroutine(
                ReproducirBuhoAleatorio()
            );
        }

        // RÁFAGA
        if (activarRafaga &&
            fuenteRafaga != null)
        {
            StartCoroutine(
                ReproducirRafagaAleatoria()
            );
        }

        // CRUJIDOS
        if (activarCrujidos &&
            fuenteCrujido != null)
        {
            StartCoroutine(
                ReproducirCrujidosAleatorios()
            );
        }
    }

    // ============================================================
    // CREAR AUDIO SOURCE 3D
    // ============================================================

    private AudioSource CrearAudioSource(
        AudioClip clip,
        float volumen,
        bool loop,
        AudioRolloffMode rolloff,
        float distanciaMin,
        float distanciaMax)
    {
        AudioSource fuente =
            gameObject.AddComponent<AudioSource>();

        fuente.clip = clip;
        fuente.volume = volumen;

        fuente.playOnAwake = false;
        fuente.loop = loop;

        fuente.spatialBlend = 1f;

        fuente.rolloffMode = rolloff;

        fuente.minDistance = distanciaMin;
        fuente.maxDistance = distanciaMax;

        fuente.dopplerLevel = 0f;

        return fuente;
    }

    // ============================================================
    // BÚHO
    // ============================================================

    private IEnumerator ReproducirBuhoAleatorio()
    {
        while (true)
        {
            float espera = Random.Range(
                tiempoMinimoBuho,
                tiempoMaximoBuho
            );

            yield return new WaitForSeconds(
                espera
            );

            if (fuenteBuho != null &&
                sonidoBuho != null)
            {
                fuenteBuho.PlayOneShot(
                    sonidoBuho
                );

                Debug.Log(
                    "[AUDIO BOSQUE] Búho reproducido."
                );
            }
        }
    }

    // ============================================================
    // RÁFAGA
    // ============================================================

    private IEnumerator ReproducirRafagaAleatoria()
    {
        while (true)
        {
            float espera = Random.Range(
                tiempoMinimoRafaga,
                tiempoMaximoRafaga
            );

            yield return new WaitForSeconds(
                espera
            );

            if (fuenteRafaga != null &&
                sonidoRafaga != null)
            {
                fuenteRafaga.PlayOneShot(
                    sonidoRafaga
                );

                Debug.Log(
                    "[AUDIO BOSQUE] Ráfaga reproducida."
                );
            }
        }
    }

    // ============================================================
    // CRUJIDOS
    // ============================================================

    private IEnumerator ReproducirCrujidosAleatorios()
    {
        while (true)
        {
            float espera = Random.Range(
                tiempoMinimoCrujido,
                tiempoMaximoCrujido
            );

            yield return new WaitForSeconds(
                espera
            );

            if (fuenteCrujido != null &&
                sonidoCrujido != null)
            {
                fuenteCrujido.PlayOneShot(
                    sonidoCrujido
                );

                Debug.Log(
                    "[AUDIO BOSQUE] Crujido reproducido."
                );
            }
        }
    }

    // ============================================================
    // DETENER TODO
    // ============================================================

    public void DetenerAudio()
    {
        StopAllCoroutines();

        if (fuenteViento != null)
            fuenteViento.Stop();

        if (fuenteBuho != null)
            fuenteBuho.Stop();

        if (fuenteRafaga != null)
            fuenteRafaga.Stop();

        if (fuenteAgua != null)
            fuenteAgua.Stop();

        if (fuenteCrujido != null)
            fuenteCrujido.Stop();
    }
}