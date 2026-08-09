using System.Collections.Generic;
using UnityEngine;

public class GeneradorObjetosClave : MonoBehaviour
{
    [Header("Objetos Clave (Prefabs)")]
    [Tooltip("Arrastrar aquí los 5 objetos que el jugador debe encontrar")]
    public GameObject[] objetosClave;

    [Header("Posibles Locaciones")]
    [Tooltip("Arrastra aquí los 20 puntos vacíos del mapa")]
    public Transform[] puntosDeAparicion;

    void Start()
    {
        // La aparición ocurre justo al iniciar la partida
        AparecerObjetosAlAzar();
    }

    public void AparecerObjetosAlAzar()
    {
        // Verificaciones de seguridad
        if (objetosClave.Length != 5)
        {
            Debug.LogError("Error: asignar exactamente 5 objetos clave en el Inspector.");
            return;
        }

        if (puntosDeAparicion.Length < 5)
        {
            Debug.LogError("Error: se necesitas al menos 5 puntos de aparición preestablecidos.");
            return;
        }

        // 1. creamos una lista con los números del 0 al 19 (representando los 20 puntos)
        List<int> indicesDisponibles = new List<int>();
        for (int i = 0; i < puntosDeAparicion.Length; i++)
        {
            indicesDisponibles.Add(i);
        }

        // 2. Iteramos por cada uno de los 5 objetos clave
        for (int i = 0; i < objetosClave.Length; i++)
        {
            // Elegimos un índice al azar de los que aún están disponibles
            int indiceAleatorio = Random.Range(0, indicesDisponibles.Count);

            // Obtenemos el número del punto de aparición real
            int puntoElegido = indicesDisponibles[indiceAleatorio];

            // 3. Hacemos aparecer (instanciar) el objeto en ese punto
            Instantiate(objetosClave[i], puntosDeAparicion[puntoElegido].position, puntosDeAparicion[puntoElegido].rotation);

            // 4. Eliminamos este punto de la lista para que ningún otro objeto lo use
            indicesDisponibles.RemoveAt(indiceAleatorio);
        }

        Debug.Log("¡Los 5 objetos clave han aparecido en locaciones únicas!");
    }
}
