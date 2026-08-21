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

    private IEnumerator ActivarSuperVelocidad(float duracion)
    {
        Debug.Log("¡Supervelocidad activada por Hongo! (5 segundos)");
        yield return new WaitForSeconds(duracion);
        Debug.Log("Supervelocidad terminada.");
    }

    public void EjecutarComandoToma()
    {
        if (objetoClaveEnMirada != null && objetoClaveEnMirada.EstaFijado())
        {
            objetosClaveRecolectados++;
            Debug.Log($"[ÉXITO] ¡Objeto clave '{objetoClaveEnMirada.nombreObjetoClave}' recolectado! Total: {objetosClaveRecolectados}/5");

            objetoClaveEnMirada.RecogerObjeto();
            objetoClaveEnMirada = null;
        }
        else
        {
            Debug.LogWarning("[RECHAZADO] Se dijo 'TOMA', pero no hay ningún objeto clave en Gaze Lock.");
        }
    }
}