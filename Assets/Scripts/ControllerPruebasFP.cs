using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ControllerPruebasFP : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidadMovimiento = 5.0f;
    public float gravedad = -9.81f;

    [Header("Cámara y Vista")]
    public Transform camaraJugador;
    public float sensibilidadRaton = 150.0f;

    private CharacterController characterController;
    private float rotacionX = 0f;
    private Vector3 velocidadY;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Si no se asignó la cámara manualmente, busca la cámara principal
        if (camaraJugador == null && Camera.main != null)
        {
            camaraJugador = Camera.main.transform;
        }

        // Ocultar y bloquear el ratón al centro para explorar cómodamente
        BloquearCursor(true);
    }

    void Update()
    {
        ManejarVista();
        ManejarMovimiento();

        // Presionar ESC para liberar el ratón si necesitan interactuar con el editor o menús
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BloquearCursor(false);
        }

        // Hacer clic de nuevo en la pantalla para volver a capturar el ratón
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            BloquearCursor(true);
        }
    }

    void ManejarVista()
    {
        if (camaraJugador == null) return;

        float mouseX = Input.GetAxis("Mouse X") * sensibilidadRaton * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadRaton * Time.deltaTime;

        // Rotación vertical (Arriba / Abajo)
        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -80f, 80f); // Limitar para no voltear la cabeza completo
        camaraJugador.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);

        // Rotación horizontal (Girar cuerpo a la izquierda / derecha)
        transform.Rotate(Vector3.up * mouseX);
    }

    void ManejarMovimiento()
    {
        float moveX = Input.GetAxis("Horizontal"); // A/D o Flechas Izq/Der
        float moveZ = Input.GetAxis("Vertical");   // W/S o Flechas Arriba/Abajo

        // Movimiento relativo a la dirección hacia donde mira el personaje
        Vector3 movimiento = transform.right * moveX + transform.forward * moveZ;
        characterController.Move(movimiento * velocidadMovimiento * Time.deltaTime);

        // Gravedad para mantenerse pegado al terreno del bosque
        if (characterController.isGrounded && velocidadY.y < 0)
        {
            velocidadY.y = -2f;
        }

        velocidadY.y += gravedad * Time.deltaTime;
        characterController.Move(velocidadY * Time.deltaTime);
    }

    void BloquearCursor(bool bloquear)
    {
        if (bloquear)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    // Detectar si el jugador entra en el área de un consumible/ítem
    private void OnTriggerEnter(Collider other)
    {
        // Si el objeto tiene un Tag de consumible o ítem, lo destruye al tocarlo
        if (other.CompareTag("Consumible") || other.gameObject.name.Contains("Consumible") || other.gameObject.name.Contains("Item"))
        {
            Destroy(other.gameObject);
        }
    }
}