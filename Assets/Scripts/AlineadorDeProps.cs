using UnityEngine;

public class AlineadorDeProps : MonoBehaviour
{
    [ContextMenu("Alinear Hijos al Terreno")]
    public void AlinearHijosAlTerreno()
    {
        Terrain terreno = Terrain.activeTerrain;
        if (terreno == null)
        {
            Debug.LogError("No se encontró ningún terreno activo en la escena.");
            return;
        }

        int contador = 0;
        foreach (Transform hijo in transform)
        {
            Vector3 pos = hijo.position;
            float ySuelo = terreno.SampleHeight(pos) + terreno.transform.position.y;
            hijo.position = new Vector3(pos.x, ySuelo, pos.z);
            contador++;
        }

        Debug.Log($"¡Listo! Se asentaron {contador} props al terreno correctamente.");
    }
}
