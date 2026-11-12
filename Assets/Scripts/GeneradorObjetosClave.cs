using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GeneradorObjetosClave : NetworkBehaviour
{
    [Header("Objetos Clave (Prefabs con NetworkObject)")]
    public GameObject[] objetosClave;

    [Header("Puntos de Spawn Predefinidos")]
    [Tooltip("Arrastra aquí el GameObject contenedor '20puntosSpawn'")]
    public Transform contenedorPuntosSpawn;

    [Header("Ajustes de Posición")]
    public float offsetAltura = 0.5f;
    public LayerMask capaTerreno;

    private List<Transform> listaPuntos = new List<Transform>();
    private bool yaGenerado = false;

    private void Start()
    {
        // Si no se ha iniciado sesión en red, aparece en local (Modo Play)
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            AparecerObjetosClave();
        }
    }

    public override void OnNetworkSpawn()
    {
        // En partida en red, solo ejecuta en el Servidor / Host
        if (!IsServer || yaGenerado) return;

        AparecerObjetosClave();
    }

    public void AparecerObjetosClave()
    {
        if (yaGenerado) return;

        // Carga los 20 puntos hijos automáticamente si se asignó el contenedor
        if (contenedorPuntosSpawn != null && listaPuntos.Count == 0)
        {
            foreach (Transform hijo in contenedorPuntosSpawn)
            {
                listaPuntos.Add(hijo);
            }
        }

        if (objetosClave == null || objetosClave.Length == 0 || listaPuntos.Count == 0)
        {
            Debug.LogError("Error: Asegúrate de tener prefabs asignados y el contenedor '20puntosSpawn' vinculado.");
            return;
        }

        // Copia de la lista para ir eliminando puntos ya usados (evita duplicados)
        List<Transform> disponibles = new List<Transform>(listaPuntos);
        int totalGenerados = 0;

        for (int i = 0; i < objetosClave.Length; i++)
        {
            if (objetosClave[i] == null || disponibles.Count == 0) continue;

            // Selecciona un punto al azar de los 20
            int indice = Random.Range(0, disponibles.Count);
            Transform puntoElegido = disponibles[indice];
            disponibles.RemoveAt(indice);

            // Ajusta la altura exacta al nivel del suelo
            Vector3 posicionFinal = CalcularAlturaSuelo(puntoElegido.position);

            GameObject objeto = Instantiate(objetosClave[i], posicionFinal, Quaternion.identity);

            // Sincronización en red
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && IsServer)
            {
                NetworkObject netObj = objeto.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn(true);
                }
            }

            totalGenerados++;
        }

        yaGenerado = true;
        Debug.Log($"¡Se han generado {totalGenerados}/{objetosClave.Length} objetos clave en los puntos de spawn!");
    }

    private Vector3 CalcularAlturaSuelo(Vector3 origen)
    {
        // Intenta ajustar la altura exacta lanzando un Raycast hacia abajo
        Vector3 rayoOrigen = new Vector3(origen.x, origen.y + 50f, origen.z);
        if (Physics.Raycast(rayoOrigen, Vector3.down, out RaycastHit hit, 100f, capaTerreno))
        {
            return hit.point + Vector3.up * offsetAltura;
        }

        // Ajuste con el Terrain activo como alternativa
        if (Terrain.activeTerrain != null)
        {
            float altura = Terrain.activeTerrain.SampleHeight(origen) + Terrain.activeTerrain.transform.position.y;
            return new Vector3(origen.x, altura + offsetAltura, origen.z);
        }

        return origen + Vector3.up * offsetAltura;
    }
}