using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHoverEnterEffect : MonoBehaviour
{
    private Material _originalMaterial;
    public Material highlightMaterial;
    
    // Start is called before the first frame update
    void Start()
    {
        _originalMaterial = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // This method will be called when you start hovering
    public void OnHoverEnter()
    {
        // Change the object's material to the highlight material
        GetComponent<Renderer>().material = highlightMaterial;
    }

    // This method will be called when you stop hovering
    public void OnHoverExit()
    {
        // Revert the object's material to the original one
        GetComponent<Renderer>().material = _originalMaterial;
    }
}
