using UnityEngine;
using UnityEngine.UI;

public class KeyItem : MonoBehaviour
{
    [Header("Información del Objeto Clave")]
    public string nombreObjetoClave;
    public int idObjeto;
    [SerializeField] private AudioClip sonidoRecoleccionUnico;

    [Header("Visual Feedback (Gaze Lock)")]
    [SerializeField] private Outline outlineComponent;

    private bool estaFijado = false;

    private void Start()
    {
        if (outlineComponent == null)
            outlineComponent = GetComponent<Outline>();

        DesresaltarObjeto();
    }

    public void ResaltarObjeto()
    {
        estaFijado = true;
        if (outlineComponent != null)
            outlineComponent.enabled = true;

        Debug.Log($"[Gaze Lock] Objeto {nombreObjetoClave} resaltado y listo para comando toma");
    }

    public void DesresaltarObjeto()
    {
        estaFijado = false;
        if (outlineComponent != null)
            outlineComponent.enabled = false;
    }

    public bool EstaFijado()
    {
        return estaFijado;
    }

    public void RecogerObjeto()
    {
        if (sonidoRecoleccionUnico != null)
        {
            AudioSource.PlayClipAtPoint(sonidoRecoleccionUnico, transform.position);
        }

        Destroy(gameObject);
    }
}