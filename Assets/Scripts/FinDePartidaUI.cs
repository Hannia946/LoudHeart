using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinDePartidaUI : MonoBehaviour
{
    [SerializeField] private GameObject panelMuerte;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private Button[] botonesMenu;         // Boton_Menu de cada panel
    [SerializeField] private Button[] botonesReintentar;   // se ocultan por ahora
    [SerializeField] private string escenaMenu = "MainMenu";

    private bool saliendo = false;

    private void Awake()
    {
        if (panelMuerte != null) panelMuerte.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);

        foreach (var b in botonesMenu)
            if (b != null) b.onClick.AddListener(VolverAlMenu);

        // "Reintentar" en red necesita reiniciar la escena y reaparecer: queda pendiente
        foreach (var b in botonesReintentar)
            if (b != null) b.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GestorPartida.OnPartidaTerminada += AlTerminar;
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback += AlDesconectarse;
    }

    private void OnDisable()
    {
        GestorPartida.OnPartidaTerminada -= AlTerminar;
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= AlDesconectarse;
    }

    private void AlTerminar(GestorPartida.EstadoPartida resultado)
    {
        var nm = NetworkManager.Singleton;
        if (nm != null && nm.LocalClient != null && nm.LocalClient.PlayerObject != null)
        {
            var cabeza = nm.LocalClient.PlayerObject.GetComponent<PlayerHeadController>();
            if (cabeza != null) cabeza.DetenerMovimiento();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameObject panel = resultado == GestorPartida.EstadoPartida.Derrota ? panelMuerte : panelVictoria;
        if (panel != null) panel.SetActive(true);
    }

    // Si el anfitrión se va, los clientes también vuelven al menú
    private void AlDesconectarse(ulong id)
    {
        var nm = NetworkManager.Singleton;
        if (nm != null && !nm.IsServer) VolverAlMenu();
    }

    public void VolverAlMenu()
    {
        if (saliendo) return;
        saliendo = true;
        StartCoroutine(Salir());
    }

    private IEnumerator Salir()
    {
        var nm = NetworkManager.Singleton;
        if (nm != null)
        {
            nm.Shutdown();
            while (nm != null && nm.ShutdownInProgress) yield return null;
            if (nm != null) Destroy(nm.gameObject);
        }
        SceneManager.LoadScene(escenaMenu);
    }
}