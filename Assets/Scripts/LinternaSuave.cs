using UnityEngine;

public class LinternaSuave : MonoBehaviour
{
    [Header("Objetivo a Seguir")]
    [Tooltip("Arrastrar aquí la CamaraOjos o el Cuello")]
    public Transform objetivoCamara;

    [Header("Ajustes de Suavizado")]
    [Tooltip("Velocidad con la que la linterna alcanza a la cámara. Un valor menor (ej. 5-8) da un efecto de peso más realista.")]
    public float velocidadSuavizado = 8f;

    void Update()
    {
        if (objetivoCamara == null) return;

        // 1. Copiamos la posición de la cámara instantáneamente (para que la luz no se desfase del cuerpo)
        transform.position = objetivoCamara.position;

        // 2. Interpolamos suavemente la rotación hacia la dirección de la cámara
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            objetivoCamara.rotation,
            Time.deltaTime * velocidadSuavizado
        );
    }
}
