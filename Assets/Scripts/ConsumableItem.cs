using Unity.Netcode;
using UnityEngine;

public class ConsumableItem : NetworkBehaviour
{
    public enum ConsumableType { Venda, Pila, Comida, HongoSpeed }

    [Header("Configuración del Consumible")]
    public ConsumableType tipoConsumible;
    public int cantidad = 1;
    [SerializeField] private AudioClip sonidoRecoleccion;

    private bool recogido = false;       // decide el servidor
    private float proximoIntento = 0f;   // pausa entre intentos del cliente

    // Lo detecta el jugador dueño en su pantalla y se lo pide al servidor.
    // OnTriggerStay (y no Enter) permite recoger un objeto sobre el que ya
    // estabas parado cuando empezó la partida.
    private void OnTriggerStay(Collider other)
    {
        if (!IsSpawned || Time.time < proximoIntento) return;

        var inventario = other.GetComponentInParent<PlayerInventoryManager>();
        if (inventario == null || !inventario.IsOwner) return;

        proximoIntento = Time.time + 0.5f;
        RecogerConsumibleServerRpc(inventario.NetworkObjectId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RecogerConsumibleServerRpc(ulong playerNetworkId)
    {
        if (recogido) return;   // otro jugador llegó antes

        if (GestorPartida.Instancia != null && !GestorPartida.Instancia.EnCurso) return;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out var jugador))
            return;

        var inventario = jugador.GetComponent<PlayerInventoryManager>();
        if (inventario == null) return;

        recogido = true;
        Debug.Log($"[CONSUMIBLE] {tipoConsumible} x{cantidad} recogido por el cliente {inventario.OwnerClientId}.");

        inventario.RecibirConsumible(tipoConsumible, cantidad);
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