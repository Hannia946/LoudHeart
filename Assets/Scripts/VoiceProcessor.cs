using System;
using UnityEngine;
using UnityEngine.Serialization;
using Unity.InferenceEngine;

public class VoiceProcessor : MonoBehaviour
{
    // ============================================================
    // LOUD HEART - VOICE PROCESSOR V13
    // ============================================================
    //
    // MODELO:
    // LoudHeart_CNN_V13.onnx
    //
    // INPUT:
    // (1, 40, 97, 1)
    //
    // CLASES:
    // 0 ALTO
    // 1 AVANZA
    // 2 CARGA
    // 3 COME
    // 4 SANA
    // 5 TOMA
    // 6 TROTA
    // 7 RUIDO
    //
    // IMPORTANTE:
    // modoPrueba = true
    //      -> NO ejecuta comandos
    //      -> muestra diagnóstico
    //
    // modoPrueba = false
    //      -> ejecuta comandos aceptados
    //
    // ============================================================


    // ============================================================
    // EVENTO PARA ESTADOEXPLORADOR
    // ============================================================

    public static event Action<string> OnComandoDetectado;


    // ============================================================
    // MODELO V13
    // ============================================================

    [Header("MODELO V13 ONNX")]

    [Tooltip("Arrastra aquí LoudHeart_CNN_V13.onnx")]
    public ModelAsset modelAsset;

    private Model runtimeModel;

    private Worker engine;


    // ============================================================
    // MICROFONO
    // ============================================================

    [Header("MICRÓFONO")]

    public int sampleRate = 16000;

    public int recordingLength = 10;

    private AudioClip microphoneClip;

    private string microphoneDevice;

    private float[] microphoneData;

    private float[] analysisBuffer;

    private bool microphoneReady = false;

    private float microphoneStartTime = 0f;


    // ============================================================
    // THRESHOLDS PARA V13 (MISMOS VALORES CONGELADOS DE V11)
    // ============================================================

    [Header("DECISIÓN V13 - THRESHOLDS CONGELADOS")]

    [Range(0f, 1f)]
    public float generalConfidenceThreshold = 0.75f;

    [Range(0f, 1f)]
    public float altoConfidenceThreshold = 0.75f;

    [Range(0f, 1f)]
    public float tomaConfidenceThreshold = 0.93f;

    [Range(0f, 1f)]
    public float minimumMargin = 0.15f;


    // ============================================================
    // INTERVALO DE ANÁLISIS
    // ============================================================

    [Header("ANÁLISIS")]

    [Tooltip("Cada cuánto se analiza el último segundo del micrófono.")]
    [FormerlySerializedAs("analysisIntervalV11")]
    [FormerlySerializedAs("analysisIntervalV12")]
    public float analysisIntervalV13 = 0.20f;

    private float nextAnalysisTime = 0f;


    // ============================================================
    // FILTRO DE SILENCIO
    //
    // NO ES UN CLASIFICADOR DE RUIDO.
    //
    // Solo evita ejecutar CNN cuando la señal es prácticamente
    // silenciosa.
    // ============================================================

    [Header("FILTRO DE SILENCIO")]

    [Range(0.0001f, 0.1f)]
    [FormerlySerializedAs("minimumRMSV11")]
    [FormerlySerializedAs("minimumRMSV12")]
    public float minimumRMSV13 = 0.015f;


    // ============================================================
    // MODO DE PRUEBA
    // ============================================================

    [Header("MODO DE PRUEBA")]

    [Tooltip(
        "ACTIVADO = NO ejecuta acciones. " +
        "Solo muestra las predicciones."
    )]
    public bool modoPrueba = true;


    [Tooltip(
        "En modo prueba muestra cada inferencia que supera el RMS."
    )]
    public bool mostrarTodosLosAnalisis = true;


    [Tooltip(
        "Muestra también las ventanas bloqueadas por RMS. " +
        "Normalmente debe permanecer desactivado."
    )]
    public bool mostrarRMSBloqueado = false;


    [Tooltip(
        "En modo normal muestra información del comando aceptado."
    )]
    public bool mostrarDiagnosticoNormal = true;


    // ============================================================
    // CONFIRMACIÓN TEMPORAL + COOLDOWN GLOBAL
    // ============================================================

    [Header("CONFIRMACIÓN TEMPORAL")]

    [Tooltip(
        "Cantidad de ventanas consecutivas que deben aceptar el mismo " +
        "comando antes de confirmarlo. Para Loud Heart usamos 2."
    )]
    [Range(1, 4)]
    public int requiredConsecutiveWindows = 2;

    [Tooltip(
        "Tiempo máximo entre ventanas consecutivas del mismo candidato. " +
        "Con Analysis Interval = 0.20, 0.50 s permite confirmar dos ventanas contiguas."
    )]
    [Range(0.2f, 1.0f)]
    public float confirmationMaxGap = 0.50f;


    [Header("CONTROL DE REPETICIÓN")]

    [Tooltip(
        "Bloqueo GLOBAL después de confirmar un comando. Evita que la cola " +
        "de una misma pronunciación active otro comando distinto. ALTO puede " +
        "romper este bloqueo por seguridad."
    )]
    public float commandCooldown = 1.0f;


    private string pendingCommand = "";

    private int pendingCommandCount = 0;

    private float lastPendingTime = -Mathf.Infinity;

    private float lastGlobalCommandTime = -Mathf.Infinity;

    // Último comando confirmado. Se usa para permitir que ALTO
    // interrumpa otro comando, pero sin confirmarse varias veces
    // durante la misma pronunciación.
    private string lastConfirmedCommand = "";


    // ============================================================
    // ORDEN OFICIAL DE CLASES V13
    //
    // NO CAMBIAR.
    // ============================================================

    private readonly string[] commandLabels =
    {
        "alto",     // 0
        "avanza",   // 1
        "carga",    // 2
        "come",     // 3
        "sana",     // 4
        "toma",     // 5
        "trota",    // 6
        "ruido"     // 7
    };


    // ============================================================
    // NORMALIZACIÓN OFICIAL V13
    //
    // NO CAMBIAR.
    // ============================================================

    private const float MEDIA_V13 = -4.718053552384031f;
    private const float STD_V13 = 2.348239325951468f;

    // ============================================================
    // LOG-MEL OFICIAL
    // ============================================================

    private const int N_FFT = 512;

    private const int WIN_LENGTH = 400;

    private const int HOP_LENGTH = 160;

    private const int N_MELS = 40;

    private const int N_FRAMES = 97;

    private const float FMIN = 20f;

    private const float FMAX = 8000f;

    private const float EPSILON = 1e-10f;


    // ============================================================
    // BUFFERS DSP
    // ============================================================

    private float[] hannWindow;

    private float[,] melFilterBank;

    private float[] fftReal;

    private float[] fftImag;

    private float[] powerSpectrum;

    private float[] tensorData;


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        Debug.Log(
            "=================================================="
        );

        Debug.Log(
            "LOUD HEART - VOICE PROCESSOR V13"
        );

        Debug.Log(
            "=================================================="
        );


        Debug.Log(
            modoPrueba
            ? "🧪 MODO PRUEBA ACTIVADO - NO SE EJECUTAN COMANDOS"
            : "🎮 MODO NORMAL - COMANDOS ACTIVOS"
        );


        Debug.Log(
            "Threshold GENERAL = " +
            generalConfidenceThreshold
        );


        Debug.Log(
            "Threshold ALTO = " +
            altoConfidenceThreshold
        );


        Debug.Log(
            "Threshold TOMA = " +
            tomaConfidenceThreshold
        );


        Debug.Log(
            "Margen mínimo = " +
            minimumMargin
        );


        Debug.Log(
            "Minimum RMS = " +
            minimumRMSV13
        );


        Debug.Log(
            "MEDIA V13 = " +
            MEDIA_V13
        );


        Debug.Log(
            "STD V13 = " +
            STD_V13
        );


        Debug.Log(
            "Confirmación temporal = " +
            requiredConsecutiveWindows +
            " ventanas consecutivas"
        );


        Debug.Log(
            "Gap máximo confirmación = " +
            confirmationMaxGap +
            " s"
        );


        Debug.Log(
            "Cooldown GLOBAL = " +
            commandCooldown +
            " s (ALTO puede interrumpirlo)"
        );


        PrepararPreprocesamiento();

        InicializarModelo();

        InicializarMicrofono();
    }


    // ============================================================
    // PREPARAR PREPROCESAMIENTO
    // ============================================================

    private void PrepararPreprocesamiento()
    {
        hannWindow =
            new float[WIN_LENGTH];


        melFilterBank =
            new float[
                N_MELS,
                N_FFT / 2 + 1
            ];


        fftReal =
            new float[N_FFT];


        fftImag =
            new float[N_FFT];


        powerSpectrum =
            new float[
                N_FFT / 2 + 1
            ];


        tensorData =
            new float[
                N_MELS *
                N_FRAMES
            ];


        CrearVentanaHann();

        CrearMelFilterBank();


        Debug.Log(
            "[V13 CHECK] Hann = " +
            hannWindow.Length
        );


        Debug.Log(
            "[V13 CHECK] MelBank = " +
            melFilterBank.GetLength(0) +
            " x " +
            melFilterBank.GetLength(1)
        );


        Debug.Log(
            "[V13 CHECK] Tensor = " +
            tensorData.Length
        );


        Debug.Log(
            "[VoiceProcessor V13] ✅ Preprocesamiento preparado."
        );
    }


    // ============================================================
    // INICIALIZAR MODELO
    // ============================================================

    private void InicializarModelo()
    {
        if (modelAsset == null)
        {
            Debug.LogError(
                "[VoiceProcessor V13] ❌ " +
                "No hay modelo asignado."
            );

            return;
        }


        try
        {
            runtimeModel =
                ModelLoader.Load(
                    modelAsset
                );


            engine =
                new Worker(
                    runtimeModel,
                    BackendType.CPU
                );


            Debug.Log(
                "[VoiceProcessor V13] ✅ Modelo cargado."
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[VoiceProcessor V13] ❌ " +
                "Error cargando el modelo:\n" +
                e
            );
        }
    }


    // ============================================================
    // INICIALIZAR MICROFONO
    // ============================================================

    private void InicializarMicrofono()
    {
        if (
            Microphone.devices == null ||
            Microphone.devices.Length == 0
        )
        {
            Debug.LogError(
                "[VoiceProcessor V13] ❌ " +
                "No se encontró ningún micrófono."
            );

            return;
        }


        microphoneDevice =
            Microphone.devices[0];


        Debug.Log(
            "[VoiceProcessor V13] Micrófono: " +
            microphoneDevice
        );


        microphoneClip =
            Microphone.Start(
                microphoneDevice,
                true,
                recordingLength,
                sampleRate
            );


        if (microphoneClip == null)
        {
            Debug.LogError(
                "[VoiceProcessor V13] ❌ " +
                "No se pudo iniciar el micrófono."
            );

            return;
        }


        microphoneData =
            new float[
                microphoneClip.samples *
                microphoneClip.channels
            ];


        analysisBuffer =
            new float[sampleRate];


        microphoneStartTime =
            Time.realtimeSinceStartup;


        microphoneReady =
            true;


        Debug.Log(
            "[VoiceProcessor V13] ✅ Micrófono iniciado."
        );
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        if (!microphoneReady)
            return;


        if (engine == null)
            return;


        // Esperar a que exista al menos ~1 segundo de audio.
        if (
            Time.realtimeSinceStartup -
            microphoneStartTime
            <
            1.1f
        )
        {
            return;
        }


        if (
            Time.time <
            nextAnalysisTime
        )
        {
            return;
        }


        nextAnalysisTime =
            Time.time +
            analysisIntervalV13;


        AnalizarMicrofono();
    }


    // ============================================================
    // ANALIZAR MICROFONO
    // ============================================================

    private void AnalizarMicrofono()
    {
        if (
            microphoneClip == null ||
            microphoneData == null ||
            analysisBuffer == null
        )
        {
            return;
        }


        int microphonePosition =
            Microphone.GetPosition(
                microphoneDevice
            );


        if (microphonePosition <= 0)
            return;


        bool lecturaCorrecta =
            microphoneClip.GetData(
                microphoneData,
                0
            );


        if (!lecturaCorrecta)
            return;


        int channels =
            microphoneClip.channels;


        if (channels <= 0)
            return;


        // ========================================================
        // EXTRAER EXACTAMENTE EL ÚLTIMO SEGUNDO
        //
        // 16000 muestras mono.
        // ========================================================

        int startPosition =
            microphonePosition -
            sampleRate;


        while (
            startPosition < 0
        )
        {
            startPosition +=
                microphoneClip.samples;
        }


        for (
            int i = 0;
            i < sampleRate;
            i++
        )
        {
            int samplePosition =
                (
                    startPosition +
                    i
                )
                %
                microphoneClip.samples;


            float sample =
                0f;


            for (
                int channel = 0;
                channel < channels;
                channel++
            )
            {
                int index =
                    samplePosition *
                    channels +
                    channel;


                if (
                    index >= 0 &&
                    index <
                    microphoneData.Length
                )
                {
                    sample +=
                        microphoneData[index];
                }
            }


            sample /=
                channels;


            analysisBuffer[i] =
                sample;
        }


        // ========================================================
        // RMS
        // ========================================================

        float rms =
            CalcularRMS(
                analysisBuffer
            );


        if (
            rms <
            minimumRMSV13
        )
        {
            // El silencio rompe cualquier candidato pendiente para que una
            // palabra vieja no pueda combinarse con otra ventana posterior.
            ResetTemporalCandidate();

            if (
                modoPrueba &&
                mostrarRMSBloqueado
            )
            {
                Debug.Log(
                    "V13 | RMS BLOQUEADO | " +
                    rms.ToString("F5")
                );
            }


            return;
        }


        // ========================================================
        // LOG-MEL V13
        // ========================================================

        float[] entrada =
            CrearLogMelNormalizado(
                analysisBuffer
            );


        if (entrada == null)
        {
            Debug.LogError(
                "[VoiceProcessor V13] " +
                "❌ No se pudo generar Log-Mel."
            );

            return;
        }


        // ========================================================
        // INFERENCIA
        // ========================================================

        EjecutarModelo(
            entrada,
            rms
        );
    }


    // ============================================================
    // RMS
    // ============================================================

    private float CalcularRMS(
        float[] audio
    )
    {
        if (
            audio == null ||
            audio.Length == 0
        )
        {
            return 0f;
        }


        double suma =
            0.0;


        for (
            int i = 0;
            i < audio.Length;
            i++
        )
        {
            double valor =
                audio[i];


            suma +=
                valor *
                valor;
        }


        return Mathf.Sqrt(
            (float)(
                suma /
                audio.Length
            )
        );
    }


    // ============================================================
    // LOG-MEL V13
    //
    // Configuración:
    //
    // SR         = 16000
    // N_FFT      = 512
    // WIN_LENGTH = 400
    // HOP        = 160
    // N_MELS     = 40
    // FMIN       = 20
    // FMAX       = 8000
    // CENTER     = false
    // POWER      = 2
    // LOG        = log10(mel + 1e-10)
    //
    // OUTPUT:
    // 40 x 97
    // ============================================================

    private float[] CrearLogMelNormalizado(
        float[] audio
    )
    {
        if (
            audio == null ||
            audio.Length != sampleRate
        )
        {
            Debug.LogError(
                "[V13 LogMel] Audio inválido. " +
                "Se esperaban 16000 muestras."
            );

            return null;
        }


        if (
            hannWindow == null ||
            melFilterBank == null ||
            fftReal == null ||
            fftImag == null ||
            powerSpectrum == null ||
            tensorData == null
        )
        {
            Debug.LogError(
                "[V13 LogMel] Buffers no inicializados."
            );

            return null;
        }


        Array.Clear(
            tensorData,
            0,
            tensorData.Length
        );


        // ========================================================
        // Librosa usa win_length=400 dentro de FFT=512.
        //
        // Ventana centrada dentro de los 512 puntos:
        //
        // (512 - 400) / 2 = 56
        // ========================================================

        int windowOffset =
            (
                N_FFT -
                WIN_LENGTH
            )
            /
            2;


        // ========================================================
        // 97 FRAMES
        // ========================================================

        for (
            int frame = 0;
            frame < N_FRAMES;
            frame++
        )
        {
            Array.Clear(
                fftReal,
                0,
                fftReal.Length
            );


            Array.Clear(
                fftImag,
                0,
                fftImag.Length
            );


            int frameStart =
                frame *
                HOP_LENGTH;


            // ====================================================
            // APLICAR HANN
            // ====================================================

            for (
                int n = 0;
                n < WIN_LENGTH;
                n++
            )
            {
                int fftIndex =
                    windowOffset +
                    n;


                int audioIndex =
                    frameStart +
                    fftIndex;


                if (
                    fftIndex < 0 ||
                    fftIndex >= N_FFT
                )
                {
                    continue;
                }


                if (
                    audioIndex < 0 ||
                    audioIndex >= audio.Length
                )
                {
                    continue;
                }


                fftReal[
                    fftIndex
                ] =
                    audio[
                        audioIndex
                    ]
                    *
                    hannWindow[n];
            }


            // ====================================================
            // FFT
            // ====================================================

            FFT(
                fftReal,
                fftImag
            );


            // ====================================================
            // POWER SPECTRUM
            // ====================================================

            int bins =
                N_FFT / 2 + 1;


            for (
                int k = 0;
                k < bins;
                k++
            )
            {
                float real =
                    fftReal[k];


                float imag =
                    fftImag[k];


                powerSpectrum[k] =
                    real *
                    real +
                    imag *
                    imag;
            }


            // ====================================================
            // MEL FILTER BANK
            // ====================================================

            for (
                int mel = 0;
                mel < N_MELS;
                mel++
            )
            {
                double energia =
                    0.0;


                for (
                    int k = 0;
                    k < bins;
                    k++
                )
                {
                    energia +=
                        melFilterBank[
                            mel,
                            k
                        ]
                        *
                        powerSpectrum[k];
                }


                float melValue =
                    Mathf.Max(
                        (float)energia,
                        0f
                    );


                // =================================================
                // LOG10 OFICIAL
                // =================================================

                float logMel =
                    Mathf.Log10(
                        melValue +
                        EPSILON
                    );


                // =================================================
                // NORMALIZACIÓN V13
                // =================================================

                float normalizado =
                    (
                        logMel -
                        MEDIA_V13
                    )
                    /
                    STD_V13;


                // =================================================
                // NHWC:
                //
                // [mel, frame]
                // =================================================

                int tensorIndex =
                    mel *
                    N_FRAMES +
                    frame;


                if (
                    tensorIndex >= 0 &&
                    tensorIndex <
                    tensorData.Length
                )
                {
                    tensorData[
                        tensorIndex
                    ] =
                        normalizado;
                }
            }
        }


        return tensorData;
    }


    // ============================================================
    // HANN PERIÓDICA
    //
    // Equivalente a scipy/librosa fftbins=True.
    // ============================================================

    private void CrearVentanaHann()
    {
        for (
            int n = 0;
            n < WIN_LENGTH;
            n++
        )
        {
            hannWindow[n] =
                0.5f
                -
                0.5f
                *
                Mathf.Cos(
                    2f *
                    Mathf.PI *
                    n /
                    WIN_LENGTH
                );
        }
    }


    // ============================================================
    // MEL FILTERBANK SLANEY
    //
    // htk = false
    // norm = "slaney"
    // ============================================================

    private void CrearMelFilterBank()
    {
        int fftBins =
            N_FFT / 2 + 1;


        float melMin =
            HzToMelSlaney(
                FMIN
            );


        float melMax =
            HzToMelSlaney(
                FMAX
            );


        float[] melPoints =
            new float[
                N_MELS + 2
            ];


        float[] hzPoints =
            new float[
                N_MELS + 2
            ];


        // ========================================================
        // PUNTOS MEL
        // ========================================================

        for (
            int i = 0;
            i < N_MELS + 2;
            i++
        )
        {
            float posicion =
                (float)i /
                (
                    N_MELS +
                    1
                );


            melPoints[i] =
                melMin
                +
                (
                    melMax -
                    melMin
                )
                *
                posicion;


            hzPoints[i] =
                MelToHzSlaney(
                    melPoints[i]
                );
        }


        // ========================================================
        // TRIÁNGULOS MEL
        // ========================================================

        for (
            int mel = 0;
            mel < N_MELS;
            mel++
        )
        {
            float left =
                hzPoints[mel];


            float center =
                hzPoints[
                    mel + 1
                ];


            float right =
                hzPoints[
                    mel + 2
                ];


            // Slaney normalization:
            //
            // enorm = 2 / (f_right - f_left)
            float ancho =
                right -
                left;


            float normalizacion =
                ancho > 0f
                ?
                2f / ancho
                :
                0f;


            for (
                int k = 0;
                k < fftBins;
                k++
            )
            {
                float frecuencia =
                    (float)k *
                    sampleRate /
                    N_FFT;


                float lower =
                    0f;


                float upper =
                    0f;


                if (
                    center >
                    left
                )
                {
                    lower =
                        (
                            frecuencia -
                            left
                        )
                        /
                        (
                            center -
                            left
                        );
                }


                if (
                    right >
                    center
                )
                {
                    upper =
                        (
                            right -
                            frecuencia
                        )
                        /
                        (
                            right -
                            center
                        );
                }


                float peso =
                    Mathf.Max(
                        0f,
                        Mathf.Min(
                            lower,
                            upper
                        )
                    );


                melFilterBank[
                    mel,
                    k
                ] =
                    peso *
                    normalizacion;
            }
        }
    }


    // ============================================================
    // HZ -> MEL SLANEY
    // ============================================================

    private float HzToMelSlaney(
        float hz
    )
    {
        const float fSp =
            200f / 3f;


        const float minLogHz =
            1000f;


        const float minLogMel =
            15f;


        float logStep =
            Mathf.Log(
                6.4f
            )
            /
            27f;


        if (
            hz <
            minLogHz
        )
        {
            return
                hz /
                fSp;
        }


        return
            minLogMel
            +
            Mathf.Log(
                hz /
                minLogHz
            )
            /
            logStep;
    }


    // ============================================================
    // MEL -> HZ SLANEY
    // ============================================================

    private float MelToHzSlaney(
        float mel
    )
    {
        const float fSp =
            200f / 3f;


        const float minLogHz =
            1000f;


        const float minLogMel =
            15f;


        float logStep =
            Mathf.Log(
                6.4f
            )
            /
            27f;


        if (
            mel <
            minLogMel
        )
        {
            return
                mel *
                fSp;
        }


        return
            minLogHz
            *
            Mathf.Exp(
                logStep
                *
                (
                    mel -
                    minLogMel
                )
            );
    }


    // ============================================================
    // FFT RADIX-2
    // ============================================================

    private void FFT(
        float[] real,
        float[] imag
    )
    {
        if (
            real == null ||
            imag == null
        )
        {
            return;
        }


        int n =
            real.Length;


        if (
            n != N_FFT ||
            imag.Length != n
        )
        {
            Debug.LogError(
                "[V13 FFT] Tamaño incorrecto."
            );

            return;
        }


        int j =
            0;


        // ========================================================
        // BIT REVERSAL
        // ========================================================

        for (
            int i = 1;
            i < n;
            i++
        )
        {
            int bit =
                n >> 1;


            while (
                (
                    j &
                    bit
                )
                != 0
            )
            {
                j ^=
                    bit;

                bit >>=
                    1;
            }


            j ^=
                bit;


            if (
                i <
                j
            )
            {
                float tempReal =
                    real[i];


                real[i] =
                    real[j];


                real[j] =
                    tempReal;


                float tempImag =
                    imag[i];


                imag[i] =
                    imag[j];


                imag[j] =
                    tempImag;
            }
        }


        // ========================================================
        // COOLEY-TUKEY
        // ========================================================

        for (
            int len = 2;
            len <= n;
            len <<= 1
        )
        {
            float angle =
                -2f *
                Mathf.PI /
                len;


            float wLenReal =
                Mathf.Cos(
                    angle
                );


            float wLenImag =
                Mathf.Sin(
                    angle
                );


            for (
                int i = 0;
                i < n;
                i += len
            )
            {
                float wReal =
                    1f;


                float wImag =
                    0f;


                int half =
                    len / 2;


                for (
                    int k = 0;
                    k < half;
                    k++
                )
                {
                    int even =
                        i +
                        k;


                    int odd =
                        i +
                        k +
                        half;


                    float oddReal =
                        real[odd] *
                        wReal
                        -
                        imag[odd] *
                        wImag;


                    float oddImag =
                        real[odd] *
                        wImag
                        +
                        imag[odd] *
                        wReal;


                    float evenReal =
                        real[even];


                    float evenImag =
                        imag[even];


                    real[even] =
                        evenReal +
                        oddReal;


                    imag[even] =
                        evenImag +
                        oddImag;


                    real[odd] =
                        evenReal -
                        oddReal;


                    imag[odd] =
                        evenImag -
                        oddImag;


                    float newWReal =
                        wReal *
                        wLenReal
                        -
                        wImag *
                        wLenImag;


                    float newWImag =
                        wReal *
                        wLenImag
                        +
                        wImag *
                        wLenReal;


                    wReal =
                        newWReal;


                    wImag =
                        newWImag;
                }
            }
        }
    }


    // ============================================================
    // EJECUTAR MODELO V13
    // ============================================================

    private void EjecutarModelo(
        float[] inputData,
        float rms
    )
    {
        if (
            inputData == null ||
            inputData.Length !=
            N_MELS *
            N_FRAMES
        )
        {
            Debug.LogError(
                "[V13] Tensor de entrada inválido."
            );

            return;
        }


        try
        {
            using Tensor<float> input =
                new Tensor<float>(

                    new TensorShape(
                        1,
                        N_MELS,
                        N_FRAMES,
                        1
                    ),

                    inputData
                );


            engine.Schedule(
                input
            );


            Tensor<float> output =
                engine.PeekOutput()
                as Tensor<float>;


            if (output == null)
            {
                Debug.LogError(
                    "[V13] Output del modelo nulo."
                );

                return;
            }


            float[] probabilities =
                output.DownloadToArray();


            if (
                probabilities == null ||
                probabilities.Length < 8
            )
            {
                Debug.LogError(
                    "[V13] Salida del modelo inválida."
                );

                return;
            }


            ProcesarPrediccion(
                probabilities,
                rms
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[VoiceProcessor V13] ❌ " +
                "Error durante inferencia:\n" +
                e
            );
        }
    }


    // ============================================================
    // PROCESAR PREDICCIÓN
    // ============================================================

    private void ProcesarPrediccion(
        float[] probabilities,
        float rms
    )
    {
        int cantidad =
            Mathf.Min(
                probabilities.Length,
                commandLabels.Length
            );


        if (cantidad < 2)
            return;


        // ========================================================
        // TOP 1 / TOP 2
        // ========================================================

        int top1 = -1;
        int top2 = -1;

        float confidence1 = float.MinValue;
        float confidence2 = float.MinValue;


        for (
            int i = 0;
            i < cantidad;
            i++
        )
        {
            float value = probabilities[i];


            if (value > confidence1)
            {
                confidence2 = confidence1;
                top2 = top1;

                confidence1 = value;
                top1 = i;
            }
            else if (value > confidence2)
            {
                confidence2 = value;
                top2 = i;
            }
        }


        if (
            top1 < 0 ||
            top2 < 0
        )
        {
            ResetTemporalCandidate();
            return;
        }


        string command1 = commandLabels[top1];
        string command2 = commandLabels[top2];

        float margin =
            confidence1 -
            confidence2;


        // ========================================================
        // THRESHOLD DE LA CLASE TOP1
        // ========================================================

        float threshold =
            generalConfidenceThreshold;


        if (command1 == "alto")
        {
            threshold =
                altoConfidenceThreshold;
        }
        else if (command1 == "toma")
        {
            threshold =
                tomaConfidenceThreshold;
        }


        // ========================================================
        // GATE INDIVIDUAL DE LA VENTANA
        // ========================================================

        string decision;
        bool passesWindowGate = false;


        if (command1 == "ruido")
        {
            decision =
                "RUIDO / SIN ACCIÓN";

            ResetTemporalCandidate();
        }
        else if (confidence1 < threshold)
        {
            decision =
                "RECHAZADO CONFIANZA";

            ResetTemporalCandidate();
        }
        else if (margin < minimumMargin)
        {
            decision =
                "RECHAZADO MARGEN";

            ResetTemporalCandidate();
        }
        else
        {
            // OJO: pasar confidence+margin ya NO significa ejecutar.
            // Primero debe confirmarse temporalmente en dos ventanas.
            decision =
                "CANDIDATO TEMPORAL";

            passesWindowGate = true;
        }


        // ========================================================
        // LOG DE CADA INFERENCIA EN MODO PRUEBA
        // ========================================================

        if (
            modoPrueba &&
            mostrarTodosLosAnalisis
        )
        {
            Debug.Log(
                "V13 PRUEBA | " +
                "Top1: " +
                command1.ToUpper() +
                " " +
                (
                    confidence1 *
                    100f
                ).ToString("F2") +
                "% | Top2: " +
                command2.ToUpper() +
                " " +
                (
                    confidence2 *
                    100f
                ).ToString("F2") +
                "% | Margen: " +
                (
                    margin *
                    100f
                ).ToString("F2") +
                "% | RMS: " +
                rms.ToString("F5") +
                " | Umbral: " +
                (
                    threshold *
                    100f
                ).ToString("F0") +
                "% | " +
                decision
            );
        }


        // Si la ventana no pasa los thresholds, aquí termina.
        if (!passesWindowGate)
            return;


        // ========================================================
        // CONFIRMACIÓN TEMPORAL
        // ========================================================

        ProcesarCandidatoTemporal(
            command1,
            confidence1,
            command2,
            confidence2,
            margin,
            rms
        );
    }


    // ============================================================
    // CONFIRMACIÓN TEMPORAL
    //
    // Requiere el mismo comando en N ventanas consecutivas.
    // Con interval=0.20 y N=2, la confirmación suele tardar ~0.2 s
    // después de la primera ventana válida.
    // ============================================================

    private void ProcesarCandidatoTemporal(
        string command,
        float confidence,
        string top2Command,
        float top2Confidence,
        float margin,
        float rms
    )
    {
        if (
            string.IsNullOrEmpty(command) ||
            command == "ruido"
        )
        {
            ResetTemporalCandidate();
            return;
        }


        float now =
            Time.realtimeSinceStartup;


        // ========================================================
        // COOLDOWN GLOBAL
        //
        // ALTO es la excepción: debe poder detener al personaje
        // aunque otro comando se haya confirmado hace poco.
        // ========================================================

        bool cooldownActivo =
            now -
            lastGlobalCommandTime
            <
            commandCooldown;


        // Durante el cooldown se bloquea cualquier comando.
        // Excepción de seguridad: ALTO puede interrumpir si el último
        // comando confirmado fue OTRO comando. Si el último también
        // fue ALTO, se bloquea para evitar doble confirmación de una
        // sola pronunciación.
        bool altoPuedeInterrumpir =
            command == "alto" &&
            lastConfirmedCommand != "alto";


        if (
            cooldownActivo &&
            !altoPuedeInterrumpir
        )
        {
            ResetTemporalCandidate();

            if (
                modoPrueba &&
                mostrarTodosLosAnalisis
            )
            {
                Debug.Log(
                    "⏳ V13 BLOQUEADO COOLDOWN GLOBAL | " +
                    command.ToUpper() +
                    " | Faltan: " +
                    Mathf.Max(
                        0f,
                        commandCooldown -
                        (
                            now -
                            lastGlobalCommandTime
                        )
                    ).ToString("F2") +
                    " s"
                );
            }

            return;
        }


        // ========================================================
        // EVITAR QUE UN CANDIDATO VIEJO SE CONSERVE DEMASIADO
        // ========================================================

        if (
            !string.IsNullOrEmpty(
                pendingCommand
            ) &&
            now -
            lastPendingTime
            >
            confirmationMaxGap
        )
        {
            ResetTemporalCandidate();
        }


        // ========================================================
        // CONTAR VENTANAS CONSECUTIVAS DEL MISMO COMANDO
        // ========================================================

        if (pendingCommand == command)
        {
            pendingCommandCount++;
        }
        else
        {
            pendingCommand = command;
            pendingCommandCount = 1;
        }


        lastPendingTime = now;


        if (
            modoPrueba &&
            mostrarTodosLosAnalisis
        )
        {
            Debug.Log(
                "🟡 V13 CANDIDATO | " +
                command.ToUpper() +
                " | Ventanas: " +
                pendingCommandCount +
                "/" +
                requiredConsecutiveWindows
            );
        }


        if (
            pendingCommandCount <
            requiredConsecutiveWindows
        )
        {
            return;
        }


        // ========================================================
        // COMANDO CONFIRMADO
        // ========================================================

        ConfirmarComando(
            command,
            confidence,
            top2Command,
            top2Confidence,
            margin,
            rms
        );
    }


    // ============================================================
    // CONFIRMAR COMANDO
    // ============================================================

    private void ConfirmarComando(
        string command,
        float confidence,
        string top2Command,
        float top2Confidence,
        float margin,
        float rms
    )
    {
        float now =
            Time.realtimeSinceStartup;


        // En cuanto se confirma una intención, comienza el bloqueo
        // global. Esto también se simula en Modo Prueba para que la
        // consola represente lo que ocurriría realmente en gameplay.
        lastGlobalCommandTime = now;
        lastConfirmedCommand = command;


        ResetTemporalCandidate();


        if (modoPrueba)
        {
            Debug.Log(
                "✅ V13 CONFIRMADO (MODO PRUEBA) | " +
                command.ToUpper() +
                " | Confianza: " +
                (
                    confidence *
                    100f
                ).ToString("F2") +
                "% | Margen: " +
                (
                    margin *
                    100f
                ).ToString("F2") +
                "% | NO SE EJECUTA ACCIÓN"
            );

            return;
        }


        EjecutarComandoConfirmado(
            command,
            confidence,
            top2Command,
            top2Confidence,
            margin,
            rms
        );
    }


    // ============================================================
    // EJECUTAR COMANDO CONFIRMADO
    // ============================================================

    private void EjecutarComandoConfirmado(
        string command,
        float confidence,
        string top2Command,
        float top2Confidence,
        float margin,
        float rms
    )
    {
        if (
            string.IsNullOrEmpty(command) ||
            command == "ruido"
        )
        {
            return;
        }


        if (mostrarDiagnosticoNormal)
        {
            Debug.Log(
                "✅ V13 ACTIVACIÓN CONFIRMADA | " +
                command.ToUpper() +
                " | Confianza: " +
                (
                    confidence *
                    100f
                ).ToString("F2") +
                "% | Top2: " +
                top2Command.ToUpper() +
                " " +
                (
                    top2Confidence *
                    100f
                ).ToString("F2") +
                "% | Margen: " +
                (
                    margin *
                    100f
                ).ToString("F2") +
                "% | RMS: " +
                rms.ToString("F5")
            );
        }


        OnComandoDetectado?.Invoke(
            command
        );
    }


    // ============================================================
    // RESET DEL CANDIDATO TEMPORAL
    // ============================================================

    private void ResetTemporalCandidate()
    {
        pendingCommand = "";
        pendingCommandCount = 0;
        lastPendingTime = -Mathf.Infinity;
    }


    // ============================================================
    // LIMPIEZA
    // ============================================================

    private void OnDestroy()
    {
        // ========================================================
        // CERRAR MICROFONO
        // ========================================================

        if (
            !string.IsNullOrEmpty(
                microphoneDevice
            )
        )
        {
            if (
                Microphone.IsRecording(
                    microphoneDevice
                )
            )
            {
                Microphone.End(
                    microphoneDevice
                );
            }
        }


        // ========================================================
        // LIBERAR WORKER
        // ========================================================

        if (
            engine != null
        )
        {
            engine.Dispose();

            engine =
                null;
        }


        runtimeModel =
            null;


        Debug.Log(
            "[VoiceProcessor V13] Recursos liberados."
        );
    }
}