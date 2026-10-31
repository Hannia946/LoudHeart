using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuMultijugador : MonoBehaviour
{
    [SerializeField] private TMP_InputField campoIP;
    [SerializeField] private TMP_Text textoEstado;
    [SerializeField] private string escenaJuego = "Bosque_Greybox";
    [SerializeField] private ushort puerto = 7777;

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += AlDesconectarse;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= AlDesconectarse;
    }

    // Botón "Crear partida"
    public void Hostear()
    {
        var nm = NetworkManager.Singleton;

        if (!nm.StartHost())
        {
            textoEstado.text = "No se pudo crear la partida.";
            return;
        }

        textoEstado.text = "Partida creada.";
        // Usa el SceneManager de Netcode, NO el de Unity
        nm.SceneManager.LoadScene(escenaJuego, LoadSceneMode.Single);
    }

    // Botón "Unirse"
    public void Unirse()
    {
        string ip = string.IsNullOrWhiteSpace(campoIP.text) ? "127.0.0.1" : campoIP.text.Trim();

        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetConnectionData(ip, puerto);

        if (NetworkManager.Singleton.StartClient())
            textoEstado.text = "Conectando a " + ip + "...";
        else
            textoEstado.text = "No se pudo iniciar la conexión.";
    }

    private void AlDesconectarse(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
            textoEstado.text = "No se pudo conectar o se perdió la conexión.";
    }
}
