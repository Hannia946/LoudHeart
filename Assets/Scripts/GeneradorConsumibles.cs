using System.Collections.Generic;
using UnityEngine;

public class GeneradorConsumibles : MonoBehaviour
{
    [Header("Prefabs de Consumibles")]
    [Tooltip("Arrastra aquí los 6 prefabs de consumibles")]
    public GameObject[] prefabsConsumibles;

    [Header("Configuración de Generación")]
    public int cantidadPorTipo = 5;
    public Vector3 tamanoArea = new Vector3(300f, 0f, 300f);
    public LayerMask capaTerreno;

    [Header("Ajustes de Posición y Obstáculos")]
    [Tooltip("Altura sobre el suelo para evitar que queden enterrados")]
    public float offsetAltura = 0.6f;

    [Tooltip("Distancia mínima en metros entre consumibles para evitar que se amontonen")]
    public float radioLibre = 8.0f;

    private List<Vector3> posicionesGeneradas = new List<Vector3>();

    void Start()
    {
        GenerarConsumibles();
    }

    public void GenerarConsumibles()
    {
        if (prefabsConsumibles == null || prefabsConsumibles.Length == 0)
        {
            Debug.LogError("Error: No hay prefabs asignados en GeneradorConsumibles.");
            return;
        }

        posicionesGeneradas.Clear();
        int totalGenerados = 0;

        Terrain terreno = Terrain.activeTerrain;
        Vector3 origenBase = terreno != null ? terreno.transform.position : transform.position;

        foreach (GameObject prefab in prefabsConsumibles)
        {
            if (prefab == null) continue;

            int colocadosDeEsteTipo = 0;
            int intentos = 0;

            // Insiste hasta colocar exactamente los 5 de cada tipo
            while (colocadosDeEsteTipo < cantidadPorTipo && intentos < 250)
            {
                intentos++;
                Vector3 puntoValido = ObtenerPuntoEnSueloDespejado(origenBase, terreno);

                if (puntoValido != Vector3.zero)
                {
                    posicionesGeneradas.Add(puntoValido);
                    Instantiate(prefab, puntoValido, Quaternion.identity, transform);
                    colocadosDeEsteTipo++;
                    totalGenerados++;
                }
            }
        }

        Debug.Log($"¡Se han generado {totalGenerados} consumibles correctamente en el mapa!");
    }

    Vector3 ObtenerPuntoEnSueloDespejado(Vector3 origenBase, Terrain terreno)
    {
        float x = Random.Range(origenBase.x + 15f, origenBase.x + tamanoArea.x - 15f);
        float z = Random.Range(origenBase.z + 15f, origenBase.z + tamanoArea.z - 15f);

        float ySuelo = 0f;

        if (terreno != null)
        {
            // Mide la elevación exacta de la tierra en ese punto X, Z
            ySuelo = terreno.SampleHeight(new Vector3(x, 0f, z)) + terreno.transform.position.y;
        }
        else
        {
            Vector3 origenRayo = new Vector3(x, origenBase.y + 200f, z);
            if (Physics.Raycast(origenRayo, Vector3.down, out RaycastHit hit, 500f, capaTerreno))
            {
                ySuelo = hit.point.y;
            }
            else
            {
                return Vector3.zero;
            }
        }

        Vector3 puntoCandidato = new Vector3(x, ySuelo + offsetAltura, z);

        // Verifica que no quede a menos de 'radioLibre' metros de otro consumible
        foreach (Vector3 posExistente in posicionesGeneradas)
        {
            if (Vector3.Distance(puntoCandidato, posExistente) < radioLibre)
            {
                return Vector3.zero;
            }
        }

        return puntoCandidato;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, tamanoArea);
    }
}