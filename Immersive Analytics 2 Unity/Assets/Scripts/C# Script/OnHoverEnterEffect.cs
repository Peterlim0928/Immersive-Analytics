using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class OnHoverEnterEffect : MonoBehaviour
{
    private Material _originalMaterial;
    public Material highlightMaterial;
    
    // Start is called before the first frame update
    void Start()
    {
        _originalMaterial = GetComponent<Renderer>().material;

        var interactable = GetComponent<XRSimpleInteractable>();
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
    }
    
    // This method will be called when you start hovering
    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        // Change the object's material to the highlight material
        GetComponent<Renderer>().material = highlightMaterial;
    }

    // This method will be called when you stop hovering
    public void OnHoverExit(HoverExitEventArgs args)
    {
        // Revert the object's material to the original one
        GetComponent<Renderer>().material = _originalMaterial;
    }
}
