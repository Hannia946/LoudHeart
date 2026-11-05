using UnityEngine;

public class PuntosSpawnJugadores : MonoBehaviour
{
    [SerializeField] private Transform[] puntos;

    public Transform ObtenerPunto(ulong clientId)
    {
        if (puntos == null || puntos.Length == 0) return null;
        return puntos[(int)(clientId % (ulong)puntos.Length)];
    }
}
