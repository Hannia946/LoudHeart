using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Unity.Netcode.Components;
using UnityEngine.SceneManagement;


public class ConfiguracionLocal : NetworkBehaviour
{
    [Header("Solo se activan en el jugador local (dueño)")]
    [SerializeField] private Camera camaraOjos;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private OpenTrackReceptor receptor;
    [SerializeField] private PlayerHeadController headController;
    [SerializeField] private string escenaJuego = "Bosque_Greybox";

    public override void OnNetworkSpawn()
    {
        bool local = IsOwner;

        camaraOjos.enabled = local;
        if (audioListener != null) audioListener.enabled = local;
        receptor.enabled = local;
        headController.enabled = local;

        if (local)
        {
            // Apaga cualquier otro AudioListener (por ejemplo el de la cámara del menú)
            // foreach (var l in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
            //     if (l != audioListener) l.enabled = false;

            foreach (var l in FindObjectsByType<AudioListener>())
                if (l != audioListener) l.enabled = false;

            StartCoroutine(CalibrarAlInicio());
            StartCoroutine(ColocarEnSpawn());
        }
    }

    private IEnumerator CalibrarAlInicio()
    {
        // Da tiempo a que lleguen datos de OpenTrack y el jugador mire al frente
        yield return new WaitForSeconds(2f);
        headController.CalibrarCentro();
    }

    private IEnumerator ColocarEnSpawn()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.useGravity = false;   // no caer mientras carga la escena

        // Espera a estar en la escena de juego
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == escenaJuego);
        yield return null;   // un frame para que existan los colliders

        var spawns = FindAnyObjectByType<PuntosSpawnJugadores>();
        var punto = spawns != null ? spawns.ObtenerPunto(OwnerClientId) : null;

        if (punto != null)
        {
            if (rb != null) rb.linearVelocity = Vector3.zero;
            GetComponent<NetworkTransform>().Teleport(punto.position, punto.rotation, transform.localScale);
        }
        else
        {
            Debug.LogWarning("[SPAWN] No se encontró PuntosSpawnJugadores en la escena.");
        }

        if (rb != null) rb.useGravity = true;
    }
}
