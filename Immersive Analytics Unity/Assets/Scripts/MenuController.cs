using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MenuController : MonoBehaviour
{
    // Whether the menu is shown
    public bool isMenuShown = false;

    // The prefab to create
    public GameObject visualisationPrefab;

    private void Start()
    {
        gameObject.SetActive(isMenuShown); 
    }

    public void ToggleMenu()
    {
        isMenuShown = !isMenuShown;
        gameObject.SetActive(isMenuShown);
    }

    public void CreateObject()
    {
        Transform spawnLocation = Camera.main.transform;
        Instantiate(visualisationPrefab, spawnLocation.position, spawnLocation.rotation);
    }
}
