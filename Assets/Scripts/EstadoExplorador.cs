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


        switch (
            comando
            .ToLower()
            .Trim()
        )
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

                CambiarEstado(
                    EstadoPersonaje.Comiendo
                );

                break;


            case "sana":

                CambiarEstado(
                    EstadoPersonaje.Curando
                );

                break;


            case "carga":

                CambiarEstado(
                    EstadoPersonaje.Cargando
                );

                break;


            case "toma":

                CambiarEstado(
                    EstadoPersonaje.Tomando
                );

                break;
        }
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

        // Aquí después conectaremos Animator,
        // movimiento, acciones, etc.
    }
}