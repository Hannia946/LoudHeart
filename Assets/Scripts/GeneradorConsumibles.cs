using System.Collections.Generic;
using UnityEngine;

public class GeneradorConsumibles : MonoBehaviour
{
    [Header("Prefabs de Consumibles")]
    [Tooltip("Arrastra aquí los 6 prefabs de consumibles")]
    public GameObject[] prefabsConsumibles;

    [Header("Configuración de Generación")]
    public int cantidadPorTipo = 10;
    public Vector3 tamanoArea = new Vector3(300f, 0f, 300f);
    public LayerMask capaTerreno;

    [Header("Ajustes de Posición y Obstáculos")]
    [Tooltip("Altura sobre el suelo para evitar que queden enterrados")]
    public float offsetAltura = 0.5f;

    [Tooltip("Radio libre alrededor del ítem para evitar pinos y objetos")]
    public float radioLibre = 1.0f;

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

        int totalGenerados = 0;

        foreach (GameObject prefab in prefabsConsumibles)
        {
            if (prefab == null) continue;

            for (int i = 0; i < cantidadPorTipo; i++)
            {
                Vector3 puntoValido = ObtenerPuntoEnSueloDespejado();

                if (puntoValido != Vector3.zero)
                {
                    Instantiate(prefab, puntoValido + Vector3.up * offsetAltura, Quaternion.identity, transform);
                    totalGenerados++;
                }
                else
                {
                    Debug.LogWarning($"No se encontró una posición despejada para instanciar {prefab.name}");
                }
            }
        }

        Debug.Log($"¡Se han generado {totalGenerados} consumibles correctamente en el mapa!");
    }

    Vector3 ObtenerPuntoEnSueloDespejado()
    {
        Vector3 centro = transform.position;

        for (int intento = 0; intento < 100; intento++)
        {
            float x = Random.Range(centro.x - tamanoArea.x / 2f, centro.x + tamanoArea.x / 2f);
            float z = Random.Range(centro.z - tamanoArea.z / 2f, centro.z + tamanoArea.z / 2f);

            Vector3 origenRayo = new Vector3(x, centro.y + 150f, z);

            if (Physics.Raycast(origenRayo, Vector3.down, out RaycastHit hit, 500f, capaTerreno))
            {
                // Revisa si hay árboles/pinos en el radio de 1 metro
                Collider[] colisiones = Physics.OverlapSphere(hit.point + Vector3.up * 0.5f, radioLibre);

                bool hayObstaculo = false;
                foreach (Collider col in colisiones)
                {
                    // Si colisiona con algo que no sea la capa del terreno, se considera obstáculo
                    if (((1 << col.gameObject.layer) & capaTerreno) == 0)
                    {
                        hayObstaculo = true;
                        break;
                    }
                }

                if (!hayObstaculo)
                {
                    return hit.point;
                }
            }
        }

        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, tamanoArea);
    }
}