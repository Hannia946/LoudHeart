using UnityEngine;

public class PruebaTomaObjetoClave : MonoBehaviour
{
    [Header("PRUEBA TEMPORAL DE TOMA")]
    public PlayerInventoryManager inventarioExplorador;
    public KeyItem objetoClavePrueba;

    [Header("CONFIGURACIÓN")]
    public bool fijarObjetoAlIniciar = true;

    private void Start()
    {
        if (fijarObjetoAlIniciar)
        {
            Invoke(nameof(FijarObjetoPrueba), 0.2f);
        }
    }

    public void FijarObjetoPrueba()
    {
        if (inventarioExplorador == null)
        {
            Debug.LogWarning("[PRUEBA TOMA] Falta PlayerInventoryManager.");
            return;
        }

        if (objetoClavePrueba == null)
        {
            Debug.LogWarning("[PRUEBA TOMA] Falta asignar un KeyItem.");
            return;
        }

        objetoClavePrueba.ResaltarObjeto();

        inventarioExplorador.objetoClaveEnMirada =
            objetoClavePrueba;

        Debug.Log(
            "[PRUEBA TOMA] Objeto fijado manualmente: " +
            objetoClavePrueba.nombreObjetoClave
        );
    }
}