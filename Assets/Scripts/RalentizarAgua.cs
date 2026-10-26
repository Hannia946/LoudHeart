using UnityEngine;

public class RalentizarAgua : MonoBehaviour
{
    [Header("Ajustes de Densidad")]
    [Tooltip("Resistencia que genera el agua. Mientras más alto, más pesado/lento se siente (ejemplo: 5 o 8)")]
    public float resistenciaAgua = 6f;

    private float resistenciaOriginal = 0f;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el jugador entra al agua
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Guardamos la resistencia original que tenía fuera del agua
                resistenciaOriginal = rb.linearDamping;

                // Aplicamos la resistencia del agua (pesadez)
                rb.linearDamping = resistenciaAgua;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Al salir del agua restaura la física normal
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearDamping = resistenciaOriginal;
            }
        }
    }
}