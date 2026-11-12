using Unity.Netcode;
using UnityEngine;

public class EstadoExplorador : NetworkBehaviour
{
    public enum EstadoPersonaje
    {
        Quieto, Caminando, Corriendo, Comiendo, Curando, Cargando, Tomando
    }

    [Header("Estado Actual del Explorador")]
    public EstadoPersonaje estadoActual = EstadoPersonaje.Quieto;

    [Header("Control de Cabeza y Movimiento")]
    public PlayerHeadController headController;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;   // los remotos no escuchan comandos
        VoiceProcessor.OnComandoDetectado += ProcesarComandoVoz;
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
            VoiceProcessor.OnComandoDetectado -= ProcesarComandoVoz;
    }

    private void ProcesarComandoVoz(string comando)
    {
        if (GestorPartida.Instancia != null && !GestorPartida.Instancia.EnCurso) return;
        if (string.IsNullOrWhiteSpace(comando)) return;

        comando = comando.ToLower().Trim();
        Debug.Log("[ESTADO EXPLORADOR] Comando recibido: " + comando);

        switch (comando)
        {
            case "alto": CambiarEstado(EstadoPersonaje.Quieto); break;
            case "avanza": CambiarEstado(EstadoPersonaje.Caminando); break;
            case "trota": CambiarEstado(EstadoPersonaje.Corriendo); break;
                // "come", "sana", "carga" y "toma" ya no se usan: se ignoran
        }
    }

    private void CambiarEstado(EstadoPersonaje nuevoEstado)
    {
        estadoActual = nuevoEstado;
        Debug.Log("[ESTADO EXPLORADOR] Nuevo estado: " + estadoActual);

        if (headController == null)
        {
            Debug.LogWarning("[MOVIMIENTO] PlayerHeadController no está asignado.");
            return;
        }

        switch (nuevoEstado)
        {
            case EstadoPersonaje.Caminando: headController.IniciarMovimiento(3f); break;
            case EstadoPersonaje.Corriendo: headController.IniciarMovimiento(6f); break;
            case EstadoPersonaje.Quieto: headController.DetenerMovimiento(); break;
        }
    }
}