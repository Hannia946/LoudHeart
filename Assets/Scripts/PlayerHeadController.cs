using UnityEngine;

public class PlayerHeadController : MonoBehaviour
{
    [Header("Conexión de Datos (Modular)")]
    
    public ProveedorHeadTracking proveedorDatos;

    [Header("Configuración del Personaje")]
    public Transform cuerpoJugador;
    public Transform cuelloJugador;
    [Range(1f, 30f)] public float velocidadSuavizado = 15f;

    [Header("Sensibilidad de la Cabeza")]
    public float sensibilidadYaw = 4f;
    public float sensibilidadPitch = 3f;

    [Header("Mecánicas del GDD")]
    private bool estaMoviendose = false; // no es pública para evitar errores, se controla por voz
    public float zonaMuertaParado = 30f;
    public float zonaMuertaMoviendo = 3f;
    public float multiplicadorGiroCuerpo = 3f;

    private float offsetYaw = 0f;
    private float offsetPitch = 0f;
    public bool estaCalibrado = false;

    // MÉTODOS PARA EL RECONOCIMIENTO DE VOZ

    [ContextMenu("Calibrar Centro")]
    public void CalibrarCentro()
    {
        if (proveedorDatos != null)
        {
            offsetYaw = proveedorDatos.ObtenerYaw();
            offsetPitch = proveedorDatos.ObtenerPitch();
            estaCalibrado = true;
            Debug.Log("Calibración exitosa por comando.");
        }
    }

    // El script de Sentis llamará a esto cuando el jugador diga "Va" o "Corre"
    public void IniciarMovimiento()
    {
        estaMoviendose = true;
    }

    // El script de Sentis llamará a esto cuando el jugador diga "Alto"
    public void DetenerMovimiento()
    {
        estaMoviendose = false;
    }

    // LÓGICA DE ROTACIÓN

    void Update()
    {
        // Si no se ha calibrado (ej. al inicio del juego) o faltan referencias, salimos
        if (!estaCalibrado || proveedorDatos == null || cuelloJugador == null || cuerpoJugador == null) return;

        // 1. Obtenemos datos de la interfaz, sin importar si es OpenTrack u OpenCV
        float yawActual = proveedorDatos.ObtenerYaw();
        float yawFinal = (yawActual - offsetYaw) * sensibilidadYaw;

        // 2. Rotación del Cuello (bloqueando el eje X)
        Quaternion rotacionObjetivo = Quaternion.Euler(0f, yawFinal, 0f);
        cuelloJugador.localRotation = Quaternion.Slerp(cuelloJugador.localRotation, rotacionObjetivo, Time.deltaTime * velocidadSuavizado);

        // 3. Lógica de Zonas Muertas
        float zonaMuertaActual = estaMoviendose ? zonaMuertaMoviendo : zonaMuertaParado;

        if (Mathf.Abs(yawFinal) > zonaMuertaActual)
        {
            float diferencial = Mathf.Abs(yawFinal) - zonaMuertaActual;
            float direccion = Mathf.Sign(yawFinal);
            float cantidadGiroCuerpo = diferencial * multiplicadorGiroCuerpo * direccion * Time.deltaTime;

            cuerpoJugador.Rotate(0f, cantidadGiroCuerpo, 0f);
        }
    }
}