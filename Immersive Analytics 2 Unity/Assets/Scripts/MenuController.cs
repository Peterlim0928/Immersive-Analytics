using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    // Whether the menu is shown
    public bool isMenuShown;

    // The prefab to create
    public GameObject prefab;

    public GameObject companyInfoCanvas;

    public Button moveButton;
    public Button deleteButton;

    private bool _isMoveMode;
    private bool _isDeleteMode;
    private Color _activeButtonColour = new Color(46 / 255f, 114 / 255f, 255 / 255f, 255 / 255f);
    private Color _inactiveButtonColour = new Color(111 / 255f, 111 / 255f, 111 / 255f, 111 / 255f);

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
            // Calculate the spawn position directly in front of the user
            Vector3 spawnPosition = spawnLocation.position + spawnLocation.forward * 2f; // 2 units in front of the user
            spawnPosition.y = spawnLocation.position.y; // Maintain the same y-level as the user

            // Ensure the panel is upright: Use the camera's forward direction but set x and z rotation to 0
            Quaternion spawnRotation = Quaternion.Euler(0, spawnLocation.eulerAngles.y, 0);

            // Instantiate the new graph at the calculated position and rotation
            GameObject newGraph = Instantiate(prefab, spawnPosition, spawnRotation);

            // Find the button and attach the listener
            Button searchButton = newGraph.transform.Find("SearchCanvas").Find("Image").Find("StockSearchButton").GetComponent<Button>();
            searchButton.onClick.AddListener(() => SearchButtonOnClick(newGraph));
        }
    }

    public void SearchButtonOnClick(GameObject graphObject)
    {
        graphObject.GetComponent<CandlestickController>().ReadStockOptions();
        companyInfoCanvas.GetComponent<CompanyInfoManager>().PopulateCompanyInfo(graphObject.GetComponent<CandlestickController>().stockCodeInputField.text);
    }

    public void OnMoveButtonPressed()
    {
        _isMoveMode = !_isMoveMode;
        _isDeleteMode = false;

        moveButton.GetComponent<Image>().color = _isMoveMode ? _activeButtonColour : _inactiveButtonColour;
        // deleteButton.GetComponent<Image>().color = _inactiveButtonColour;

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

        moveButton.GetComponent<Image>().color = _inactiveButtonColour;
        deleteButton.GetComponent<Image>().color = _isDeleteMode ? _activeButtonColour : _inactiveButtonColour;

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
