using Unity.Netcode;
using UnityEngine;

public class GestorPartida : NetworkBehaviour
{
    public static GestorPartida Instancia { get; private set; }

    public const int MetaObjetosClave = 5;

    public NetworkVariable<int> objetosClaveRecolectados = new NetworkVariable<int>(0);

    public override void OnNetworkSpawn()
    {
        Instancia = this;
        objetosClaveRecolectados.OnValueChanged += AlCambiar;
    }

    public override void OnNetworkDespawn()
    {
        objetosClaveRecolectados.OnValueChanged -= AlCambiar;
        if (Instancia == this) Instancia = null;
    }

    private void AlCambiar(int anterior, int actual)
    {
        Debug.Log($"[META] Objetos clave del equipo: {actual}/{MetaObjetosClave}");
        if (actual >= MetaObjetosClave) Debug.Log("[META] ¡Meta alcanzada!");
    }

    public void RegistrarObjetoClave()   // solo se llama en el servidor
    {
        if (!IsServer) return;
        objetosClaveRecolectados.Value++;
    }
}
