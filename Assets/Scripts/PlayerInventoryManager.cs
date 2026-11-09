using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventoryManager : NetworkBehaviour
{
    [Header("Inventario de Consumibles")]
    public int cantidadVendas = 0;
    public int cantidadPilas = 0;

    // Sin uso: se conserva solo para que otros scripts (ItemHighlight, etc.) sigan compilando
    [HideInInspector] public KeyItem objetoClaveEnMirada = null;

    // ===================== ENTRADA DESDE EL SERVIDOR =====================

    // Lo llama el consumible, solo en el servidor
    public void RecibirConsumible(ConsumableItem.ConsumableType tipo, int cantidad)
    {
        AgregarConsumibleClientRpc(tipo, cantidad, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
        });
    }

    [ClientRpc]
    private void AgregarConsumibleClientRpc(ConsumableItem.ConsumableType tipo, int cantidad, ClientRpcParams p = default)
    {
        AgregarConsumible(tipo, cantidad);   // se ejecuta solo en el dueño
    }

    public void AgregarConsumible(ConsumableItem.ConsumableType tipo, int cantidad)
    {
        switch (tipo)
        {
            case ConsumableItem.ConsumableType.Venda:
                cantidadVendas += cantidad;
                Debug.Log($"[Inventario] Vendas: {cantidadVendas}");
                break;

            case ConsumableItem.ConsumableType.Pila:
                cantidadPilas += cantidad;
                Debug.Log($"[Inventario] Pilas: {cantidadPilas}");
                break;

            case ConsumableItem.ConsumableType.HongoSpeed:
                StartCoroutine(ActivarSuperVelocidad(5f));
                var boost = GetComponent<SpeedBoostEffect>();
                if (boost != null) boost.ActivarEfectoVelocidad();
                break;
        }
    }

    // ===================== USO =====================

    public bool UsarVenda()
    {
        if (cantidadVendas <= 0) return false;
        cantidadVendas--;
        Debug.Log($"[SANA] Venda utilizada automáticamente. Restantes: {cantidadVendas}");
        return true;
    }

    public bool UsarPila()
    {
        if (cantidadPilas <= 0) return false;
        cantidadPilas--;
        Debug.Log($"[CARGA] Pila utilizada automáticamente. Restantes: {cantidadPilas}");
        return true;
    }

    private IEnumerator ActivarSuperVelocidad(float duracion)
    {
        Debug.Log("¡Supervelocidad activada por Hongo! (5 segundos)");
        yield return new WaitForSeconds(duracion);
        Debug.Log("Supervelocidad terminada.");
    }
}