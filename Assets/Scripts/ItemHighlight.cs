using UnityEngine;

public class ItemHighlight : MonoBehaviour
{
    [Header("Configuración de Brillo")]
    [ColorUsage(true, true)]
    public Color highlightColor = Color.yellow; // Color dorado/amarillo para objetos clave
    public float pulseSpeed = 2.0f;
    public float minIntensity = 0.2f;
    public float maxIntensity = 2.0f;

    private Material[] itemMaterials;
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        // Obtiene los renderers del objeto y de todos sus hijos
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        itemMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            itemMaterials[i] = renderers[i].material;
            itemMaterials[i].EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        if (itemMaterials == null || itemMaterials.Length == 0) return;

        // Pulso de brillo continuo
        float pingPong = Mathf.PingPong(Time.time * pulseSpeed, 1.0f);
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, pingPong);
        Color finalColor = highlightColor * intensity;

        foreach (var mat in itemMaterials)
        {
            if (mat != null)
            {
                mat.SetColor(EmissionColorID, finalColor);
            }
        }
    }
}