using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class KeyItem : NetworkBehaviour
{
    [Header("Información del Objeto Clave")]
    public string nombreObjetoClave;
    public int idObjeto;
    [SerializeField] private AudioClip sonidoRecoleccionUnico;

    [Header("Visual Feedback (Gaze Lock)")]
    [SerializeField] private Outline outlineComponent;

    private bool estaFijado = false;
    private bool recogido = false;

    private void Start()
    {
        if (outlineComponent == null)
            outlineComponent = GetComponent<Outline>();

        DesresaltarObjeto();
    }

    public void ResaltarObjeto()
    {
        estaFijado = true;
        if (outlineComponent != null)
            outlineComponent.enabled = true;

        Debug.Log($"[Gaze Lock] Objeto {nombreObjetoClave} resaltado y listo para comando toma");
    }

    public void DesresaltarObjeto()
    {
        estaFijado = false;
        if (outlineComponent != null)
            outlineComponent.enabled = false;
    }

    public bool EstaFijado()
    {
        return estaFijado;
    }

    // Recolección automática: solo decide el servidor
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || recogido) return;
        if (other.GetComponentInParent<PlayerInventoryManager>() == null) return;

        recogido = true;

        if (GestorPartida.Instancia != null)
            GestorPartida.Instancia.RegistrarObjetoClave();

        Debug.Log($"[OBJETO CLAVE] '{nombreObjetoClave}' recolectado.");

        ReproducirSonidoClientRpc();
        NetworkObject.Despawn();   // desaparece para todos
    }

    [ClientRpc]
    private void ReproducirSonidoClientRpc()
    {
        if (sonidoRecoleccionUnico != null)
            AudioSource.PlayClipAtPoint(sonidoRecoleccionUnico, transform.position);
    }
}