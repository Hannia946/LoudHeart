using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GeneradorObjetosClave : NetworkBehaviour
{
    [Header("Objetos Clave (Prefabs)")]
    public GameObject[] objetosClave;

    [Header("Posibles Locaciones")]
    public Transform[] puntosDeAparicion;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;   // los clientes reciben los objetos ya creados
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
            int indiceAleatorio = Random.Range(0, indicesDisponibles.Count);
            int puntoElegido = indicesDisponibles[indiceAleatorio];
            Transform punto = puntosDeAparicion[puntoElegido];

            GameObject objeto = Instantiate(objetosClave[i], punto.position, punto.rotation);
            objeto.GetComponent<NetworkObject>().Spawn(true);   // visible para todos

            indicesDisponibles.RemoveAt(indiceAleatorio);
        }

        Debug.Log("¡Los 5 objetos clave han aparecido en locaciones únicas!");
    }
}
