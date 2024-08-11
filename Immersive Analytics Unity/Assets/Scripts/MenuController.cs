using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;


public class MenuController : MonoBehaviour
{
    // Whether the menu is shown
    public bool isMenuShown = false;

    // The prefab to create
    public GameObject visualisationPrefab;

    private bool isMoveMode = false;
    private bool isDeleteMode = false;

    private void Start()
    {
        gameObject.SetActive(isMenuShown); 
    }

    public void ToggleMenu()
    {
        isMenuShown = !isMenuShown;
        gameObject.SetActive(isMenuShown);
    }

    public void OnCreateButtonPressed()
    {
        Transform spawnLocation = Camera.main.transform;
        Instantiate(visualisationPrefab, spawnLocation.position, spawnLocation.rotation);
    }

    public void OnMoveButtonPressed()
    {
        isMoveMode = !isMoveMode;
        isDeleteMode = false;

        GameObject[] moveableObjects = GameObject.FindGameObjectsWithTag("Moveable");

        if (isMoveMode)
        {
            // Enable the move mode
            Debug.Log("Move mode enabled");

            foreach (GameObject moveableObject in moveableObjects)
            {
                var grabInteractable = moveableObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                if (grabInteractable == null)
                {
                    grabInteractable = moveableObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                }
                grabInteractable.throwOnDetach = false;
            }
        }
        else
        {
            // Disable the move mode
            Debug.Log("Move mode disabled");

            foreach (GameObject moveableObject in moveableObjects)
            {
                var grabInteractable = moveableObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                Destroy(grabInteractable);
            }
        }
    }

    public void OnDeleteButtonPressed()
    {
        isMoveMode = false;
        isDeleteMode = !isDeleteMode;

        if (isDeleteMode)
        {
            // Enable the delete mode
            Debug.Log("Delete mode enabled");
        }
        else
        {
            // Disable the delete mode
            Debug.Log("Delete mode disabled");
        }
    }
}
