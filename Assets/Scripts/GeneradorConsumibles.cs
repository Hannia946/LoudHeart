using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GeneradorConsumibles : NetworkBehaviour
{
    [Header("Prefabs de Consumibles (con NetworkObject)")]
    public GameObject[] prefabsConsumibles;

    [Header("Configuración de Generación")]
    public int cantidadPorTipo = 5;
    public Vector3 tamanoArea = new Vector3(300f, 0f, 300f);
    public LayerMask capaTerreno;

    [Header("Ajustes de Posición y Obstáculos")]
    public float offsetAltura = 0.6f;
    public float radioLibre = 8.0f;
    [Tooltip("Radio de la esfera que detecta casas, rocas y otros objetos")]
    public float radioObstaculo = 1.5f;

    private readonly List<Vector3> posicionesGeneradas = new List<Vector3>();
    private readonly Collider[] buffer = new Collider[16];
    private bool yaGenerado = false;

    private void Start()
    {
        // Prueba sin red (Play directo en la escena): genera en local
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            GenerarConsumibles();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;   // los clientes reciben los objetos ya creados
        GenerarConsumibles();
    }

    public void GenerarConsumibles()
    {
        if (yaGenerado) return;

        if (prefabsConsumibles == null || prefabsConsumibles.Length == 0)
        {
            Debug.LogError("Error: No hay prefabs asignados en GeneradorConsumibles.");
            return;
        }

        var nm = NetworkManager.Singleton;
        bool hayRed = nm != null && nm.IsListening && nm.IsServer;

        posicionesGeneradas.Clear();
        int total = 0;

        Terrain terreno = Terrain.activeTerrain;
        Vector3 origenBase = terreno != null ? terreno.transform.position : transform.position;

        foreach (GameObject prefab in prefabsConsumibles)
        {
            if (prefab == null) continue;

            int colocados = 0;
            int intentos = 0;

            while (colocados < cantidadPorTipo && intentos < 250)
            {
                intentos++;
                Vector3 punto = ObtenerPuntoDespejado(origenBase, terreno);
                if (punto == Vector3.zero) continue;

                posicionesGeneradas.Add(punto);
                GameObject objeto = Instantiate(prefab, punto, Quaternion.identity);   // sin padre

                if (hayRed)
                {
                    var net = objeto.GetComponent<NetworkObject>();
                    if (net != null) net.Spawn(true);
                    else Debug.LogWarning($"[GENERADOR] '{prefab.name}' no tiene NetworkObject.");
                }

                colocados++;
                total++;
            }

            if (colocados < cantidadPorTipo)
                Debug.LogWarning($"[GENERADOR] Solo se colocaron {colocados}/{cantidadPorTipo} de '{prefab.name}'.");
        }

        yaGenerado = true;
        Debug.Log($"[GENERADOR] Se generaron {total} consumibles.");
    }

    private Vector3 ObtenerPuntoDespejado(Vector3 origenBase, Terrain terreno)
    {
        float x = Random.Range(origenBase.x + 15f, origenBase.x + tamanoArea.x - 15f);
        float z = Random.Range(origenBase.z + 15f, origenBase.z + tamanoArea.z - 15f);

        float ySuelo;
        if (terreno != null)
        {
            ySuelo = terreno.SampleHeight(new Vector3(x, 0f, z)) + terreno.transform.position.y;
        }
        else
        {
            Vector3 origenRayo = new Vector3(x, origenBase.y + 200f, z);
            if (!Physics.Raycast(origenRayo, Vector3.down, out RaycastHit hit, 500f, capaTerreno))
                return Vector3.zero;
            ySuelo = hit.point.y;
        }

        Vector3 candidato = new Vector3(x, ySuelo + offsetAltura, z);

        // Separación mínima entre consumibles
        foreach (Vector3 existente in posicionesGeneradas)
            if (Vector3.Distance(candidato, existente) < radioLibre) return Vector3.zero;

        // Obstáculos (casas, rocas...): esfera apoyada un poco sobre el suelo
        Vector3 centro = new Vector3(x, ySuelo + radioObstaculo + 0.3f, z);
        int n = Physics.OverlapSphereNonAlloc(centro, radioObstaculo, buffer, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
            if (!(buffer[i] is TerrainCollider)) return Vector3.zero;

        return candidato;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, tamanoArea);
    }
}