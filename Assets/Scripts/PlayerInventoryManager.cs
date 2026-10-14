using System.Collections;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    [Header("Inventario de Consumibles")]
    public int cantidadVendas = 0;
    public int cantidadPilas = 0;
    public int cantidadComida = 0;

    [Header("Objetos Clave Recolectados (Meta: 5)")]
    public int objetosClaveRecolectados = 0;

    [Header("Referencia al Objeto Clave Actual en Gaze Lock")]
    public KeyItem objetoClaveEnMirada = null;

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

            case ConsumableItem.ConsumableType.Comida:
                cantidadComida += cantidad;
                Debug.Log($"[Inventario] Comida: {cantidadComida}");
                break;

            case ConsumableItem.ConsumableType.HongoSpeed:
                StartCoroutine(ActivarSuperVelocidad(5f));
                break;
        }

    }
    public bool UsarVenda()
    {
        if (cantidadVendas <= 0)
        {
            Debug.LogWarning(
                "[SANA] No hay vendas disponibles."
            );

            return false;
        }

        cantidadVendas--;

        Debug.Log(
            "[SANA] Venda utilizada. Vendas restantes: " +
            cantidadVendas
        );

        return true;
    }
    public bool UsarComida()
    {
        if (cantidadComida <= 0)
        {
            Debug.LogWarning(
                "[COME] No hay comida disponible."
            );

            return false;
        }

        cantidadComida--;

        Debug.Log(
            "[COME] Comida utilizada. Comida restante: " +
            cantidadComida
        );

        return true;
    }
    // ============================================================
    // USAR PILA - CARGA
    // ============================================================

    public bool UsarPila()
    {
        if (cantidadPilas <= 0)
        {
            Debug.LogWarning(
                "[CARGA] No hay pilas disponibles."
            );

            return false;
        }

        cantidadPilas--;

        Debug.Log(
            "[CARGA] Pila utilizada. Pilas restantes: " +
            cantidadPilas
        );

        return true;
    }

    private IEnumerator ActivarSuperVelocidad(float duracion)
    {
        Debug.Log("¡Supervelocidad activada por Hongo! (5 segundos)");
        yield return new WaitForSeconds(duracion);
        Debug.Log("Supervelocidad terminada.");
    }

    public bool EjecutarComandoToma()
    {
        if (
            objetoClaveEnMirada != null &&
            objetoClaveEnMirada.EstaFijado()
        )
        {
            objetosClaveRecolectados++;

            Debug.Log(
                "[TOMA] Objeto clave '" +
                objetoClaveEnMirada.nombreObjetoClave +
                "' recolectado. Total: " +
                objetosClaveRecolectados +
                "/5"
            );

            objetoClaveEnMirada.RecogerObjeto();

            objetoClaveEnMirada = null;

            return true;
        }

        Debug.LogWarning(
            "[TOMA] Rechazado: no hay ningún objeto clave fijado con Gaze Lock."
        );

        return false;
    }
}