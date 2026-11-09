using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GeneradorObjetosClave : NetworkBehaviour
{
    [Header("Objetos Clave (Prefabs con NetworkObject)")]
    [Tooltip("Arrastra aquí los 5 prefabs de los objetos clave")]
    public GameObject[] objetosClave;

    [Header("Configuración del Área")]
    public Vector3 tamanoArea = new Vector3(300f, 0f, 300f);
    public LayerMask capaTerreno;

    [Header("Ajustes de Posición y Obstáculos")]
    [Tooltip("Altura sobre el suelo para evitar que queden enterrados")]
    public float offsetAltura = 0.5f;

    [Tooltip("Radio libre alrededor del objeto para evitar colisiones con rocas/árboles")]
    public float radioLibre = 1.2f;

    public override void OnNetworkSpawn()
    {
        // Solo el servidor o host calcula las posiciones y spawnea en red
        if (!IsServer) return;

        AparecerObjetosClave();
    }

    public void AparecerObjetosClave()
    {
        if (objetosClave == null || objetosClave.Length == 0)
        {
            Debug.LogError("Error: No hay prefabs asignados en GeneradorObjetosClave.");
            return;
        }

        int totalGenerados = 0;

        for (int i = 0; i < objetosClave.Length; i++)
        {
            if (objetosClave[i] == null) continue;

            Vector3 puntoValido = ObtenerPuntoEnSueloDespejado();

            if (puntoValido != Vector3.zero)
            {
                // Posición corregida con el offset
                Vector3 posicionFinal = puntoValido + Vector3.up * offsetAltura;

                // Instanciar en el servidor
                GameObject objeto = Instantiate(objetosClave[i], posicionFinal, Quaternion.identity);

                // Replicar a todos los clientes conectados
                NetworkObject netObj = objeto.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn(true);
                    totalGenerados++;
                }
                else
                {
                    Debug.LogError($"El prefab {objetosClave[i].name} no tiene un componente NetworkObject.");
                }
            }
            else
            {
                Debug.LogWarning($"No se encontró una posición segura en el terreno para {objetosClave[i].name}");
            }
        }

        Debug.Log($"¡Se han generado {totalGenerados}/{objetosClave.Length} objetos clave sincronizados y sobre terreno firme!");
    }

    private Vector3 ObtenerPuntoEnSueloDespejado()
    {
        Vector3 centro = transform.position;

        for (int intento = 0; intento < 100; intento++)
        {
            float x = Random.Range(centro.x - tamanoArea.x / 2f, centro.x + tamanoArea.x / 2f);
            float z = Random.Range(centro.z - tamanoArea.z / 2f, centro.z + tamanoArea.z / 2f);

            // Rayo lanzado desde arriba filtrando exclusivamente por capaTerreno
            Vector3 origenRayo = new Vector3(x, centro.y + 150f, z);

            if (Physics.Raycast(origenRayo, Vector3.down, out RaycastHit hit, 500f, capaTerreno))
            {
                // Verifica si hay obstáculos en el radio libre
                Collider[] colisiones = Physics.OverlapSphere(hit.point + Vector3.up * 0.5f, radioLibre);

                bool hayObstaculo = false;
                foreach (Collider col in colisiones)
                {
                    // Si toca algo ajeno a la capa del suelo, es obstáculo
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, tamanoArea);
    }
}