using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

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

    // Textos narrativos
    private string historiaFull = "Tu perrito se perdió en el bosque.\nSolo puedes verlo a través de una cámara.\n\nGuíalo con tu voz: cada comando puede salvarle la vida.";
    private string misionFull = "<color=#00FF00>MISIÓN</color>\n\n<color=#FFFF00>PILAS</color> mantienen encendida su linterna.\n\n<color=#FF0000>VENDAS</color> le devuelven la vida.\n\n<color=#00FF00>VIDA</color> no puede llegar a cero.";
    private string comandosFull = "<color=#00FF00>COMANDOS DE VOZ</color>\n\n<size=120%><color=#00FF00>AVANZA</color></size>\nCamina hacia adelante.\n\n<size=120%><color=#00FF00>ALTO</color></size>\nSe detiene y se queda quieto.\n\n<size=120%><color=#00FF00>TROTA</color></size>\nCorre para escapar del monstruo.";
    private string objetivoFull = "<color=#00FF00>OBJETIVO</color>   Encuentra los 5 juguetes y huye.";

    private TMP_Text etiquetaComenzar;
    private string textoOriginalComenzar = "";
    private bool esperandoCompanero = false;

    private void Awake()
    {
        PrepararUIYRaycasts();

        if (botonComenzar != null)
        {
            etiquetaComenzar = botonComenzar.GetComponentInChildren<TMP_Text>(true);
            if (etiquetaComenzar != null) textoOriginalComenzar = etiquetaComenzar.text;
        }

        ConectarBotonesSiFaltan();
    }

    private void OnEnable()
    {
        IniciarSecuencia();
        VoiceProcessor.OnComandoDetectado += AlComandoDeVoz;
    }

    private void OnDisable()
    {
        VoiceProcessor.OnComandoDetectado -= AlComandoDeVoz;
    }

    private void AlComandoDeVoz(string comando)
    {
        if (string.IsNullOrWhiteSpace(comando)) return;

        if (comando.ToLower().Trim() == "avanza" && botonComenzar != null
            && botonComenzar.gameObject.activeInHierarchy && botonComenzar.interactable)
        {
            Comenzar();
        }
    }

    private void Update()
    {
        if (esperandoCompanero && GestorPartida.Instancia != null && GestorPartida.Instancia.EnCurso)
        {
            esperandoCompanero = false;
            gameObject.SetActive(false);   // empezó la partida: cierra la pantalla
            return;
        }

        // Evita que Jugador_Pruebas bloquee el mouse mientras esta pantalla está abierta
        if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void PrepararUIYRaycasts()
    {
        // 1. Asegurar GraphicRaycaster en el Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        // 2. Asegurar que exista un EventSystem en la escena
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        // 3. Desactivar Raycast Target en imágenes decorativas y textos para que no tapen los botones
        Image[] imagenes = GetComponentsInChildren<Image>(true);
        foreach (Image img in imagenes)
        {
            if (img.GetComponent<Button>() == null)
            {
                img.raycastTarget = false;
            }
        }

        TMP_Text[] textos = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text txt in textos)
        {
            if (txt.GetComponentInParent<Button>() == null)
            {
                txt.raycastTarget = false;
            }
        }
    }

    private void ConectarBotonesSiFaltan()
    {
        // Solo conecta por código si no está asignado ya en el Inspector (evita doble clic)
        if (botonSaltar != null && botonSaltar.onClick.GetPersistentEventCount() == 0)
        {
            botonSaltar.onClick.RemoveAllListeners();
            botonSaltar.onClick.AddListener(SaltarAnimacion);
        }

        if (botonVolverAver != null && botonVolverAver.onClick.GetPersistentEventCount() == 0)
        {
            botonVolverAver.onClick.RemoveAllListeners();
            botonVolverAver.onClick.AddListener(VolverAVer);
        }

        if (botonComenzar != null && botonComenzar.onClick.GetPersistentEventCount() == 0)
        {
            botonComenzar.onClick.RemoveAllListeners();
            botonComenzar.onClick.AddListener(Comenzar);
        }
    }

    public void IniciarSecuencia()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StopAllCoroutines();
        StartCoroutine(EscribirPantalla());
    }

    private IEnumerator EscribirPantalla()
    {
        // 1. Estado inicial de botones
        if (botonSaltar != null) botonSaltar.gameObject.SetActive(true);
        if (botonVolverAver != null) botonVolverAver.gameObject.SetActive(false);
        if (botonComenzar != null) botonComenzar.gameObject.SetActive(false);
        if (botonComenzar != null) botonComenzar.interactable = true;
        if (etiquetaComenzar != null) etiquetaComenzar.text = textoOriginalComenzar;

        // 2. Estado inicial de marcos
        if (marcoComandos != null) marcoComandos.SetActive(false);
        if (marcoObjetivo != null) marcoObjetivo.SetActive(false);

        // 3. Limpiar textos al inicio
        LimpiarTexto(textoHistoria);
        LimpiarTexto(textoMision);
        LimpiarTexto(textoComandos);
        LimpiarTexto(textoObjetivo);

        // Esperar un frame normal (no depende de que haya una cámara renderizando)
        yield return null;

        // 4. Secuencia de escritura
        if (textoHistoria != null) yield return StartCoroutine(EscribirTexto(textoHistoria, historiaFull));
        yield return EsperarSegundosReales(pausaEntreSecciones);

        if (textoMision != null) yield return StartCoroutine(EscribirTexto(textoMision, misionFull));
        yield return EsperarSegundosReales(pausaEntreSecciones);

        if (marcoComandos != null) marcoComandos.SetActive(true);
        yield return EsperarSegundosReales(0.15f);
        if (textoComandos != null) yield return StartCoroutine(EscribirTexto(textoComandos, comandosFull));
        yield return EsperarSegundosReales(pausaEntreSecciones);

        if (marcoObjetivo != null) marcoObjetivo.SetActive(true);
        yield return EsperarSegundosReales(0.15f);
        if (textoObjetivo != null) yield return StartCoroutine(EscribirTexto(textoObjetivo, objetivoFull));

        FinalizarEscritura();
    }

    private IEnumerator EsperarSegundosReales(float segundos)
    {
        float transcurrido = 0f;
        while (transcurrido < segundos)
        {
            transcurrido += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void LimpiarTexto(TMP_Text comp)
    {
        if (comp == null) return;
        comp.text = "";
        comp.maxVisibleCharacters = 0;
    }

    private IEnumerator EscribirTexto(TMP_Text comp, string contenido)
    {
        if (comp == null) yield break;

        comp.text = contenido;
        comp.maxVisibleCharacters = 0;

        yield return null;
        comp.ForceMeshUpdate();

        int totalCaracteres = comp.textInfo.characterCount;

        if (totalCaracteres <= 0)
        {
            comp.maxVisibleCharacters = 99999;
            yield break;
        }

        for (int i = 0; i <= totalCaracteres; i++)
        {
            comp.maxVisibleCharacters = i;
            yield return EsperarSegundosReales(velocidadEscritura);
        }

        comp.maxVisibleCharacters = 99999;
    }

    public void SaltarAnimacion()
    {
        StopAllCoroutines();

        if (marcoComandos != null) marcoComandos.SetActive(true);
        if (marcoObjetivo != null) marcoObjetivo.SetActive(true);

        MostrarTextoCompleto(textoHistoria, historiaFull);
        MostrarTextoCompleto(textoMision, misionFull);
        MostrarTextoCompleto(textoComandos, comandosFull);
        MostrarTextoCompleto(textoObjetivo, objetivoFull);

        FinalizarEscritura();
    }

    public void VolverAVer()
    {
        IniciarSecuencia();
    }

    private void MostrarTextoCompleto(TMP_Text comp, string contenido)
    {
        if (comp == null) return;
        comp.text = contenido;
        comp.maxVisibleCharacters = 99999;
    }

    private void FinalizarEscritura()
    {
        if (botonSaltar != null) botonSaltar.gameObject.SetActive(false);
        if (botonVolverAver != null) botonVolverAver.gameObject.SetActive(true);
        if (botonComenzar != null) botonComenzar.gameObject.SetActive(true);
    }

    public void Comenzar()
    {
        if (botonComenzar != null) botonComenzar.interactable = false;

        // Sin red (prueba en el editor): cierra directamente
        if (GestorPartida.Instancia == null)
        {
            gameObject.SetActive(false);
            return;
        }

        esperandoCompanero = true;
        if (etiquetaComenzar != null) etiquetaComenzar.text = "ESPERANDO AL COMPAÑERO...";
        GestorPartida.Instancia.JugadorListoRpc();
    }
}