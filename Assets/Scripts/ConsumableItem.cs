using UnityEngine;

public class ConsumableItem : MonoBehaviour
{
    public enum ConsumableType { Venda, Pila, Comida, HongoSpeed }

    [Header("Configuración del Consumible")]
    public ConsumableType tipoConsumible;
    public int cantidad = 1;
    [SerializeField] private AudioClip sonidoRecoleccion;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventoryManager inventario = other.GetComponent<PlayerInventoryManager>();
            if (inventario != null)
            {
                inventario.AgregarConsumible(tipoConsumible, cantidad);

                if (sonidoRecoleccion != null)
                {
                    AudioSource.PlayClipAtPoint(sonidoRecoleccion, transform.position);
                }

                Destroy(gameObject);
            }
        }
    }
}
