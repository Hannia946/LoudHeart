using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHeadController : MonoBehaviour
{
    [Header("Conexión de Datos")]
    public OpenTrackReceptor proveedorDatos;

    [Header("Configuración del Personaje")]
    public Transform cuelloJugador; // Objeto Cuello
    [Range(1f, 30f)] public float velocidadSuavizado = 15f;

    [Header("Sensibilidad de la Cabeza")]
    [Tooltip("Aumenta este valor si la cámara gira muy poco en el juego")]
    public float sensibilidadYaw = 4f;   // Multiplicador horizontal
    public float sensibilidadPitch = 3f; // Multiplicador vertical

    // Variables de Calibración
    private float offsetYaw = 0f;
    private float offsetPitch = 0f;
    public bool estaCalibrado = false;

    [ContextMenu("Calibrar Centro Manual")]
    public void CalibrarCentro()
    {
        if (proveedorDatos != null)
        {
            offsetYaw = proveedorDatos.rawYaw;
            offsetPitch = proveedorDatos.rawPitch;
            estaCalibrado = true;
            Debug.Log("¡Calibración exitosa! Ejes centrados.");
        }
    }

    void Update()
    {
        // Calibrar al presionar la barra espaciadora
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CalibrarCentro();
        }

        // Si no se ha calibrado, no hay proveedor o falta asignar el cuello, no hacemos nada
        if (!estaCalibrado || proveedorDatos == null || cuelloJugador == null) return;

        // 1. Calculamos la diferencia respecto al centro y la multiplicamos por la sensibilidad
        float yawFinal = (proveedorDatos.rawYaw - offsetYaw) * sensibilidadYaw;
        float pitchFinal = (proveedorDatos.rawPitch - offsetPitch) * sensibilidadPitch;

        // 2. Calculamos la rotación objetivo
        Quaternion rotacionObjetivo = Quaternion.Euler(-pitchFinal, yawFinal, 0f);

        // 3. Aplicamos la rotación suave al cuello
        cuelloJugador.localRotation = Quaternion.Slerp(cuelloJugador.localRotation, rotacionObjetivo, Time.deltaTime * velocidadSuavizado);
    }
}