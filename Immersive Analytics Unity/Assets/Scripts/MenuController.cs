using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MenuController : MonoBehaviour
{
    // Whether the menu is shown
    public bool isMenuShown;

    // The prefab to create
    public GameObject prefab;

    private bool _isMoveMode;
    private bool _isDeleteMode;

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
        Transform spawnLocation = Camera.main?.transform;
        if (spawnLocation != null)
        {
            Instantiate(prefab, spawnLocation.position, spawnLocation.rotation);
        }
    }

    public void OnMoveButtonPressed()
    {
        _isMoveMode = !_isMoveMode;
        _isDeleteMode = false;

        GameObject[] moveableObjects = GameObject.FindGameObjectsWithTag("Moveable");

        if (_isMoveMode)
        {
            // Enable the move mode
            Debug.Log("Move mode enabled");

            foreach (GameObject moveableObject in moveableObjects)
            {
                var grabInteractable = moveableObject.GetComponent<XRGrabInteractable>();
                if (grabInteractable == null)
                {
                    grabInteractable = moveableObject.AddComponent<XRGrabInteractable>();
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
                var grabInteractable = moveableObject.GetComponent<XRGrabInteractable>();
                Destroy(grabInteractable);
            }
        }
    }

    public void OnDeleteButtonPressed()
    {
        _isMoveMode = false;
        _isDeleteMode = !_isDeleteMode;

        if (_isDeleteMode)
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
