using UnityEngine;

// Al ser una clase abstracta, Unity nos permite arrastrarla al Inspector
public abstract class ProveedorHeadTracking : MonoBehaviour
{
    // Cualquier sistema que uses (OpenTrack, OpenCV, etc.) DEBERÁ entregar estos dos datos
    public abstract float ObtenerYaw();
    public abstract float ObtenerPitch();
}
