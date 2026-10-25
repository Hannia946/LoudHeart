using UnityEngine;
using UnityEngine.UI;

public class DamageOverlay : MonoBehaviour
{
    [SerializeField] private Image bloodImage;
    [SerializeField] private float pulseSpeed = 2.5f;

    public void ActualizarEfectoVida(float vidaActual, float vidaMaxima)
    {
        float porcentajeVida = vidaActual / vidaMaxima;

        // Muestra el efecto si la vida cae por debajo del 35%
        if (porcentajeVida <= 0.35f)
        {
            float alphaBase = Mathf.Lerp(0.7f, 0.2f, porcentajeVida / 0.35f);
            float alphaPulsante = alphaBase + (Mathf.Sin(Time.time * pulseSpeed) * 0.15f);
            SetAlpha(Mathf.Clamp01(alphaPulsante));
        }
        else
        {
            SetAlpha(0f);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color color = bloodImage.color;
        color.a = alpha;
        bloodImage.color = color;
    }

    [ContextMenu("Probar Sangre (Baja Vida)")]
    public void TestSangreBaja()
    {
        ActualizarEfectoVida(20f, 100f);
    }

    [ContextMenu("Quitar Sangre (Vida Llena)")]
    public void TestSangreLlena()
    {
        ActualizarEfectoVida(100f, 100f);
    }
}