using UnityEngine;

public class EstadoVitalExplorador : MonoBehaviour
{
    // ============================================================
    // VIDA
    // ============================================================

    [Header("VIDA")]

    [Min(1f)]
    public float vidaMaxima = 100f;

    [Range(0f, 100f)]
    public float vidaActual = 60f;

    [Min(0f)]
    public float curacionPorVenda = 30f;


    // ============================================================
    // ENERGÍA
    // ============================================================

    [Header("ENERGÍA")]

    [Min(1f)]
    public float energiaMaxima = 100f;

    [Range(0f, 100f)]
    public float energiaActual = 50f;

    [Min(0f)]
    public float energiaPorComida = 25f;


    // ============================================================
    // BATERÍA
    // ============================================================

    [Header("BATERÍA DE LA LINTERNA")]

    [Min(1f)]
    public float bateriaMaxima = 100f;

    [Range(0f, 100f)]
    public float bateriaActual = 40f;

    [Min(0f)]
    public float recargaPorPila = 40f;


    // ============================================================
    // LINTERNA REAL
    // ============================================================

    [Header("LINTERNA")]

    public Light luzLinterna;

    [Min(0f)]
    public float consumoBateriaPorSegundo = 2f;


    // ============================================================
    // INICIALIZACIÓN
    // ============================================================

    private void Awake()
    {
        vidaActual =
            Mathf.Clamp(
                vidaActual,
                0f,
                vidaMaxima
            );

        energiaActual =
            Mathf.Clamp(
                energiaActual,
                0f,
                energiaMaxima
            );

        bateriaActual =
            Mathf.Clamp(
                bateriaActual,
                0f,
                bateriaMaxima
            );
    }


    // ============================================================
    // CONSUMO DE BATERÍA
    // ============================================================

    private void Update()
    {
        if (
            luzLinterna == null ||
            !luzLinterna.enabled
        )
        {
            return;
        }

        if (bateriaActual > 0f)
        {
            bateriaActual -=
                consumoBateriaPorSegundo *
                Time.deltaTime;

            bateriaActual =
                Mathf.Max(
                    bateriaActual,
                    0f
                );
        }

        if (bateriaActual <= 0f)
        {
            bateriaActual = 0f;

            luzLinterna.enabled = false;

            Debug.LogWarning(
                "[LINTERNA] Batería agotada."
            );
        }
    }


    // ============================================================
    // SANA
    // ============================================================

    public bool PuedeSanar()
    {
        return vidaActual < vidaMaxima;
    }


    public void Sanar()
    {
        float vidaAnterior =
            vidaActual;

        vidaActual =
            Mathf.Min(
                vidaActual + curacionPorVenda,
                vidaMaxima
            );

        Debug.Log(
            "[VIDA] " +
            vidaAnterior.ToString("F0") +
            " → " +
            vidaActual.ToString("F0") +
            " / " +
            vidaMaxima.ToString("F0")
        );
    }


    // ============================================================
    // COME
    // ============================================================

    public bool PuedeComer()
    {
        return energiaActual < energiaMaxima;
    }


    public void Comer()
    {
        float energiaAnterior =
            energiaActual;

        energiaActual =
            Mathf.Min(
                energiaActual + energiaPorComida,
                energiaMaxima
            );

        Debug.Log(
            "[ENERGÍA] " +
            energiaAnterior.ToString("F0") +
            " → " +
            energiaActual.ToString("F0") +
            " / " +
            energiaMaxima.ToString("F0")
        );
    }


    // ============================================================
    // CARGA
    // ============================================================

    public bool PuedeRecargar()
    {
        return bateriaActual < bateriaMaxima;
    }


    public void RecargarBateria()
    {
        float bateriaAnterior =
            bateriaActual;

        bateriaActual =
            Mathf.Min(
                bateriaActual + recargaPorPila,
                bateriaMaxima
            );

        Debug.Log(
            "[BATERÍA] " +
            bateriaAnterior.ToString("F0") +
            " → " +
            bateriaActual.ToString("F0") +
            " / " +
            bateriaMaxima.ToString("F0")
        );
    }
}