using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PantallaHistoriaController : MonoBehaviour
{
    [Header("Componentes de Texto")]
    public TMP_Text textoHistoria;
    public TMP_Text textoMision;
    public TMP_Text textoComandos;
    public TMP_Text textoObjetivo;

    [Header("Marcos / Recuadros")]
    public GameObject marcoComandos;
    public GameObject marcoObjetivo;

    [Header("Botones")]
    public Button botonSaltar;
    public Button botonVolverAver;
    public Button botonComenzar;

    [Header("Configuración de Tiempo")]
    public float velocidadEscritura = 0.03f;
    public float pausaEntreSecciones = 0.3f;

    // Textos narrativos (misionFull ahora tiene saltos dobles entre ítems para mayor espacio)
    private string historiaFull = "Tu perrito se perdió en el bosque.\nSolo puedes verlo a través de una cámara.\n\nGuíalo con tu voz: cada comando puede salvarle la vida.";
    private string misionFull = "<color=#00FF00>MISIÓN</color>\n\n<color=#FFFF00>PILAS</color> mantienen encendida su linterna.\n\n<color=#FF0000>VENDAS</color> le devuelven la vida.\n\n<color=#FFA500>COMIDA</color> le da fuerzas para seguir.\n\n<color=#00FF00>VIDA</color> no puede llegar a cero.";
    private string comandosFull = "<color=#00FF00>COMANDOS DE VOZ</color>\n\n<size=120%><color=#00FF00>AVANZA</color></size>\nCamina hacia adelante.\n\n<size=120%><color=#00FF00>ALTO</color></size>\nSe detiene y se queda quieto.\n\n<size=120%><color=#00FF00>TROTA</color></size>\nCorre para escapar del monstruo.";
    private string objetivoFull = "<color=#00FF00>OBJETIVO</color>   Encuentra los 5 juguetes y huye.";

    private Coroutine corrutinaAnimacion;
    private bool estaEscribiendo = false;

    void OnEnable()
    {
        IniciarSecuencia();
    }

    public void IniciarSecuencia()
    {
        if (corrutinaAnimacion != null) StopCoroutine(corrutinaAnimacion);
        corrutinaAnimacion = StartCoroutine(EscribirPantalla());
    }

    IEnumerator EscribirPantalla()
    {
        estaEscribiendo = true;

        // Estado inicial de botones
        if (botonSaltar != null) botonSaltar.gameObject.SetActive(true);
        if (botonVolverAver != null) botonVolverAver.gameObject.SetActive(false);
        if (botonComenzar != null) botonComenzar.gameObject.SetActive(false);

        // Ocultar marcos al inicio
        if (marcoComandos != null) marcoComandos.SetActive(false);
        if (marcoObjetivo != null) marcoObjetivo.SetActive(false);

        // Ocultar textos
        OcultarTexto(textoHistoria, historiaFull);
        OcultarTexto(textoMision, misionFull);
        OcultarTexto(textoComandos, comandosFull);
        OcultarTexto(textoObjetivo, objetivoFull);

        // Esperar a que el Canvas termine de inicializarse en el motor sin requerir clics
        yield return new WaitForEndOfFrame();

        // 1. Escribir Historia
        if (textoHistoria != null) yield return StartCoroutine(EscribirTexto(textoHistoria));
        yield return new WaitForSeconds(pausaEntreSecciones);

        // 2. Escribir Misión
        if (textoMision != null) yield return StartCoroutine(EscribirTexto(textoMision));
        yield return new WaitForSeconds(pausaEntreSecciones);

        // 3. Mostrar Marco de Comandos y escribir texto
        if (marcoComandos != null) marcoComandos.SetActive(true);
        yield return new WaitForSeconds(0.15f);
        if (textoComandos != null) yield return StartCoroutine(EscribirTexto(textoComandos));
        yield return new WaitForSeconds(pausaEntreSecciones);

        // 4. Mostrar Marco de Objetivo y escribir texto
        if (marcoObjetivo != null) marcoObjetivo.SetActive(true);
        yield return new WaitForSeconds(0.15f);
        if (textoObjetivo != null) yield return StartCoroutine(EscribirTexto(textoObjetivo));

        FinalizarEscritura();
    }

    private void OcultarTexto(TMP_Text componenteTexto, string contenido)
    {
        if (componenteTexto == null) return;
        componenteTexto.text = contenido;
        componenteTexto.maxVisibleCharacters = 0;
    }

    IEnumerator EscribirTexto(TMP_Text componenteTexto)
    {
        if (componenteTexto == null) yield break;

        // Forzar la actualización del Canvas y de la malla del texto
        Canvas.ForceUpdateCanvases();
        componenteTexto.ForceMeshUpdate();

        int totalCaracteres = componenteTexto.textInfo.characterCount;

        for (int i = 0; i <= totalCaracteres; i++)
        {
            componenteTexto.maxVisibleCharacters = i;
            yield return new WaitForSeconds(velocidadEscritura);
        }
    }

    public void SaltarAnimacion()
    {
        if (!estaEscribiendo) return;

        StopAllCoroutines();

        if (marcoComandos != null) marcoComandos.SetActive(true);
        if (marcoObjetivo != null) marcoObjetivo.SetActive(true);

        MostrarTodoElTexto(textoHistoria, historiaFull);
        MostrarTodoElTexto(textoMision, misionFull);
        MostrarTodoElTexto(textoComandos, comandosFull);
        MostrarTodoElTexto(textoObjetivo, objetivoFull);

        FinalizarEscritura();
    }

    private void MostrarTodoElTexto(TMP_Text componenteTexto, string contenido)
    {
        if (componenteTexto == null) return;
        componenteTexto.text = contenido;
        componenteTexto.maxVisibleCharacters = 99999;
    }

    private void FinalizarEscritura()
    {
        estaEscribiendo = false;

        if (botonSaltar != null) botonSaltar.gameObject.SetActive(false);
        if (botonVolverAver != null) botonVolverAver.gameObject.SetActive(true);
        if (botonComenzar != null) botonComenzar.gameObject.SetActive(true);
    }
}