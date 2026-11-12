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
        if (recogido) return;

        var inventario = other.GetComponentInParent<PlayerInventoryManager>();
        if (inventario == null) return;

        // Solo el jugador dueño de este personaje inicia la recolección
        if (inventario.IsOwner)
        {
            recogido = true;
            RecogerConsumibleServerRpc(inventario.NetworkObjectId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RecogerConsumibleServerRpc(ulong playerNetworkId)
    {
        // El servidor localiza al jugador y actualiza su inventario en la red
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out var playerObject))
        {
            var inventario = playerObject.GetComponent<PlayerInventoryManager>();
            if (inventario != null)
            {
                inventario.RecibirConsumible(tipoConsumible, cantidad);
                ReproducirSonidoClientRpc();
                NetworkObject.Despawn();
            }
        }
    }

    [ClientRpc]
    private void ReproducirSonidoClientRpc()
    {
        if (sonidoRecoleccion != null)
            AudioSource.PlayClipAtPoint(sonidoRecoleccion, transform.position);
    }
}