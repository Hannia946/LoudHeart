using UnityEngine;

public class EstadoExplorador : MonoBehaviour
{
    // Definición de los estados posibles del Explorador
    public enum EstadoPersonaje
    {
        Quieto,      // ALTO
        Caminando,   // VA
        Corriendo,   // CORRE
        Comiendo,    // COME
        Curando,     // SANA
        Cargando     // CARGA
    }

    [Header("Estado Actual del Explorador")]
    public EstadoPersonaje estadoActual = EstadoPersonaje.Quieto;

    private void OnEnable()
    {
        // Suscribirse a los comandos de voz de la IA
        VoiceProcessor.OnComandoDetectado += ProcesarComandoVoz;
    }

    private void OnDisable()
    {
        // Desuscribirse para evitar errores de memoria
        VoiceProcessor.OnComandoDetectado -= ProcesarComandoVoz;
    }

    private void ProcesarComandoVoz(string comando)
    {
        switch (comando)
        {
            case "alto":
                CambiarEstado(EstadoPersonaje.Quieto);
                break;

            case "va":
                CambiarEstado(EstadoPersonaje.Caminando);
                break;

            case "corre":
                CambiarEstado(EstadoPersonaje.Corriendo);
                break;

            case "come":
                CambiarEstado(EstadoPersonaje.Comiendo);
                break;

            case "sana":
                CambiarEstado(EstadoPersonaje.Curando);
                break;

            case "carga":
                CambiarEstado(EstadoPersonaje.Cargando);
                break;

            default:
                Debug.LogWarning($"?? Comando '{comando}' no tiene acción asignada en los estados.");
                break;
        }
    }

    private void CambiarEstado(EstadoPersonaje nuevoEstado)
    {
        estadoActual = nuevoEstado;
        Debug.Log($"?? ESTADO CAMBIADO: El personaje ahora está en estado **{estadoActual}**");

        // Aquí se activarán las animaciones o la velocidad real cuando esté listo el personaje
    }
}