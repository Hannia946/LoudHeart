using Unity.Netcode;
using UnityEngine;
using System.Collections;


public class ConfiguracionLocal : NetworkBehaviour
{
    [Header("Solo se activan en el jugador local (dueño)")]
    [SerializeField] private Camera camaraOjos;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private OpenTrackReceptor receptor;
    [SerializeField] private PlayerHeadController headController;

    public override void OnNetworkSpawn()
    {
        bool local = IsOwner;

        camaraOjos.enabled = local;
        if (audioListener != null) audioListener.enabled = local;
        receptor.enabled = local;
        headController.enabled = local;
        if (local) StartCoroutine(CalibrarAlInicio());
    }

    private IEnumerator CalibrarAlInicio()
    {
        // Da tiempo a que lleguen datos de OpenTrack y el jugador mire al frente
        yield return new WaitForSeconds(2f);
        headController.CalibrarCentro();
    }
}
