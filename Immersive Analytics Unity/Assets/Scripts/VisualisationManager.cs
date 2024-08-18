using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using IATK;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class VisualisationManager : MonoBehaviour
{
    // Prefab to create
    public GameObject vrMenuPrefab;
    
    // Stock code textbox
    public TMP_InputField stockCodeInputField;
    public TMP_Dropdown stockTimeDropdown;
    
    // Visualisation object
    private CSVDataSource _dataSource;
    private Visualisation _visualisation;
    private GameObject _vrMenu; // This is game object not component

    //Real-Time Variables
    private string _stockCode;
    private string _stockTimeOption;
    private static Timer _realTimeTimer;
    public TMP_Dropdown stockRealTimeIntervalDropdown;

    private string[] _stockTimeOptionList =
    {
        "1 Day", "5 Days", "1 Month", "3 Months", "6 Months", "1 Year", "2 Years", "5 Years", "10 Years",
        "Year To Date", "All"
    };
    private string[] _stockTimeList = { "1d", "5d", "1mo", "3mo", "6mo", "1y", "2y", "5y", "10y", "ytd", "max" };
    
    private string[] _realTimeUpdateIntervalOptions =
    {
        "15 secs", "30 secs", "1 min", "5 mins", "15 mins", "30 mins", "1 hour", "3 hour", "5 hours"
    };

    private int[] _realTimeUpdateIntervalInMS =
    {
        15000, 30000, 60000, 300000, 900000, 1800000, 3600000, 10800000, 18000000
    };

    void Start()
    {
        stockTimeDropdown.AddOptions(_stockTimeOptionList.ToList());
        stockRealTimeIntervalDropdown.AddOptions(_realTimeUpdateIntervalOptions.ToList());
    }
    
    public void ReadStockOptions()
    { 
        int index = stockTimeDropdown.value; 
        string chosenTime = _stockTimeList[index];
        Debug.Log(String.Format("Stock Code: {0} | Stock Time Option: {1}", stockCodeInputField.text, chosenTime));
                
        // Run the Download Python Script
        string pythonScriptPath = "Assets/IATK/Scripts/Controller/script.py";
        string pythonArgs = $"{stockCodeInputField.text} {chosenTime}";
        
        //Adjust variable for Real-Time update
        _stockCode = stockCodeInputField.text;
        _stockTimeOption = chosenTime;
        
        RunPythonScript(pythonScriptPath, pythonArgs);
        
        UpdateGraph();
        UpdateGraphRealTime();
    }
    
    private void UpdateGraphRealTimeHelper(System.Object source, ElapsedEventArgs e)
    {
        Debug.Log(String.Format("(Real Time Update) Stock Code: {0} | Stock Time Option: {1}", _stockCode, _stockTimeOption));
        
        // Run the Download Python Script
        string pythonScriptPath = "Assets/IATK/Scripts/Controller/script.py";
        string pythonArgs = $"{_stockCode} {_stockTimeOption}";
        RunPythonScript(pythonScriptPath, pythonArgs);
        
        UpdateGraph();
    }
    
    private void UpdateGraphRealTime()
    {
        int index = stockRealTimeIntervalDropdown.value; 
        int interval = _realTimeUpdateIntervalInMS[index];
        _realTimeTimer = new Timer(interval); // Set the timer interval to 1 second (1000 milliseconds)
        _realTimeTimer.Elapsed += UpdateGraphRealTimeHelper;
        _realTimeTimer.AutoReset = true; // Restart the timer automatically after each elapsed event
        _realTimeTimer.Enabled = true; // Start the timer
    }

    public void RunPythonScript(string scriptPath, string arguments)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "python";
        start.Arguments = $"{scriptPath} {arguments}";
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.CreateNoWindow = true;

        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                Debug.Log(result);
            }
            string error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
            }
        }
    }

    public void UpdateGraph()
    {
        //Added for debug
        Debug.Log("Graph Updating");
        
        // Delete Everything
        Transform currentVisualisation = transform.Find("IATK Visualisation");
        if (currentVisualisation != null)
            Destroy(currentVisualisation.gameObject);
        Transform currentCsvSource = transform.Find("CSV Data Source");
        if (currentCsvSource != null)
            Destroy(currentCsvSource.gameObject);
        if (_vrMenu != null)
            Destroy(_vrMenu.gameObject);

        
        // Load CSV file
        string fileName = stockCodeInputField.text.ToUpper() + "-" + _stockTimeList[stockTimeDropdown.value] + ".csv";
        string filePath = Path.Combine("./Assets/IATK/Datasets/", fileName);

        if (File.Exists(filePath))
        {
            TextAsset csvFile = new TextAsset(File.ReadAllText(filePath));
            
            if (csvFile != null)
            {
                // Assign the CSV file content to the Data field of CSVDataSource
                GameObject dataSource = new GameObject("CSV Data Source");
                dataSource.AddComponent<CSVDataSource>();
                dataSource.transform.SetParent(transform);
                _dataSource = dataSource.GetComponent<CSVDataSource>();
                _dataSource.data = csvFile;
                _dataSource.load();
            }
            else
            {
                Debug.LogError("CSV file not found in Resources folder.");
            }
        }
        
        // Re-create all the game objects
        // IATK Visualisation
        GameObject visualisation = new GameObject("IATK Visualisation");
        visualisation.tag = "Moveable";
        visualisation.transform.SetParent(transform);
        visualisation.transform.rotation = transform.rotation;
        visualisation.transform.localPosition = new Vector3(-0.05f, -0.12f, 0f);
        
        // Add Components to IATK Visualisation
        BoxCollider visualisationBoxCollider = visualisation.AddComponent<BoxCollider>();
        visualisationBoxCollider.size = new Vector3(0.05f, 0.05f, 0.05f);
        Rigidbody visualisationRigidbody = visualisation.AddComponent<Rigidbody>();
        visualisationRigidbody.useGravity = false; // Disable gravity
        visualisationRigidbody.isKinematic = true; // Prevents movement caused by physics
        visualisation.AddComponent<Visualisation>();
        
        // Get the script of the component
        _visualisation = visualisation.GetComponent<Visualisation>();
        _visualisation.dataSource = _dataSource;
        _visualisation.geometry = AbstractVisualisation.GeometryType.Spheres;

        // VR Menu
        _vrMenu = Instantiate(vrMenuPrefab, transform);
        _vrMenu.tag = "Moveable";
        _vrMenu.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        _vrMenu.transform.localPosition = new Vector3(-0.5f, 0.4f, 0f);
        
        // Add Components to VR Menu
        BoxCollider vrMenuBoxCollider = _vrMenu.AddComponent<BoxCollider>();
        vrMenuBoxCollider.size = new Vector3(1f, 2f, 0.01f);
        Rigidbody vrMenuRigidbody = _vrMenu.AddComponent<Rigidbody>();
        vrMenuRigidbody.useGravity = false; // Disable gravity
        vrMenuRigidbody.isKinematic = true; // Prevents movement caused by physics
        
        // Get the script of the component
        _vrMenu.GetComponent<VRMenuInteractor>().visualisation = _visualisation;
    }
}
