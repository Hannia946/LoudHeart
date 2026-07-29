using UnityEngine;

public class PlayerHeadController : MonoBehaviour
{
    [Header("Conexión de Datos")]
    // Referencia al script proveedor. Si cambias a OpenCV, solo cambias el tipo de esta variable.
    public OpenTrackReceptor proveedorDatos;

    [Header("Configuración del Personaje")]
    public Transform cuelloJugador; // El objeto que rotará
    public float velocidadSuavizado = 10f;

    // Variables de Calibración
    private float offsetYaw = 0f;
    private float offsetPitch = 0f;
    public bool estaCalibrado = false;

    //MÉTODO QUE SE CONECTA A UN BOTÓN DE LA UI
    public void CalibrarCentro()
    {
        if (proveedorDatos != null)
        {
            offsetYaw = proveedorDatos.rawYaw;
            offsetPitch = proveedorDatos.rawPitch;
            estaCalibrado = true;
            Debug.Log("¡Calibración exitosa!.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!estaCalibrado || proveedorDatos == null) return;

        // 1. Restamos el offset para encontrar el centro real del jugador
        float yawFinal = proveedorDatos.rawYaw - offsetYaw;
        float pitchFinal = proveedorDatos.rawPitch - offsetPitch;

        // 2. Calculamos la rotación objetivo (Pitch es negativo para no invertir arriba/abajo)
        Quaternion rotacionObjetivo = Quaternion.Euler(-pitchFinal, yawFinal, 0f);

        // 3. Suavizamos el movimiento de la cámara
        cuelloJugador.localRotation = Quaternion.Slerp(cuelloJugador.localRotation, rotacionObjetivo, Time.deltaTime * velocidadSuavizado);

    }
}
