using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MonstruoIA : NetworkBehaviour
{
    public enum Estado { Patrulla, Persecucion, Retirada }

    [Header("Fases (índice = objetos clave recogidos, 0 a 5)")]
    [SerializeField] private float[] velocidadPorFase = { 1.5f, 2f, 2.5f, 3f, 4f, 5f };
    [SerializeField] private float[] radioDeteccionPorFase = { 12f, 14f, 16f, 18f, 22f, 26f };
    [SerializeField] private float[] tiempoRetiradaPorFase = { 8f, 7f, 6f, 5f, 4f, 3f };

    [Header("Detección según el ritmo del jugador")]
    [SerializeField] private float factorQuieto = 0.35f;
    [SerializeField] private float factorCaminando = 1f;
    [SerializeField] private float factorCorriendo = 1.8f;
    [SerializeField] private float umbralQuieto = 0.6f;      // m/s por debajo = quieto
    [SerializeField] private float umbralCorriendo = 4.5f;   // m/s por encima = trotando

    [Header("Comportamiento")]
    [SerializeField] private float factorVelocidadPatrulla = 0.6f;
    [SerializeField] private float radioMerodeo = 30f;
    [SerializeField] private float tiempoPerderInteres = 3f;
    [SerializeField] private float factorRadioPerdida = 1.3f;
    [SerializeField] private float distanciaRetirada = 25f;

    [Header("Ataque")]
    [SerializeField] private float rangoAtaque = 1.8f;
    [SerializeField] private float danoPorGolpe = 15f;
    [SerializeField] private float enfriamientoAtaque = 1.5f;

    [Header("Luz de la linterna")]
    [SerializeField] private float duracionRetiradaPorLuz = 3f;
    [SerializeField] private float inmunidadLuz = 6f;

    private class DatosRitmo
    {
        public Vector3 ultimaPos;
        public float velocidad;
    }

    private NavMeshAgent agente;
    private Estado estado = Estado.Patrulla;
    private NetworkObject objetivo;
    private float finRetirada;
    private float proximoAtaque;
    private float inmuneLuzHasta;
    private float tiempoSinDetectar;
    private float proximoCambioPatrulla;
    private int ultimaFase = -1;
    private bool avisoNavMesh;
    private readonly Dictionary<ulong, DatosRitmo> ritmos = new Dictionary<ulong, DatosRitmo>();

    public override void OnNetworkSpawn()
    {
        agente = GetComponent<NavMeshAgent>();

        // La IA solo corre en el servidor; los clientes reciben la posición por NetworkTransform
        if (!IsServer) agente.enabled = false;
    }

    private void Update()
    {
        if (!IsServer || !IsSpawned) return;

        if (!agente.isOnNavMesh)
        {
            if (!avisoNavMesh)
            {
                avisoNavMesh = true;
                Debug.LogWarning("[MONSTRUO] No está sobre el NavMesh. Colócalo sobre la zona azul.");
            }
            return;
        }

        ActualizarRitmos();

        int fase = ObtenerFase();
        if (fase != ultimaFase)
        {
            ultimaFase = fase;
            Debug.Log($"[MONSTRUO] Fase {fase}: velocidad {Valor(velocidadPorFase, fase)}");
        }

        float velocidad = Valor(velocidadPorFase, fase);

        switch (estado)
        {
            case Estado.Patrulla: ActualizarPatrulla(fase, velocidad); break;
            case Estado.Persecucion: ActualizarPersecucion(fase, velocidad); break;
            case Estado.Retirada: ActualizarRetirada(velocidad); break;
        }
    }

    // ===================== ESTADOS =====================

    private void ActualizarPatrulla(int fase, float velocidad)
    {
        agente.speed = velocidad * factorVelocidadPatrulla;

        NetworkObject detectado = BuscarJugadorDetectado(fase);
        if (detectado != null)
        {
            objetivo = detectado;
            tiempoSinDetectar = 0f;
            CambiarEstado(Estado.Persecucion);
            return;
        }

        bool llegó = !agente.pathPending && agente.remainingDistance <= agente.stoppingDistance + 0.5f;
        if (Time.time >= proximoCambioPatrulla || llegó)
        {
            ElegirPuntoDeMerodeo();
            proximoCambioPatrulla = Time.time + 12f;
        }
    }

    private void ActualizarPersecucion(int fase, float velocidad)
    {
        agente.speed = velocidad;

        if (objetivo == null)
        {
            CambiarEstado(Estado.Patrulla);
            return;
        }

        float radioPerdida = Valor(radioDeteccionPorFase, fase) * FactorRitmo(objetivo.OwnerClientId) * factorRadioPerdida;
        float dist = DistanciaPlana(transform.position, objetivo.transform.position);

        if (dist > radioPerdida)
        {
            tiempoSinDetectar += Time.deltaTime;
            if (tiempoSinDetectar >= tiempoPerderInteres)
            {
                objetivo = null;
                proximoCambioPatrulla = 0f;
                CambiarEstado(Estado.Patrulla);
                return;
            }
        }
        else
        {
            tiempoSinDetectar = 0f;
        }

        agente.SetDestination(objetivo.transform.position);

        if (dist <= rangoAtaque && Time.time >= proximoAtaque)
            Atacar(fase);
    }

    private void ActualizarRetirada(float velocidad)
    {
        agente.speed = velocidad;

        if (Time.time >= finRetirada)
        {
            proximoCambioPatrulla = 0f;
            CambiarEstado(Estado.Patrulla);
        }
    }

    // ===================== ACCIONES =====================

    private void Atacar(int fase)
    {
        proximoAtaque = Time.time + enfriamientoAtaque;

        var vital = objetivo.GetComponent<EstadoVitalExplorador>();
        if (vital != null) vital.RecibirDanoDesdeServidor(danoPorGolpe);

        IniciarRetirada(Valor(tiempoRetiradaPorFase, fase));
    }

    private void IniciarRetirada(float duracion)
    {
        finRetirada = Time.time + duracion;
        objetivo = null;
        CambiarEstado(Estado.Retirada);

        NetworkObject cercano = JugadorMasCercano();
        Vector3 dir = cercano != null ? transform.position - cercano.transform.position : transform.forward;
        dir.y = 0f;
        dir = dir.sqrMagnitude > 0.01f ? dir.normalized : transform.forward;

        if (NavMesh.SamplePosition(transform.position + dir * distanciaRetirada, out NavMeshHit hit, 15f, NavMesh.AllAreas))
            agente.SetDestination(hit.position);
    }

    // Lo llama el jugador que ilumina al monstruo con su linterna
    [ServerRpc(RequireOwnership = false)]
    public void RecibirLuzServerRpc()
    {
        if (Time.time < inmuneLuzHasta) return;   // evita aturdirlo sin parar

        inmuneLuzHasta = Time.time + inmunidadLuz;
        Debug.Log("[MONSTRUO] Iluminado: se retira.");
        IniciarRetirada(duracionRetiradaPorLuz);
    }

    private void ElegirPuntoDeMerodeo()
    {
        NetworkObject centro = JugadorMasCercano();
        if (centro == null) return;

        Vector2 c = Random.insideUnitCircle.normalized * Random.Range(10f, radioMerodeo);
        Vector3 deseado = centro.transform.position + new Vector3(c.x, 0f, c.y);

        if (NavMesh.SamplePosition(deseado, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            agente.SetDestination(hit.position);
    }

    // ===================== DETECCIÓN =====================

    private void ActualizarRitmos()
    {
        foreach (var cliente in NetworkManager.Singleton.ConnectedClientsList)
        {
            NetworkObject obj = cliente.PlayerObject;
            if (obj == null) continue;

            Vector3 pos = obj.transform.position;

            if (!ritmos.TryGetValue(cliente.ClientId, out DatosRitmo d))
            {
                ritmos[cliente.ClientId] = new DatosRitmo { ultimaPos = pos };
                continue;
            }

            Vector3 delta = pos - d.ultimaPos;
            delta.y = 0f;

            float v = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
            v = Mathf.Min(v, 12f);   // ignora teletransportes (spawn)

            d.velocidad = Mathf.Lerp(d.velocidad, v, 8f * Time.deltaTime);
            d.ultimaPos = pos;
        }
    }

    private float FactorRitmo(ulong clientId)
    {
        if (!ritmos.TryGetValue(clientId, out DatosRitmo d)) return factorCaminando;
        if (d.velocidad < umbralQuieto) return factorQuieto;
        if (d.velocidad >= umbralCorriendo) return factorCorriendo;
        return factorCaminando;
    }

    private NetworkObject BuscarJugadorDetectado(int fase)
    {
        float radioBase = Valor(radioDeteccionPorFase, fase);
        NetworkObject mejor = null;
        float mejorDist = float.MaxValue;

        foreach (var cliente in NetworkManager.Singleton.ConnectedClientsList)
        {
            NetworkObject obj = cliente.PlayerObject;
            if (obj == null) continue;

            float dist = DistanciaPlana(transform.position, obj.transform.position);
            float radio = radioBase * FactorRitmo(cliente.ClientId);

            if (dist <= radio && dist < mejorDist)
            {
                mejorDist = dist;
                mejor = obj;
            }
        }
        return mejor;
    }

    private NetworkObject JugadorMasCercano()
    {
        NetworkObject mejor = null;
        float mejorDist = float.MaxValue;

        foreach (var cliente in NetworkManager.Singleton.ConnectedClientsList)
        {
            NetworkObject obj = cliente.PlayerObject;
            if (obj == null) continue;

            float dist = DistanciaPlana(transform.position, obj.transform.position);
            if (dist < mejorDist)
            {
                mejorDist = dist;
                mejor = obj;
            }
        }
        return mejor;
    }

    // ===================== UTILIDADES =====================

    private int ObtenerFase()
    {
        int n = GestorPartida.Instancia != null ? GestorPartida.Instancia.objetosClaveRecolectados.Value : 0;
        return Mathf.Clamp(n, 0, velocidadPorFase.Length - 1);
    }

    private void CambiarEstado(Estado nuevo)
    {
        if (estado == nuevo) return;
        Debug.Log($"[MONSTRUO] {estado} ? {nuevo}");
        estado = nuevo;
    }

    private static float Valor(float[] datos, int fase)
    {
        return datos[Mathf.Clamp(fase, 0, datos.Length - 1)];
    }

    private static float DistanciaPlana(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    // Ayuda para ajustar: selecciona al monstruo en la escena para ver los radios
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccionPorFase.Length > 0 ? radioDeteccionPorFase[0] : 12f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }
}
