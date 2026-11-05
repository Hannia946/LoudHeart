using System.Collections.Generic;
using UnityEngine;

public class GeneradorObjetosClave : MonoBehaviour
{
    [Header("Objetos Clave (Prefabs)")]
    public GameObject[] objetosClave;

    [Header("Posibles Locaciones")]
    public Transform[] puntosDeAparicion;

    [Header("Ajustes de Posición y Obstáculos")]
    public float offsetAltura = 0.7f;
    public float radioLibre = 1.2f;

    void Start()
    {
        AparecerObjetosAlAzar();
    }

    public void AparecerObjetosAlAzar()
    {
        if (objetosClave.Length != 5 || puntosDeAparicion.Length < 5) return;

        List<int> indicesDisponibles = new List<int>();
        for (int i = 0; i < puntosDeAparicion.Length; i++)
        {
            indicesDisponibles.Add(i);
        }

        for (int i = 0; i < objetosClave.Length; i++)
        {
            int indiceAleatorio = Random.Range(0, indicesDisponibles.Count);
            int puntoElegido = indicesDisponibles[indiceAleatorio];

            Transform puntoTransform = puntosDeAparicion[puntoElegido];
            Vector3 posicionFinal = ObtenerPosicionDespejada(puntoTransform.position);

            Instantiate(objetosClave[i], posicionFinal, puntoTransform.rotation);
            indicesDisponibles.RemoveAt(indiceAleatorio);
        }

        Debug.Log("¡Los 5 objetos clave han aparecido despejados de obstáculos!");
    }

    Vector3 ObtenerPosicionDespejada(Vector3 origenPunto)
    {
        Vector3 posicionBase = origenPunto;

        Vector3 origenRayo = origenPunto + Vector3.up * 50f;
        if (Physics.Raycast(origenRayo, Vector3.down, out RaycastHit hit, 100f))
        {
            posicionBase = hit.point;
        }

        Vector3 posicionAjustada = posicionBase;

        // Prueba en círculo alrededor del punto si detecta un obstáculo
        for (int intento = 0; intento < 8; intento++)
        {
            Vector3 offset = (intento == 0) ? Vector3.zero : Quaternion.Euler(0, intento * 45f, 0) * Vector3.forward * (intento * 0.8f);
            Vector3 pruebaPosicion = posicionBase + offset;

            Collider[] colisiones = Physics.OverlapSphere(pruebaPosicion + Vector3.up * 0.5f, radioLibre);
            bool hayObstaculo = false;

            foreach (Collider col in colisiones)
            {
                if (col is TerrainCollider || col.gameObject.name.ToLower().Contains("terrain")) continue;

                hayObstaculo = true;
                break;
            }

            if (!hayObstaculo)
            {
                posicionAjustada = pruebaPosicion;
                break;
            }
        }

        return posicionAjustada + Vector3.up * offsetAltura;
    }
}