using Unity.Netcode;
using UnityEngine;

public class EstadoVitalExplorador : NetworkBehaviour
{
    [Header("VIDA")]
    [Min(1f)] public float vidaMaxima = 100f;
    [Range(0f, 100f)] public float vidaActual = 60f;
    [Min(0f)] public float curacionPorVenda = 30f;

    [Header("ENERGÍA (sin uso)")]
    [Min(1f)] public float energiaMaxima = 100f;
    [Range(0f, 100f)] public float energiaActual = 50f;

    [Header("BATERÍA DE LA LINTERNA")]
    [Min(1f)] public float bateriaMaxima = 100f;
    [Range(0f, 100f)] public float bateriaActual = 40f;
    [Min(0f)] public float recargaPorPila = 40f;

    [Header("LINTERNA")]
    public Light luzLinterna;
    [Min(0f)] public float consumoBateriaPorSegundo = 2f;

    [Header("USO AUTOMÁTICO DE CONSUMIBLES")]
    public PlayerInventoryManager inventario;
    [Range(0f, 100f)] public float umbralVida = 40f;       // usa venda si la vida baja de aquí
    [Range(0f, 100f)] public float umbralBateria = 25f;    // usa pila si la batería baja de aquí
    [Min(0f)] public float enfriamientoUso = 1f;           // segundos entre usos

    private float proximoUsoPermitido = 0f;

    public override void OnNetworkSpawn()
    {
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);
        energiaActual = Mathf.Clamp(energiaActual, 0f, energiaMaxima);
        bateriaActual = Mathf.Clamp(bateriaActual, 0f, bateriaMaxima);
    }

    private void Update()
    {
        if (!IsSpawned || !IsOwner) return;   // solo el dueño gasta y usa

        ConsumirBateria();
        UsoAutomatico();
    }

    private void ConsumirBateria()
    {
        if (luzLinterna == null || !luzLinterna.enabled) return;

        if (bateriaActual > 0f)
        {
            bateriaActual = Mathf.Max(bateriaActual - consumoBateriaPorSegundo * Time.deltaTime, 0f);
        }

        if (bateriaActual <= 0f)
        {
            bateriaActual = 0f;
            luzLinterna.enabled = false;
            Debug.LogWarning("[LINTERNA] Batería agotada.");
        }
    }

    private void UsoAutomatico()
    {
        if (inventario == null || Time.time < proximoUsoPermitido) return;

        if (vidaActual <= umbralVida && inventario.cantidadVendas > 0 && PuedeSanar())
        {
            inventario.UsarVenda();
            Sanar();
            proximoUsoPermitido = Time.time + enfriamientoUso;
            return;
        }

        if (bateriaActual <= umbralBateria && inventario.cantidadPilas > 0 && PuedeRecargar())
        {
            inventario.UsarPila();
            RecargarBateria();
            proximoUsoPermitido = Time.time + enfriamientoUso;
        }
    }

    public bool PuedeSanar() { return vidaActual < vidaMaxima; }

    public void Sanar()
    {
        float anterior = vidaActual;
        vidaActual = Mathf.Min(vidaActual + curacionPorVenda, vidaMaxima);
        Debug.Log($"[VIDA] {anterior:F0} → {vidaActual:F0} / {vidaMaxima:F0}");
    }

    public bool PuedeRecargar() { return bateriaActual < bateriaMaxima; }

    public void RecargarBateria()
    {
        float anterior = bateriaActual;
        bateriaActual = Mathf.Min(bateriaActual + recargaPorPila, bateriaMaxima);

        if (luzLinterna != null && bateriaActual > 0f)
            luzLinterna.enabled = true;   // por si se había apagado al llegar a 0

        Debug.Log($"[BATERÍA] {anterior:F0} → {bateriaActual:F0} / {bateriaMaxima:F0}");
    }

    public void RecibirDanoDesdeServidor(float dano)
    {
        if (!IsServer) return;

        RecibirDanoClientRpc(dano, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
        });
    }

    [ClientRpc]
    private void RecibirDanoClientRpc(float dano, ClientRpcParams p = default)
    {
        float anterior = vidaActual;
        vidaActual = Mathf.Max(vidaActual - dano, 0f);
        Debug.Log($"[VIDA] Golpe del monstruo: {anterior:F0} → {vidaActual:F0} / {vidaMaxima:F0}");

        if (vidaActual <= 0f)
            Debug.LogWarning("[VIDA] El jugador se quedó sin vida.");   // pendiente: definir qué pasa
    }
}