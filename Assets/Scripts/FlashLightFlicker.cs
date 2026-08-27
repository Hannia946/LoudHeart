using UnityEngine;

public class FlashLightFlicker : MonoBehaviour
{
    [SerializeField] private Light flashlight;

    public void PrenderLinterna()
    {
        if (flashlight != null)
            flashlight.enabled = true;
    }

    public void ApagarLinterna()
    {
        if (flashlight != null)
            flashlight.enabled = false;
    }

    public void ToggleLinterna()
    {
        if (flashlight != null)
            flashlight.enabled = !flashlight.enabled;
    }

    [ContextMenu("Prender Linterna")]
    public void TestPrender()
    {
        PrenderLinterna();
    }

    [ContextMenu("Apagar Linterna")]
    public void TestApagar()
    {
        ApagarLinterna();
    }
}