using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MonstruoIA : NetworkBehaviour
{
    public enum Estado
    {
        Patrulla,
        Persecucion,
        Retirada
    }

    // ============================================================
    // FASES
    // ============================================================

    [Header("Fases (índice = objetos clave recogidos, 0 a 5)")]
    [SerializeField]
    private float[] velocidadPorFase =
    {
        1.5f, 2f, 2.5f, 3f, 4f, 5f
    };

    [SerializeField]
    private float[] radioDeteccionPorFase =
    {
        12f, 14f, 16f, 18f, 22f, 26f
    };

    [SerializeField]
    private float[] tiempoRetiradaPorFase =
    {
        8f, 7f, 6f, 5f, 4f, 3f
    };


    // ============================================================
    // DETECCIÓN SEGÚN RITMO
    // ============================================================

    [Header("Detección según el ritmo del jugador")]
    [SerializeField] private float factorQuieto = 0.35f;
    [SerializeField] private float factorCaminando = 1f;
    [SerializeField] private float factorCorriendo = 1.8f;

    [SerializeField] private float umbralQuieto = 0.6f;
    [SerializeField] private float umbralCorriendo = 4.5f;


    // ============================================================
    // COMPORTAMIENTO
    // ============================================================

    [Header("Comportamiento")]
    [SerializeField] private float factorVelocidadPatrulla = 0.6f;
    [SerializeField] private float radioMerodeo = 30f;
    [SerializeField] private float tiempoPerderInteres = 3f;
    [SerializeField] private float factorRadioPerdida = 1.3f;
    [SerializeField] private float distanciaRetirada = 25f;


    // ============================================================
    // ATAQUE
    // ============================================================

    [Header("Ataque")]
    [SerializeField] private float rangoAtaque = 1.8f;
    [SerializeField] private float danoPorGolpe = 15f;
    [SerializeField] private float enfriamientoAtaque = 1.5f;


    // ============================================================
    // LINTERNA
    // ============================================================

    [Header("Luz de la linterna")]
    [SerializeField] private float duracionRetiradaPorLuz = 3f;
    [SerializeField] private float inmunidadLuz = 6f;


    // ============================================================
    // REATAQUE
    // ============================================================

    [Header("Reataque tras retirarse")]
    [SerializeField] private float tiempoMemoria = 12f;
    [SerializeField] private float radioReataque = 45f;
    [SerializeField] private float radioDeteccionMinimo = 6f;


    // ============================================================
    // AUDIO DEL MONSTRUO
    // ============================================================

    [Header("Audio del monstruo")]

    [SerializeField] private AudioClip sonidoRespiracion;
    [SerializeField] private AudioClip sonidoPasos;
    [SerializeField] private AudioClip sonidoGrunido;
    [SerializeField] private AudioClip sonidoDeteccion;
    [SerializeField] private AudioClip sonidoPersecucion;
    [SerializeField] private AudioClip sonidoAtaque;


    [Header("Volúmenes")]

    [Range(0f, 1f)]
    [SerializeField] private float volumenRespiracion = 0.75f;

    [Range(0f, 1f)]
    [SerializeField] private float volumenPasos = 0.9f;

    [Range(0f, 1f)]
    [SerializeField] private float volumenGrunido = 0.85f;

    [Range(0f, 1f)]
    [SerializeField] private float volumenDeteccion = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float volumenPersecucion = 0.65f;

    [Range(0f, 1f)]
    [SerializeField] private float volumenAtaque = 1f;


    [Header("Distancia audio 3D")]

    [SerializeField] private float distanciaMinimaAudio = 2f;
    [SerializeField] private float distanciaMaximaAudio = 25f;


    [Header("Gruñidos")]

    [SerializeField] private float intervaloGrunidoMin = 8f;
    [SerializeField] private float intervaloGrunidoMax = 16f;


    // ============================================================
    // FUENTES DE AUDIO
    // ============================================================

    private AudioSource fuenteRespiracion;
    private AudioSource fuentePasos;
    private AudioSource fuenteEfectos;
    private AudioSource fuentePersecucion;

    private Vector3 posicionAnterior;
    private float proximoGrunido;


    // ============================================================
    // VARIABLES DE IA
    // ============================================================

    private NetworkObject ultimoObjetivo;
    private float memoriaHasta;

    // Guarda qué jugador está siendo perseguido.
    // Lo usamos para reproducir el alivio únicamente a ese jugador.
    private ulong jugadorPerseguidoId = ulong.MaxValue;


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

    private readonly Dictionary<ulong, DatosRitmo> ritmos =
        new Dictionary<ulong, DatosRitmo>();


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        ConfigurarAudio();
    }


    // ============================================================
    // CONFIGURACIÓN DE AUDIO
    // ============================================================

    private void ConfigurarAudio()
    {
        // RESPIRACIÓN
        fuenteRespiracion =
            gameObject.AddComponent<AudioSource>();

        ConfigurarFuente3D(
            fuenteRespiracion
        );

        fuenteRespiracion.clip =
            sonidoRespiracion;

        fuenteRespiracion.loop =
            true;

        fuenteRespiracion.volume =
            volumenRespiracion;


        // PASOS
        fuentePasos =
            gameObject.AddComponent<AudioSource>();

        ConfigurarFuente3D(
            fuentePasos
        );

        fuentePasos.clip =
            sonidoPasos;

        fuentePasos.loop =
            true;

        fuentePasos.volume =
            volumenPasos;


        // EFECTOS:
        // gruñido, detección y ataque
        fuenteEfectos =
            gameObject.AddComponent<AudioSource>();

        ConfigurarFuente3D(
            fuenteEfectos
        );

        fuenteEfectos.loop =
            false;


        // PERSECUCIÓN
        // Este sonido representa tensión para el jugador,
        // por eso se reproduce como audio 2D.
        fuentePersecucion =
            gameObject.AddComponent<AudioSource>();

        fuentePersecucion.playOnAwake =
            false;

        fuentePersecucion.loop =
            true;

        fuentePersecucion.spatialBlend =
            0f;

        fuentePersecucion.volume =
            volumenPersecucion;

        fuentePersecucion.clip =
            sonidoPersecucion;
    }


    private void ConfigurarFuente3D(
        AudioSource fuente
    )
    {
        fuente.playOnAwake =
            false;

        fuente.spatialBlend =
            1f;

        fuente.rolloffMode =
            AudioRolloffMode.Linear;

        fuente.minDistance =
            distanciaMinimaAudio;

        fuente.maxDistance =
            distanciaMaximaAudio;

        fuente.dopplerLevel =
            0f;

        fuente.spread =
            0f;
    }


    // ============================================================
    // NETWORK SPAWN
    // ============================================================

    public override void OnNetworkSpawn()
    {
        agente =
            GetComponent<NavMeshAgent>();

        posicionAnterior =
            transform.position;


        IniciarRespiracion();


        if (IsServer)
        {
            proximoGrunido =
                Time.time +
                Random.Range(
                    intervaloGrunidoMin,
                    intervaloGrunidoMax
                );
        }


        // La IA solo se ejecuta en el servidor.
        // Los clientes reciben la posición por NetworkTransform.
        if (!IsServer)
        {
            agente.enabled =
                false;
        }
    }


    public override void OnNetworkDespawn()
    {
        DetenerTodoAudio();
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        // El movimiento del monstruo también se recibe
        // visualmente en los clientes.
        ActualizarAudioMovimiento();


        if (
            !IsServer ||
            !IsSpawned
        )
        {
            return;
        }


        if (
            GestorPartida.Instancia != null &&
            !GestorPartida.Instancia.EnCurso
        )
        {
            if (
                agente != null &&
                agente.isOnNavMesh
            )
            {
                agente.isStopped =
                    true;
            }

            return;
        }


        if (
            agente.isOnNavMesh &&
            agente.isStopped
        )
        {
            agente.isStopped =
                false;
        }


        ActualizarRitmos();


        int fase =
            ObtenerFase();


        if (
            fase !=
            ultimaFase
        )
        {
            ultimaFase =
                fase;


            Debug.Log(
                $"[MONSTRUO] Fase {fase}: " +
                $"velocidad {Valor(velocidadPorFase, fase)}"
            );
        }


        float velocidad =
            Valor(
                velocidadPorFase,
                fase
            );


        switch (estado)
        {
            case Estado.Patrulla:

                ActualizarPatrulla(
                    fase,
                    velocidad
                );

                break;


            case Estado.Persecucion:

                ActualizarPersecucion(
                    fase,
                    velocidad
                );

                break;


            case Estado.Retirada:

                ActualizarRetirada(
                    velocidad
                );

                break;
        }


        ActualizarGrunidos();
    }


    // ============================================================
    // AUDIO - RESPIRACIÓN
    // ============================================================

    private void IniciarRespiracion()
    {
        if (
            fuenteRespiracion == null ||
            sonidoRespiracion == null
        )
        {
            return;
        }


        fuenteRespiracion.clip =
            sonidoRespiracion;


        if (
            !fuenteRespiracion.isPlaying
        )
        {
            fuenteRespiracion.Play();
        }
    }


    // ============================================================
    // AUDIO - PASOS
    // ============================================================

    private void ActualizarAudioMovimiento()
    {
        if (
            !IsSpawned ||
            fuentePasos == null ||
            sonidoPasos == null
        )
        {
            return;
        }


        float dt =
            Mathf.Max(
                Time.deltaTime,
                0.0001f
            );


        Vector3 delta =
            transform.position -
            posicionAnterior;


        delta.y =
            0f;


        float velocidadVisual =
            delta.magnitude /
            dt;


        bool seEstaMoviendo =
            velocidadVisual >
            0.15f;


        if (seEstaMoviendo)
        {
            if (
                !fuentePasos.isPlaying
            )
            {
                fuentePasos.clip =
                    sonidoPasos;

                fuentePasos.Play();
            }
        }
        else
        {
            if (
                fuentePasos.isPlaying
            )
            {
                fuentePasos.Stop();
            }
        }


        posicionAnterior =
            transform.position;
    }


    // ============================================================
    // AUDIO - GRUÑIDOS
    // ============================================================

    private void ActualizarGrunidos()
    {
        if (
            sonidoGrunido == null ||
            Time.time <
            proximoGrunido
        )
        {
            return;
        }


        if (
            estado == Estado.Patrulla ||
            estado == Estado.Persecucion
        )
        {
            ReproducirGrunidoRpc();
        }


        proximoGrunido =
            Time.time +
            Random.Range(
                intervaloGrunidoMin,
                intervaloGrunidoMax
            );
    }


    [Rpc(SendTo.Everyone)]
    private void ReproducirGrunidoRpc()
    {
        if (
            fuenteEfectos != null &&
            sonidoGrunido != null
        )
        {
            fuenteEfectos.PlayOneShot(
                sonidoGrunido,
                volumenGrunido
            );
        }
    }


    // ============================================================
    // AUDIO - DETECCIÓN
    // ============================================================

    [Rpc(SendTo.Everyone)]
    private void ReproducirDeteccionRpc()
    {
        if (
            fuenteEfectos != null &&
            sonidoDeteccion != null
        )
        {
            fuenteEfectos.PlayOneShot(
                sonidoDeteccion,
                volumenDeteccion
            );
        }
    }


    // ============================================================
    // AUDIO - PERSECUCIÓN
    // ============================================================

    [Rpc(SendTo.Everyone)]
    private void CambiarAudioPersecucionRpc(
        bool activo
    )
    {
        if (
            fuentePersecucion == null ||
            sonidoPersecucion == null
        )
        {
            return;
        }


        fuentePersecucion.clip =
            sonidoPersecucion;


        if (activo)
        {
            if (
                !fuentePersecucion.isPlaying
            )
            {
                fuentePersecucion.Play();
            }
        }
        else
        {
            if (
                fuentePersecucion.isPlaying
            )
            {
                fuentePersecucion.Stop();
            }
        }
    }


    // ============================================================
    // AUDIO - ATAQUE
    // ============================================================

    [Rpc(SendTo.Everyone)]
    private void ReproducirAtaqueRpc()
    {
        if (
            fuenteEfectos != null &&
            sonidoAtaque != null
        )
        {
            fuenteEfectos.PlayOneShot(
                sonidoAtaque,
                volumenAtaque
            );
        }
    }


    // ============================================================
    // AUDIO - ALIVIO DEL JUGADOR
    // ============================================================

    [Rpc(SendTo.Everyone)]
    private void ReproducirAlivioRpc(
        ulong clientId
    )
    {
        AudioAlivioJugador
            .ReproducirParaCliente(
                clientId
            );
    }


    private void DetenerTodoAudio()
    {
        if (
            fuenteRespiracion != null
        )
        {
            fuenteRespiracion.Stop();
        }


        if (
            fuentePasos != null
        )
        {
            fuentePasos.Stop();
        }


        if (
            fuenteEfectos != null
        )
        {
            fuenteEfectos.Stop();
        }


        if (
            fuentePersecucion != null
        )
        {
            fuentePersecucion.Stop();
        }
    }


    // ============================================================
    // PATRULLA
    // ============================================================

    private void ActualizarPatrulla(
        int fase,
        float velocidad
    )
    {
        agente.speed =
            velocidad *
            factorVelocidadPatrulla;


        NetworkObject detectado =
            BuscarJugadorDetectado(
                fase
            );


        if (
            detectado != null
        )
        {
            objetivo =
                detectado;


            // Guardamos quién está siendo perseguido.
            jugadorPerseguidoId =
                detectado.OwnerClientId;


            tiempoSinDetectar =
                0f;


            CambiarEstado(
                Estado.Persecucion
            );


            return;
        }


        bool llego =
            !agente.pathPending &&
            agente.remainingDistance <=
            agente.stoppingDistance +
            0.5f;


        if (
            Time.time >=
            proximoCambioPatrulla ||
            llego
        )
        {
            ElegirPuntoDeMerodeo();


            proximoCambioPatrulla =
                Time.time +
                12f;
        }
    }


    // ============================================================
    // PERSECUCIÓN
    // ============================================================

    private void ActualizarPersecucion(
        int fase,
        float velocidad
    )
    {
        agente.speed =
            velocidad;


        if (
            objetivo == null
        )
        {
            CambiarEstado(
                Estado.Patrulla
            );

            return;
        }


        float radio =
            Mathf.Max(
                Valor(
                    radioDeteccionPorFase,
                    fase
                )
                *
                FactorRitmo(
                    objetivo.OwnerClientId
                ),
                radioDeteccionMinimo
            );


        float radioPerdida =
            radio *
            factorRadioPerdida;


        float dist =
            DistanciaPlana(
                transform.position,
                objetivo.transform.position
            );


        // Mientras dura la memoria,
        // no pierde el interés aunque el jugador
        // se quede quieto.
        if (
            dist >
            radioPerdida &&
            Time.time >=
            memoriaHasta
        )
        {
            tiempoSinDetectar +=
                Time.deltaTime;


            // ====================================================
            // EL JUGADOR LOGRÓ ESCAPAR
            // ====================================================

            if (
                tiempoSinDetectar >=
                tiempoPerderInteres
            )
            {
                if (
                    jugadorPerseguidoId !=
                    ulong.MaxValue
                )
                {
                    ReproducirAlivioRpc(
                        jugadorPerseguidoId
                    );
                }


                jugadorPerseguidoId =
                    ulong.MaxValue;


                objetivo =
                    null;


                proximoCambioPatrulla =
                    0f;


                CambiarEstado(
                    Estado.Patrulla
                );


                return;
            }
        }
        else
        {
            tiempoSinDetectar =
                0f;
        }


        agente.SetDestination(
            objetivo.transform.position
        );


        if (
            dist <=
            rangoAtaque &&
            Time.time >=
            proximoAtaque
        )
        {
            Atacar(
                fase
            );
        }
    }


    // ============================================================
    // RETIRADA
    // ============================================================

    private void ActualizarRetirada(
        float velocidad
    )
    {
        agente.speed =
            velocidad;


        if (
            Time.time <
            finRetirada
        )
        {
            return;
        }


        if (
            ultimoObjetivo != null &&
            DistanciaPlana(
                transform.position,
                ultimoObjetivo.transform.position
            )
            <=
            radioReataque
        )
        {
            objetivo =
                ultimoObjetivo;


            // Si vuelve a perseguirlo,
            // conservamos quién es el jugador.
            jugadorPerseguidoId =
                objetivo.OwnerClientId;


            tiempoSinDetectar =
                0f;


            CambiarEstado(
                Estado.Persecucion
            );
        }
        else
        {
            proximoCambioPatrulla =
                0f;


            CambiarEstado(
                Estado.Patrulla
            );
        }
    }


    // ============================================================
    // ATAQUE
    // ============================================================

    private void Atacar(
        int fase
    )
    {
        proximoAtaque =
            Time.time +
            enfriamientoAtaque;


        ReproducirAtaqueRpc();


        var vital =
            objetivo.GetComponent<
                EstadoVitalExplorador
            >();


        if (
            vital != null
        )
        {
            vital.RecibirDanoDesdeServidor(
                danoPorGolpe
            );
        }


        IniciarRetirada(
            Valor(
                tiempoRetiradaPorFase,
                fase
            )
        );
    }


    // ============================================================
    // INICIAR RETIRADA
    // ============================================================

    private void IniciarRetirada(
        float duracion
    )
    {
        ultimoObjetivo =
            objetivo != null
                ? objetivo
                : JugadorMasCercano();


        memoriaHasta =
            Time.time +
            duracion +
            tiempoMemoria;


        finRetirada =
            Time.time +
            duracion;


        objetivo =
            null;


        CambiarEstado(
            Estado.Retirada
        );


        NetworkObject cercano =
            JugadorMasCercano();


        Vector3 dir =
            cercano != null
                ? transform.position -
                  cercano.transform.position
                : transform.forward;


        dir.y =
            0f;


        dir =
            dir.sqrMagnitude >
            0.01f
                ? dir.normalized
                : transform.forward;


        if (
            NavMesh.SamplePosition(
                transform.position +
                dir *
                distanciaRetirada,
                out NavMeshHit hit,
                15f,
                NavMesh.AllAreas
            )
        )
        {
            agente.SetDestination(
                hit.position
            );
        }
    }


    // ============================================================
    // LUZ DE LA LINTERNA
    // ============================================================

    [Rpc(
        SendTo.Server,
        InvokePermission =
            RpcInvokePermission.Everyone
    )]
    public void RecibirLuzServerRpc()
    {
        if (
            Time.time <
            inmuneLuzHasta
        )
        {
            return;
        }


        inmuneLuzHasta =
            Time.time +
            inmunidadLuz;


        Debug.Log(
            "[MONSTRUO] Iluminado: se retira."
        );


        IniciarRetirada(
            duracionRetiradaPorLuz
        );
    }


    // ============================================================
    // MERODEO
    // ============================================================

    private void ElegirPuntoDeMerodeo()
    {
        NetworkObject centro =
            JugadorMasCercano();


        if (
            centro == null
        )
        {
            return;
        }


        Vector2 c =
            Random.insideUnitCircle
            .normalized
            *
            Random.Range(
                10f,
                radioMerodeo
            );


        Vector3 deseado =
            centro.transform.position +
            new Vector3(
                c.x,
                0f,
                c.y
            );


        if (
            NavMesh.SamplePosition(
                deseado,
                out NavMeshHit hit,
                10f,
                NavMesh.AllAreas
            )
        )
        {
            agente.SetDestination(
                hit.position
            );
        }
    }


    // ============================================================
    // RITMOS DEL JUGADOR
    // ============================================================

    private void ActualizarRitmos()
    {
        foreach (
            var cliente in
            NetworkManager.Singleton
                .ConnectedClientsList
        )
        {
            NetworkObject obj =
                cliente.PlayerObject;


            if (
                obj == null
            )
            {
                continue;
            }


            Vector3 pos =
                obj.transform.position;


            if (
                !ritmos.TryGetValue(
                    cliente.ClientId,
                    out DatosRitmo d
                )
            )
            {
                ritmos[
                    cliente.ClientId
                ] =
                    new DatosRitmo
                    {
                        ultimaPos =
                            pos
                    };


                continue;
            }


            Vector3 delta =
                pos -
                d.ultimaPos;


            delta.y =
                0f;


            float v =
                Time.deltaTime >
                0f
                    ? delta.magnitude /
                      Time.deltaTime
                    : 0f;


            // Ignora teletransportes de spawn.
            v =
                Mathf.Min(
                    v,
                    12f
                );


            d.velocidad =
                Mathf.Lerp(
                    d.velocidad,
                    v,
                    8f *
                    Time.deltaTime
                );


            d.ultimaPos =
                pos;
        }
    }


    // ============================================================
    // FACTOR DE RITMO
    // ============================================================

    private float FactorRitmo(
        ulong clientId
    )
    {
        if (
            !ritmos.TryGetValue(
                clientId,
                out DatosRitmo d
            )
        )
        {
            return factorCaminando;
        }


        if (
            d.velocidad <
            umbralQuieto
        )
        {
            return factorQuieto;
        }


        if (
            d.velocidad >=
            umbralCorriendo
        )
        {
            return factorCorriendo;
        }


        return factorCaminando;
    }


    // ============================================================
    // BUSCAR JUGADOR DETECTADO
    // ============================================================

    private NetworkObject BuscarJugadorDetectado(
        int fase
    )
    {
        float radioBase =
            Valor(
                radioDeteccionPorFase,
                fase
            );


        NetworkObject mejor =
            null;


        float mejorDist =
            float.MaxValue;


        foreach (
            var cliente in
            NetworkManager.Singleton
                .ConnectedClientsList
        )
        {
            NetworkObject obj =
                cliente.PlayerObject;


            if (
                obj == null
            )
            {
                continue;
            }


            float dist =
                DistanciaPlana(
                    transform.position,
                    obj.transform.position
                );


            float radio =
                Mathf.Max(
                    radioBase *
                    FactorRitmo(
                        cliente.ClientId
                    ),
                    radioDeteccionMinimo
                );


            if (
                dist <= radio &&
                dist < mejorDist
            )
            {
                mejorDist =
                    dist;


                mejor =
                    obj;
            }
        }


        return mejor;
    }


    // ============================================================
    // JUGADOR MÁS CERCANO
    // ============================================================

    private NetworkObject JugadorMasCercano()
    {
        NetworkObject mejor =
            null;


        float mejorDist =
            float.MaxValue;


        foreach (
            var cliente in
            NetworkManager.Singleton
                .ConnectedClientsList
        )
        {
            NetworkObject obj =
                cliente.PlayerObject;


            if (
                obj == null
            )
            {
                continue;
            }


            float dist =
                DistanciaPlana(
                    transform.position,
                    obj.transform.position
                );


            if (
                dist <
                mejorDist
            )
            {
                mejorDist =
                    dist;


                mejor =
                    obj;
            }
        }


        return mejor;
    }


    // ============================================================
    // OBTENER FASE
    // ============================================================

    private int ObtenerFase()
    {
        int n =
            GestorPartida.Instancia != null
                ? GestorPartida
                    .Instancia
                    .objetosClaveRecolectados
                    .Value
                : 0;


        return Mathf.Clamp(
            n,
            0,
            velocidadPorFase.Length -
            1
        );
    }


    // ============================================================
    // CAMBIAR ESTADO
    // ============================================================

    private void CambiarEstado(
        Estado nuevo
    )
    {
        if (
            estado ==
            nuevo
        )
        {
            return;
        }


        Estado anterior =
            estado;


        Debug.Log(
            $"[MONSTRUO] " +
            $"{estado} -> {nuevo}"
        );


        estado =
            nuevo;


        // ========================================================
        // AUDIO SEGÚN ESTADO
        // ========================================================

        if (
            nuevo ==
            Estado.Persecucion
        )
        {
            // Si viene de Patrulla significa
            // que acaba de detectar al jugador.
            if (
                anterior ==
                Estado.Patrulla
            )
            {
                ReproducirDeteccionRpc();
            }


            CambiarAudioPersecucionRpc(
                true
            );
        }
        else
        {
            CambiarAudioPersecucionRpc(
                false
            );
        }
    }


    // ============================================================
    // VALOR POR FASE
    // ============================================================

    private static float Valor(
        float[] datos,
        int fase
    )
    {
        return datos[
            Mathf.Clamp(
                fase,
                0,
                datos.Length -
                1
            )
        ];
    }


    // ============================================================
    // DISTANCIA PLANA
    // ============================================================

    private static float DistanciaPlana(
        Vector3 a,
        Vector3 b
    )
    {
        a.y =
            0f;


        b.y =
            0f;


        return Vector3.Distance(
            a,
            b
        );
    }


    // ============================================================
    // GIZMOS
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.yellow;


        Gizmos.DrawWireSphere(
            transform.position,
            radioDeteccionPorFase.Length >
            0
                ? radioDeteccionPorFase[0]
                : 12f
        );


        Gizmos.color =
            Color.red;


        Gizmos.DrawWireSphere(
            transform.position,
            rangoAtaque
        );
    }
}