using UnityEngine;

public class PlayerHeadController : MonoBehaviour
{
    // ============================================================
    // CONEXIÓN DE DATOS
    // ============================================================

    [Header("Conexión de Datos")]
    public ProveedorHeadTracking proveedorDatos;


    // ============================================================
    // REFERENCIAS DEL PERSONAJE
    // ============================================================

    [Header("Configuración del Personaje")]
    public Transform cuerpoJugador;
    public Transform cuelloJugador;

    [Range(1f, 30f)]
    public float velocidadSuavizado = 15f;


    // ============================================================
    // SENSIBILIDAD
    // ============================================================

    [Header("Sensibilidad de la Cabeza")]
    public float sensibilidadYaw = 4f;
    public float sensibilidadPitch = 3f;


    // ============================================================
    // MECÁNICAS DE MOVIMIENTO
    // ============================================================

    [Header("Movimiento y Giro")]

    public float zonaMuertaParado = 30f;
    public float zonaMuertaMoviendo = 3f;

    public float multiplicadorGiroCuerpo = 3f;


    // ============================================================
    // ESTADO INTERNO
    // ============================================================

    private bool estaMoviendose = false;

    private float velocidadMovimientoActual = 0f;

    private float offsetYaw = 0f;
    private float offsetPitch = 0f;

    public bool estaCalibrado = false;


    // ============================================================
    // CALIBRAR CENTRO
    // ============================================================

    [ContextMenu("Calibrar Centro")]
    public void CalibrarCentro()
    {
        if (proveedorDatos == null)
        {
            Debug.LogWarning(
                "[HEADTRACKING] No hay proveedor de datos asignado."
            );

            return;
        }

        offsetYaw =
            proveedorDatos.ObtenerYaw();

        offsetPitch =
            proveedorDatos.ObtenerPitch();

        estaCalibrado = true;

        Debug.Log(
            "[HEADTRACKING] Centro calibrado correctamente."
        );
    }


    // ============================================================
    // INICIAR MOVIMIENTO
    // ============================================================

    public void IniciarMovimiento(float velocidad)
    {
        if (cuerpoJugador == null)
        {
            Debug.LogWarning(
                "[MOVIMIENTO] No se ha asignado cuerpoJugador."
            );

            return;
        }

        estaMoviendose = true;

        velocidadMovimientoActual =
            velocidad;

        Debug.Log(
            "[MOVIMIENTO] Movimiento iniciado. Velocidad: " +
            velocidadMovimientoActual
        );
    }


    // ============================================================
    // DETENER MOVIMIENTO
    // ============================================================

    public void DetenerMovimiento()
    {
        estaMoviendose = false;

        velocidadMovimientoActual = 0f;

        Debug.Log(
            "[MOVIMIENTO] Jugador detenido."
        );
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        // ========================================================
        // MOVIMIENTO DEL CUERPO
        // ========================================================
        //
        // IMPORTANTE:
        // El movimiento NO depende de que el headtracking
        // ya esté calibrado.
        //
        // Esto permite comprobar AVANZA, TROTA y ALTO
        // independientemente del sistema de cabeza.
        // ========================================================

        if (
            estaMoviendose &&
            cuerpoJugador != null
        )
        {
            cuerpoJugador.Translate(
                Vector3.forward *
                velocidadMovimientoActual *
                Time.deltaTime,
                Space.Self
            );
        }


        // ========================================================
        // HEADTRACKING
        // ========================================================

        if (
            !estaCalibrado ||
            proveedorDatos == null ||
            cuelloJugador == null ||
            cuerpoJugador == null
        )
        {
            return;
        }


        // ========================================================
        // OBTENER DATOS
        // ========================================================

        float yawActual =
            proveedorDatos.ObtenerYaw();

        float pitchActual =
            proveedorDatos.ObtenerPitch();


        // ========================================================
        // APLICAR OFFSET Y SENSIBILIDAD
        // ========================================================

        float yawFinal =
            (yawActual - offsetYaw) *
            sensibilidadYaw;

        float pitchFinal =
            (pitchActual - offsetPitch) *
            sensibilidadPitch;


        // ========================================================
        // ROTACIÓN DE CABEZA / CÁMARA
        // ========================================================

        Quaternion rotacionObjetivo =
            Quaternion.Euler(
                pitchFinal,
                yawFinal,
                0f
            );


        cuelloJugador.localRotation =
            Quaternion.Slerp(
                cuelloJugador.localRotation,
                rotacionObjetivo,
                Time.deltaTime *
                velocidadSuavizado
            );


        // ========================================================
        // ZONA MUERTA
        // ========================================================

        float zonaMuertaActual =
            estaMoviendose
                ? zonaMuertaMoviendo
                : zonaMuertaParado;


        // ========================================================
        // GIRO DEL CUERPO
        // ========================================================

        if (
            Mathf.Abs(yawFinal) >
            zonaMuertaActual
        )
        {
            float diferencial =
                Mathf.Abs(yawFinal) -
                zonaMuertaActual;


            float direccion =
                Mathf.Sign(yawFinal);


            float cantidadGiroCuerpo =
                diferencial *
                multiplicadorGiroCuerpo *
                direccion *
                Time.deltaTime;


            cuerpoJugador.Rotate(
                0f,
                cantidadGiroCuerpo,
                0f
            );
        }
    }
}