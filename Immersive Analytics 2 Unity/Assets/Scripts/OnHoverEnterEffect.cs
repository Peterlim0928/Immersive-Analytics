using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class OnHoverEnterEffect : MonoBehaviour
{
    private Material _originalMaterial;
    public Material highlightMaterial;
    
    // Start is called before the first frame update
    void Start()
    {
        _originalMaterial = GetComponent<Renderer>().material;

        var interactable = GetComponent<XRGrabInteractable>();
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
    }

    // Update is called once per frame
    void Update()
    {
        
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
