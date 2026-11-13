using TMPro;
using Unity.Netcode;
using UnityEngine;

public class HudOperador : MonoBehaviour
{
    [SerializeField] private TMP_Text textoVida;
    [SerializeField] private TMP_Text textoBateria;
    [SerializeField] private TMP_Text textoVendas;
    [SerializeField] private TMP_Text textoPilas;
    [SerializeField] private TMP_Text textoObjetosClave;
    [SerializeField] private DamageOverlay overlay;

    [SerializeField] private Color colorNormal = new Color(0.2f, 1f, 0.4f);
    [SerializeField] private Color colorBajo = new Color(1f, 0.3f, 0.3f);

    private EstadoVitalExplorador vital;
    private PlayerInventoryManager inventario;
    private int uVida = -1, uBateria = -1, uVendas = -1, uPilas = -1, uObjetos = -1;

    private void Update()
    {
        if (vital == null || inventario == null)
        {
            BuscarJugadorLocal();
            return;
        }

        if (overlay != null)
            overlay.ActualizarEfectoVida(vital.vidaActual, vital.vidaMaxima);

        int vida = Mathf.RoundToInt(vital.vidaActual / vital.vidaMaxima * 100f);
        if (vida != uVida && textoVida != null)
        {
            uVida = vida;
            textoVida.text = vida + "%";
            textoVida.color = vital.vidaActual <= vital.umbralVida ? colorBajo : colorNormal;
        }

        int bateria = Mathf.RoundToInt(vital.bateriaActual / vital.bateriaMaxima * 100f);
        if (bateria != uBateria && textoBateria != null)
        {
            uBateria = bateria;
            textoBateria.text = bateria + "%";
            textoBateria.color = vital.bateriaActual <= vital.umbralBateria ? colorBajo : colorNormal;
        }

        if (inventario.cantidadVendas != uVendas && textoVendas != null)
        {
            uVendas = inventario.cantidadVendas;
            textoVendas.text = "x" + uVendas;
        }

        if (inventario.cantidadPilas != uPilas && textoPilas != null)
        {
            uPilas = inventario.cantidadPilas;
            textoPilas.text = "x" + uPilas;
        }

        if (GestorPartida.Instancia != null && textoObjetosClave != null)
        {
            int n = GestorPartida.Instancia.objetosClaveRecolectados.Value;
            if (n != uObjetos)
            {
                uObjetos = n;
                textoObjetosClave.text = n + "/" + GestorPartida.MetaObjetosClave;
            }
        }
    }

    private void BuscarJugadorLocal()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null || nm.LocalClient == null || nm.LocalClient.PlayerObject == null) return;

        NetworkObject jugador = nm.LocalClient.PlayerObject;
        vital = jugador.GetComponent<EstadoVitalExplorador>();
        inventario = jugador.GetComponent<PlayerInventoryManager>();
    }
}