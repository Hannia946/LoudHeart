using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GestorPartida : NetworkBehaviour
{
    public enum EstadoPartida { Esperando, Jugando, Victoria, Derrota }

    public static GestorPartida Instancia { get; private set; }
    public static event Action<EstadoPartida> OnPartidaTerminada;

    public const int MetaObjetosClave = 5;

    public NetworkVariable<int> objetosClaveRecolectados = new NetworkVariable<int>(0);
    public NetworkVariable<EstadoPartida> estado = new NetworkVariable<EstadoPartida>(EstadoPartida.Esperando);

    public bool EnCurso => estado.Value == EstadoPartida.Jugando;

    private readonly HashSet<ulong> listos = new HashSet<ulong>();

    public override void OnNetworkSpawn()
    {
        Instancia = this;
        objetosClaveRecolectados.OnValueChanged += AlCambiarObjetos;
        estado.OnValueChanged += AlCambiarEstado;
    }

    public override void OnNetworkDespawn()
    {
        objetosClaveRecolectados.OnValueChanged -= AlCambiarObjetos;
        estado.OnValueChanged -= AlCambiarEstado;
        if (Instancia == this) Instancia = null;
    }

    private void AlCambiarObjetos(int anterior, int actual)
    {
        Debug.Log($"[META] Objetos clave del equipo: {actual}/{MetaObjetosClave}");
    }

    private void AlCambiarEstado(EstadoPartida anterior, EstadoPartida actual)
    {
        Debug.Log($"[PARTIDA] {anterior} -> {actual}");
        if (actual == EstadoPartida.Derrota || actual == EstadoPartida.Victoria)
            OnPartidaTerminada?.Invoke(actual);
    }

    public void RegistrarObjetoClave()   // solo servidor
    {
        if (!IsServer) return;
        objetosClaveRecolectados.Value++;
    }

    public void TerminarPartida(EstadoPartida resultado)   // solo servidor
    {
        if (!IsServer || !EnCurso) return;
        estado.Value = resultado;
    }

    // Cada jugador lo llama al pulsar "Comenzar"
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void JugadorListoRpc(RpcParams rpcParams = default)
    {
        if (estado.Value != EstadoPartida.Esperando) return;

        listos.Add(rpcParams.Receive.SenderClientId);
        int total = NetworkManager.ConnectedClientsList.Count;
        Debug.Log($"[PARTIDA] Listos: {listos.Count}/{total}");

        if (listos.Count >= total)
            estado.Value = EstadoPartida.Jugando;
    }
}