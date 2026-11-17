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


    // ============================================================
    // AGUA LOCALIZADA
    // ============================================================

    [Header("DISTANCIA DEL AGUA")]
    [Min(0.1f)]
    public float distanciaMinimaAgua = 25f;

    [Min(1f)]
    public float distanciaMaximaAgua = 60f;


    // ============================================================
    // SONIDOS OCASIONALES
    // ============================================================

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


    // ============================================================
    // AUDIO SOURCES
    // ============================================================

    private AudioSource fuenteViento;
    private AudioSource fuenteBuho;
    private AudioSource fuenteRafaga;
    private AudioSource fuenteAgua;
    private AudioSource fuenteCrujido;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        // ========================================================
        // AMBIENTE GENERAL DEL BOSQUE
        // ========================================================
        //
        // Estos sonidos son 2D para que el bosque nunca quede
        // completamente en silencio, incluso en los extremos
        // del mapa.
        // ========================================================


        // VIENTO - CONTINUO
        if (sonidoViento != null)
        {
            fuenteViento = CrearAudioSourceGlobal(
                sonidoViento,
                volumenViento,
                true
            );
        }


        // BÚHO - OCASIONAL
        if (sonidoBuho != null)
        {
            fuenteBuho = CrearAudioSourceGlobal(
                sonidoBuho,
                volumenBuho,
                false
            );
        }


        // RÁFAGA - OCASIONAL
        if (sonidoRafaga != null)
        {
            fuenteRafaga = CrearAudioSourceGlobal(
                sonidoRafaga,
                volumenRafaga,
                false
            );
        }


        // CRUJIDO - OCASIONAL
        if (sonidoCrujido != null)
        {
            fuenteCrujido = CrearAudioSourceGlobal(
                sonidoCrujido,
                volumenCrujido,
                false
            );
        }


        // ========================================================
        // AGUA - CONTINUA Y LOCALIZADA
        // ========================================================

        if (sonidoAgua != null)
        {
            fuenteAgua = CrearAudioSource3D(
                sonidoAgua,
                volumenAgua,
                true,
                AudioRolloffMode.Linear,
                distanciaMinimaAgua,
                distanciaMaximaAgua
            );
        }
    }


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        // VIENTO
        if (fuenteViento != null)
        {
            fuenteViento.Play();

            Debug.Log(
                "[AUDIO BOSQUE] Ambiente global iniciado."
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
        if (
            activarBuho &&
            fuenteBuho != null
        )
        {
            StartCoroutine(
                ReproducirBuhoAleatorio()
            );
        }


        // RÁFAGA
        if (
            activarRafaga &&
            fuenteRafaga != null
        )
        {
            StartCoroutine(
                ReproducirRafagaAleatoria()
            );
        }


        // CRUJIDOS
        if (
            activarCrujidos &&
            fuenteCrujido != null
        )
        {
            StartCoroutine(
                ReproducirCrujidosAleatorios()
            );
        }
    }


    // ============================================================
    // CREAR AUDIO SOURCE GLOBAL / 2D
    // ============================================================

    private AudioSource CrearAudioSourceGlobal(
        AudioClip clip,
        float volumen,
        bool loop
    )
    {
        AudioSource fuente =
            gameObject.AddComponent<AudioSource>();


        fuente.clip =
            clip;


        fuente.volume =
            volumen;


        fuente.playOnAwake =
            false;


        fuente.loop =
            loop;


        // ========================================================
        // 0 = completamente 2D
        // Se escucha igual en cualquier punto del mapa.
        // ========================================================

        fuente.spatialBlend =
            0f;


        fuente.dopplerLevel =
            0f;


        return fuente;
    }


    // ============================================================
    // CREAR AUDIO SOURCE 3D
    // ============================================================

    private AudioSource CrearAudioSource3D(
        AudioClip clip,
        float volumen,
        bool loop,
        AudioRolloffMode rolloff,
        float distanciaMin,
        float distanciaMax
    )
    {
        AudioSource fuente =
            gameObject.AddComponent<AudioSource>();


        fuente.clip =
            clip;


        fuente.volume =
            volumen;


        fuente.playOnAwake =
            false;


        fuente.loop =
            loop;


        fuente.spatialBlend =
            1f;


        fuente.rolloffMode =
            rolloff;


        fuente.minDistance =
            distanciaMin;


        fuente.maxDistance =
            distanciaMax;


        fuente.dopplerLevel =
            0f;


        fuente.spread =
            0f;


        return fuente;
    }


    // ============================================================
    // BÚHO
    // ============================================================

    private IEnumerator ReproducirBuhoAleatorio()
    {
        while (true)
        {
            float espera =
                Random.Range(
                    tiempoMinimoBuho,
                    tiempoMaximoBuho
                );


            yield return new WaitForSeconds(
                espera
            );


            if (
                fuenteBuho != null &&
                sonidoBuho != null
            )
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
            float espera =
                Random.Range(
                    tiempoMinimoRafaga,
                    tiempoMaximoRafaga
                );


            yield return new WaitForSeconds(
                espera
            );


            if (
                fuenteRafaga != null &&
                sonidoRafaga != null
            )
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
            float espera =
                Random.Range(
                    tiempoMinimoCrujido,
                    tiempoMaximoCrujido
                );


            yield return new WaitForSeconds(
                espera
            );


            if (
                fuenteCrujido != null &&
                sonidoCrujido != null
            )
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