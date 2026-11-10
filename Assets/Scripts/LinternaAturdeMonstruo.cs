using Unity.Netcode;
using UnityEngine;

public class LinternaAturdeMonstruo : NetworkBehaviour
{
    [SerializeField] private EstadoVitalExplorador vital;   // de él toma la luz de la linterna
    [SerializeField] private float alcance = 18f;
    [SerializeField] private float anguloMaximo = 25f;      // semiángulo del haz
    [SerializeField] private float costoBateria = 5f;
    [SerializeField] private float enfriamiento = 6f;       // igual o mayor que la inmunidad del monstruo
    [SerializeField] private LayerMask obstaculos = ~0;     // lo que tapa la luz: árboles, paredes...

    private MonstruoIA monstruo;
    private float proximaRevision;
    private float proximoUso;

    private void Update()
    {
        if (!IsSpawned || !IsOwner || vital == null) return;

        Light luz = vital.luzLinterna;
        if (luz == null || !luz.enabled) return;

        if (Time.time < proximaRevision || Time.time < proximoUso) return;
        proximaRevision = Time.time + 0.15f;

        if (monstruo == null)
        {
            monstruo = FindAnyObjectByType<MonstruoIA>();
            if (monstruo == null) return;
        }

        Vector3 origen = luz.transform.position;
        Vector3 haciaMonstruo = (monstruo.transform.position + Vector3.up * 1.1f) - origen;
        float distancia = haciaMonstruo.magnitude;

        if (distancia > alcance) return;
        if (Vector3.Angle(luz.transform.forward, haciaMonstruo) > anguloMaximo) return;

        // ¿Algo tapa la luz?
        if (Physics.Raycast(origen, haciaMonstruo.normalized, out RaycastHit hit, distancia,
                            obstaculos, QueryTriggerInteraction.Ignore))
        {
            if (!hit.transform.IsChildOf(monstruo.transform)) return;
        }

        proximoUso = Time.time + enfriamiento;
        vital.bateriaActual = Mathf.Max(0f, vital.bateriaActual - costoBateria);
        monstruo.RecibirLuzServerRpc();
    }
}
