using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CargaMenu : MonoBehaviour
{
    [Header("Referencias UI")]
    public Slider barraDeCarga;
    public TMP_Text textoPorcentaje;

    [Header("Configuración de Carga")]
    [Tooltip("Tiempo en segundos que tarda en llenarse la barra")]
    public float tiempoDeCarga = 4.0f;

    [Tooltip("Nombre exacto de la escena del juego")]
    public string escenaJuego = "Bosque_Greybox";

    private void Start()
    {
        // Inicia la simulación de carga automáticamente al empezar
        StartCoroutine(SimularCarga());
    }

    private IEnumerator SimularCarga()
    {
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoDeCarga)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = Mathf.Clamp01(tiempoTranscurrido / tiempoDeCarga);

            // Actualizar el Slider
            if (barraDeCarga != null)
            {
                barraDeCarga.value = progreso;
            }

            // Actualizar el Texto
            if (textoPorcentaje != null)
            {
                textoPorcentaje.text = $"INICIALIZANDO SISTEMA DE VOZ... {Mathf.RoundToInt(progreso * 100)}%";
            }

            yield return null; // Espera al siguiente frame
        }

        // Al llegar a 100%, carga la escena de juego
        SceneManager.LoadScene(escenaJuego);
    }
}
