using Unity.Netcode;
using UnityEngine;

public class ConsumableItem : NetworkBehaviour
{
    public enum ConsumableType { Venda, Pila, Comida, HongoSpeed }

    [Header("Configuración del Consumible")]
    public ConsumableType tipoConsumible;
    public int cantidad = 1;
    [SerializeField] private AudioClip sonidoRecoleccion;

    private bool recogido = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || recogido) return;   // solo decide el servidor

        var inventario = other.GetComponentInParent<PlayerInventoryManager>();
        if (inventario == null) return;

        recogido = true;

        inventario.RecibirConsumible(tipoConsumible, cantidad);   // avisa al dueño
        ReproducirSonidoClientRpc();
        NetworkObject.Despawn();

    }

    [ClientRpc]
    private void ReproducirSonidoClientRpc()
    {
        if (sonidoRecoleccion != null)
            AudioSource.PlayClipAtPoint(sonidoRecoleccion, transform.position);
    }
}