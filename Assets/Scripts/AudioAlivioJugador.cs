using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioAlivioJugador : NetworkBehaviour
{
    [Header("Audio de alivio")]
    [SerializeField] private AudioClip sonidoAlivio;

    [Range(0f, 1f)]
    [SerializeField] private float volumen = 0.8f;

    private AudioSource fuente;


    private void Awake()
    {
        fuente = GetComponent<AudioSource>();

        fuente.playOnAwake = false;
        fuente.loop = false;

        // El alivio lo escucha directamente el jugador.
        // No debe venir desde una posición del mapa.
        fuente.spatialBlend = 0f;

        fuente.volume = volumen;
    }


    public void ReproducirAlivio()
    {
        // Solo el jugador dueño de este PlayerObject
        // debe escuchar su propio alivio.
        if (!IsOwner)
            return;

        if (sonidoAlivio == null)
        {
            Debug.LogWarning(
                "[AUDIO JUGADOR] No hay sonido de alivio asignado."
            );

            return;
        }

        fuente.PlayOneShot(
            sonidoAlivio,
            volumen
        );

        Debug.Log(
            "[AUDIO JUGADOR] Alivio reproducido."
        );
    }


    public static void ReproducirParaCliente(
        ulong clientId
    )
    {
        if (NetworkManager.Singleton == null)
            return;

        // Este mensaje RPC llegará a todos,
        // pero solo continúa el cliente correcto.
        if (
            NetworkManager.Singleton.LocalClientId !=
            clientId
        )
        {
            return;
        }


        AudioAlivioJugador[] jugadores =
            FindObjectsByType<AudioAlivioJugador>(
                FindObjectsSortMode.None
            );


        foreach (
            AudioAlivioJugador audioJugador
            in jugadores
        )
        {
            if (
                audioJugador != null &&
                audioJugador.IsOwner
            )
            {
                audioJugador.ReproducirAlivio();

                return;
            }
        }


        Debug.LogWarning(
            "[AUDIO JUGADOR] No se encontró AudioAlivioJugador local."
        );
    }
}
