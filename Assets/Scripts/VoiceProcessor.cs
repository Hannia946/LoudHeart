using UnityEngine;
using Unity.InferenceEngine;

public class VoiceProcessor : MonoBehaviour
{
    [Header("Configuración de IA")]
    public ModelAsset modelAsset; // Arrastra aquí tu archivo .onnx desde Assets/ONNX
    private Model runtimeModel;
    private Worker engine;

    [Header("Configuración de Micrófono")]
    public int sampleRate = 16000;
    private AudioClip micClip;
    private string micDevice;

    void Start()
    {
        // 1. Cargar el modelo ONNX en el motor de InferenceEngine
        if (modelAsset != null)
        {
            runtimeModel = ModelLoader.Load(modelAsset);
            engine = new Worker(runtimeModel, BackendType.GPUCompute);
            Debug.Log("🤖 Modelo ONNX cargado correctamente.");
        }
        else
        {
            Debug.LogError("⚠️ Falta asignar el ModelAsset (.onnx) en el Inspector.");
        }

        // 2. Detectar e iniciar el micrófono predeterminado del PC
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
            micClip = Microphone.Start(micDevice, true, 1, sampleRate);
            Debug.Log("🎙️ Micrófono iniciado: " + micDevice);
        }
        else
        {
            Debug.LogError("⚠️ No se detectó ningún micrófono conectado al PC.");
        }
    }

    private void OnDestroy()
    {
        // Liberar la memoria de la GPU al cerrar el juego
        engine?.Dispose();
    }
}