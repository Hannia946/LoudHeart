using Unity.Netcode;
using UnityEngine;

public class SonidoProximidadObjeto : MonoBehaviour
{
    [Header("Sonido de proximidad")]
    [SerializeField] private AudioClip sonidoCercania;

    [Header("Distancia")]
    [SerializeField] private float radioDeteccion = 15f;
    [SerializeField] private float distanciaMinima = 1.5f;

    [Header("Volumen")]
    [Range(0f, 1f)]
    [SerializeField] private float volumenLejos = 0.20f;

    [Range(0f, 1f)]
    [SerializeField] private float volumenCerca = 0.75f;

    [Header("Frecuencia del aviso")]
    [SerializeField] private float intervaloLejos = 1.8f;
    [SerializeField] private float intervaloCerca = 0.35f;

    private AudioSource fuente;
    private Transform jugadorLocal;

    private float siguienteSonido;
    private float siguienteBusquedaJugador;


    private void Awake()
    {
        fuente = gameObject.AddComponent<AudioSource>();

        fuente.playOnAwake = false;
        fuente.loop = false;

        // Sonido completamente 3D.
        fuente.spatialBlend = 1f;

        fuente.rolloffMode = AudioRolloffMode.Linear;

        fuente.minDistance = distanciaMinima;
        fuente.maxDistance = radioDeteccion;

        fuente.dopplerLevel = 0f;
        fuente.spread = 0f;
    }


    private void Update()
    {
        if (sonidoCercania == null)
            return;


        // Buscar periódicamente al jugador local.
        if (
            jugadorLocal == null &&
            Time.time >= siguienteBusquedaJugador
        )
        {
            BuscarJugadorLocal();

            siguienteBusquedaJugador =
                Time.time + 0.5f;
        }


        if (jugadorLocal == null)
            return;


        Vector3 posicionObjeto =
            transform.position;

        Vector3 posicionJugador =
            jugadorLocal.position;


        posicionObjeto.y = 0f;
        posicionJugador.y = 0f;


        float distancia =
            Vector3.Distance(
                posicionObjeto,
                posicionJugador
            );


        // Fuera del rango no suena.
        if (distancia > radioDeteccion)
            return;


        /*
         * 0 = jugador en el límite exterior.
         * 1 = jugador prácticamente encima del objeto.
         */
        float cercania =
            1f -
            Mathf.InverseLerp(
                distanciaMinima,
                radioDeteccion,
                distancia
            );


        cercania =
            Mathf.Clamp01(cercania);


        // Conforme se acerca:
        // más volumen.
        float volumenActual =
            Mathf.Lerp(
                volumenLejos,
                volumenCerca,
                cercania
            );


        // Conforme se acerca:
        // menos tiempo entre avisos.
        float intervaloActual =
            Mathf.Lerp(
                intervaloLejos,
                intervaloCerca,
                cercania
            );


        if (Time.time < siguienteSonido)
            return;


        fuente.PlayOneShot(
            sonidoCercania,
            volumenActual
        );


        siguienteSonido =
            Time.time +
            intervaloActual;
    }


    private void BuscarJugadorLocal()
    {
        // ========================================================
        // MULTIJUGADOR
        // ========================================================

        if (
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening &&
            NetworkManager.Singleton.LocalClient != null &&
            NetworkManager.Singleton.LocalClient.PlayerObject != null
        )
        {
            jugadorLocal =
                NetworkManager.Singleton
                    .LocalClient
                    .PlayerObject
                    .transform;

            return;
        }


        // ========================================================
        // PRUEBA LOCAL
        // ========================================================

        PlayerInventoryManager jugador =
            FindAnyObjectByType<PlayerInventoryManager>();


        if (jugador != null)
        {
            jugadorLocal =
                jugador.transform;
        }
    }
}