using UnityEngine;

public class EstadoExplorador : MonoBehaviour
{
    // ============================================================
    // ESTADOS
    // ============================================================

    public enum EstadoPersonaje
    {
        Quieto,
        Caminando,
        Corriendo,
        Comiendo,
        Curando,
        Cargando,
        Tomando
    }


    // ============================================================
    // ESTADO ACTUAL
    // ============================================================

    [Header("Estado Actual del Explorador")]

    public EstadoPersonaje estadoActual =
        EstadoPersonaje.Quieto;


    // ============================================================
    // CONTROL DE MOVIMIENTO
    // ============================================================

    [Header("Control de Cabeza y Movimiento")]

    public PlayerHeadController headController;


    // ============================================================
    // INVENTARIO
    // ============================================================

    [Header("Inventario del Explorador")]

    public PlayerInventoryManager inventarioExplorador;


    // ============================================================
    // ESTADO VITAL
    // ============================================================

    [Header("Estado Vital del Explorador")]

    public EstadoVitalExplorador estadoVitalExplorador;


    // ============================================================
    // EVENTOS
    // ============================================================

    private void OnEnable()
    {
        VoiceProcessor.OnComandoDetectado +=
            ProcesarComandoVoz;
    }


    private void OnDisable()
    {
        VoiceProcessor.OnComandoDetectado -=
            ProcesarComandoVoz;
    }


    // ============================================================
    // PROCESAR COMANDO
    // ============================================================

    private void ProcesarComandoVoz(
        string comando
    )
    {
        if (
            string.IsNullOrWhiteSpace(
                comando
            )
        )
        {
            return;
        }


        comando =
            comando
            .ToLower()
            .Trim();


        Debug.Log(
            "[ESTADO EXPLORADOR] Comando recibido: " +
            comando
        );


        switch (comando)
        {
            case "alto":

                CambiarEstado(
                    EstadoPersonaje.Quieto
                );

                break;


            case "avanza":

                CambiarEstado(
                    EstadoPersonaje.Caminando
                );

                break;


            case "trota":

                CambiarEstado(
                    EstadoPersonaje.Corriendo
                );

                break;


            case "come":

                EjecutarCome();

                break;


            case "sana":

                EjecutarSana();

                break;


            case "carga":

                EjecutarCarga();

                break;


            case "toma":

                EjecutarToma();

                break;
        }
    }


    // ============================================================
    // COMANDO SANA
    // ============================================================

    private void EjecutarSana()
    {
        if (inventarioExplorador == null)
        {
            Debug.LogWarning(
                "[SANA] No se ha asignado PlayerInventoryManager."
            );

            return;
        }


        if (estadoVitalExplorador == null)
        {
            Debug.LogWarning(
                "[SANA] No se ha asignado EstadoVitalExplorador."
            );

            return;
        }


        if (
            !estadoVitalExplorador.PuedeSanar()
        )
        {
            Debug.LogWarning(
                "[SANA] La vida ya está completa. " +
                "No se utilizó ninguna venda."
            );

            return;
        }


        bool vendaUtilizada =
            inventarioExplorador.UsarVenda();


        if (!vendaUtilizada)
        {
            Debug.LogWarning(
                "[SANA] No se pudo ejecutar porque no hay vendas."
            );

            return;
        }


        estadoVitalExplorador.Sanar();


        CambiarEstado(
            EstadoPersonaje.Curando
        );


        Debug.Log(
            "[SANA] Comando ejecutado correctamente."
        );
    }


    // ============================================================
    // COMANDO COME
    // ============================================================

    private void EjecutarCome()
    {
        if (inventarioExplorador == null)
        {
            Debug.LogWarning(
                "[COME] No se ha asignado PlayerInventoryManager."
            );

            return;
        }


        if (estadoVitalExplorador == null)
        {
            Debug.LogWarning(
                "[COME] No se ha asignado EstadoVitalExplorador."
            );

            return;
        }


        if (
            !estadoVitalExplorador.PuedeComer()
        )
        {
            Debug.LogWarning(
                "[COME] La energía ya está completa. " +
                "No se utilizó comida."
            );

            return;
        }


        bool comidaUtilizada =
            inventarioExplorador.UsarComida();


        if (!comidaUtilizada)
        {
            Debug.LogWarning(
                "[COME] No se pudo ejecutar porque no hay comida."
            );

            return;
        }


        estadoVitalExplorador.Comer();


        CambiarEstado(
            EstadoPersonaje.Comiendo
        );


        Debug.Log(
            "[COME] Comando ejecutado correctamente."
        );
    }


    // ============================================================
    // COMANDO CARGA
    // ============================================================

    private void EjecutarCarga()
    {
        if (inventarioExplorador == null)
        {
            Debug.LogWarning(
                "[CARGA] No se ha asignado PlayerInventoryManager."
            );

            return;
        }


        if (estadoVitalExplorador == null)
        {
            Debug.LogWarning(
                "[CARGA] No se ha asignado EstadoVitalExplorador."
            );

            return;
        }


        if (
            !estadoVitalExplorador.PuedeRecargar()
        )
        {
            Debug.LogWarning(
                "[CARGA] La batería ya está completa. " +
                "No se utilizó ninguna pila."
            );

            return;
        }


        bool pilaUtilizada =
            inventarioExplorador.UsarPila();


        if (!pilaUtilizada)
        {
            Debug.LogWarning(
                "[CARGA] No se pudo ejecutar porque no hay pilas."
            );

            return;
        }


        estadoVitalExplorador
            .RecargarBateria();


        CambiarEstado(
            EstadoPersonaje.Cargando
        );


        Debug.Log(
            "[CARGA] Comando ejecutado correctamente."
        );
    }


    // ============================================================
    // COMANDO TOMA
    // ============================================================

    private void EjecutarToma()
    {
        if (inventarioExplorador == null)
        {
            Debug.LogWarning(
                "[TOMA] No se ha asignado PlayerInventoryManager."
            );

            return;
        }


        bool objetoTomado =
            inventarioExplorador
                .EjecutarComandoToma();


        if (!objetoTomado)
        {
            return;
        }


        CambiarEstado(
            EstadoPersonaje.Tomando
        );


        Debug.Log(
            "[TOMA] Comando ejecutado correctamente."
        );
    }


    // ============================================================
    // CAMBIAR ESTADO
    // ============================================================

    private void CambiarEstado(
        EstadoPersonaje nuevoEstado
    )
    {
        estadoActual =
            nuevoEstado;


        Debug.Log(
            "[ESTADO EXPLORADOR] Nuevo estado: " +
            estadoActual
        );


        if (headController == null)
        {
            Debug.LogWarning(
                "[MOVIMIENTO] PlayerHeadController no está asignado."
            );

            return;
        }


        switch (nuevoEstado)
        {
            // ====================================================
            // AVANZA
            // ====================================================

            case EstadoPersonaje.Caminando:

                headController
                    .IniciarMovimiento(3f);

                break;


            // ====================================================
            // TROTA
            // ====================================================

            case EstadoPersonaje.Corriendo:

                headController
                    .IniciarMovimiento(6f);

                break;


            // ====================================================
            // ALTO
            // ====================================================

            case EstadoPersonaje.Quieto:

                headController
                    .DetenerMovimiento();

                break;
        }
    }
}