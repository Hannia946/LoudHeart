using UnityEngine;
using Unity.InferenceEngine; // 👈 Namespace para Sentis 2.6+
using UnityEngine.InputSystem;

public class VoiceProcessor : MonoBehaviour
{
    [Header("Configuración de IA (Sentis)")]
    public ModelAsset modelAsset; // Casilla para arrastrar tu modelo .onnx

    [Header("Clases de Voz (Orden alfabético estricto)")]
    public string[] clases = new string[] {
        "alto", "carga", "come", "corre", "ruido", "sana", "toma", "va"
    };

    [Header("Filtros de Precisión")]
    [Range(0.5f, 0.95f)]
    public float certezaMinima = 0.80f; // Exige al menos 80% de seguridad para validar el comando

    [Header("Configuración de Micrófono")]
    public int sampleRate = 16000;
    public float umbralVolumen = 0.05f; // Subido para ignorar voces lejanas/ambiente
    public float cooldownInferencia = 1.0f;

    private AudioClip micClip;
    private string micDevice;
    private Model runtimeModel;
    private Worker worker;
    private float tiempoUltimaPrediccion = 0f;

    void Start()
    {
        // 1. Cargar modelo en Sentis
        if (modelAsset != null)
        {
            runtimeModel = ModelLoader.Load(modelAsset);
            worker = new Worker(runtimeModel, BackendType.GPUCompute);
            Debug.Log("🤖 Modelo ONNX cargado exitosamente en Unity Sentis.");
        }
        else
        {
            Debug.LogError("❌ FALTAN DATOS: Arrastra el archivo ONNX a la casilla 'Model Asset' en el Inspector.");
        }

        // 2. Iniciar micrófono
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
            micClip = Microphone.Start(micDevice, true, 1, sampleRate);
            Debug.Log("🎙️ Escuchando micrófono: " + micDevice);
        }
        else
        {
            Debug.LogError("⚠️ No hay micrófono conectado.");
        }
    }

    void Update()
    {
        if (micClip == null) return;

        // Medir volumen del micrófono
        int micPosition = Microphone.GetPosition(micDevice) - 1280;
        if (micPosition < 0) return;

        float[] samplesLectura = new float[1280];
        micClip.GetData(samplesLectura, micPosition);

        float sum = 0f;
        for (int i = 0; i < samplesLectura.Length; i++)
        {
            sum += Mathf.Abs(samplesLectura[i]);
        }
        float averageVolume = sum / samplesLectura.Length;

        // Disparador de inferencia por voz o tecla Espacio (Nuevo Input System)
        bool detectoVoz = (averageVolume > umbralVolumen) && (Time.time - tiempoUltimaPrediccion > cooldownInferencia);
        bool presionoTecla = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (detectoVoz || presionoTecla)
        {
            tiempoUltimaPrediccion = Time.time;
            EjecutarInferenciaIA();
        }
    }

    public void EjecutarInferenciaIA()
    {
        if (worker == null || micClip == null) return;

        // Extraer audio
        float[] datosAudio = new float[sampleRate];
        micClip.GetData(datosAudio, 0);

        // Tensor<float> para la versión 2.6
        using Tensor<float> inputTensor = new Tensor<float>(new TensorShape(1, sampleRate), datosAudio);

        // Ejecutar inferencia
        worker.Schedule(inputTensor);

        // Obtener resultados
        using Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;
        float[] probabilidades = outputTensor.DownloadToArray();

        // Calcular ganador
        int indiceGanador = 0;
        float maxProb = probabilidades[0];
        for (int i = 1; i < probabilidades.Length; i++)
        {
            if (probabilidades[i] > maxProb)
            {
                maxProb = probabilidades[i];
                indiceGanador = i;
            }
        }

        string comandoDetectado = clases[indiceGanador];

        // 🛡️ FILTRO 1: Ignorar si la IA detectó "ruido"
        if (comandoDetectado.ToLower() == "ruido")
        {
            Debug.Log($"🤫 Sonido descartado: Clasificado como RUIDO ({maxProb * 100f:F1}% de probabilidad)");
            return;
        }

        // 🛡️ FILTRO 2: Ignorar si la seguridad es baja (menor al umbral de Certeza Mínima)
        if (maxProb < certezaMinima)
        {
            Debug.Log($"⚠️ Comando ignorado: '{comandoDetectado.ToUpper()}' obtuvo {maxProb * 100f:F1}% (Mínimo requerido: {certezaMinima * 100f}%)");
            return;
        }

        // ✅ COMANDO ACEPTADO Y VALIDO
        Debug.Log($"✅ COMANDO DETECTADO: **{comandoDetectado.ToUpper()}** ({maxProb * 100f:F1}% de certeza)");
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
}