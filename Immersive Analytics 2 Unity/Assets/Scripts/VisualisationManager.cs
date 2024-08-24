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
    // Path for downloading script
    private const string ScriptPath = "./Assets/Scripts/script.py";
    private const string DataPath = "./Assets/Datasets/";
    
    // Stock code textbox
    public TMP_InputField stockCodeInputField;
    public TMP_Dropdown stockTimeDropdown;

    // Real-Time Variables
    private string _stockCode;
    private string _stockTimeOption;
    private static Timer _realTimeTimer;
    public TMP_Dropdown stockRealTimeIntervalDropdown;

    private readonly string[] _stockTimeOptionList =
    {
        "1 Day", "5 Days", "1 Month", "3 Months", "6 Months", "1 Year", "2 Years", "5 Years", "10 Years",
        "Year To Date", "All"
    };
    private readonly string[] _stockTimeList = { "1d", "5d", "1mo", "3mo", "6mo", "1y", "2y", "5y", "10y", "ytd", "max" };
    
    private readonly string[] _realTimeUpdateIntervalOptions =
    {
        "15 secs", "30 secs", "1 min", "5 mins", "15 mins", "30 mins", "1 hour", "3 hour", "5 hours"
    };

    private readonly int[] _realTimeUpdateIntervalInMS =
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
        Debug.Log($"Stock Code: {stockCodeInputField.text} | Stock Time Option: {chosenTime}");
                
        // Run the Download Python Script
        string pythonScriptPath = ScriptPath;
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
        Debug.Log($"(Real Time Update) Stock Code: {_stockCode} | Stock Time Option: {_stockTimeOption}");
        
        // Run the Download Python Script
        string pythonScriptPath = ScriptPath;
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
        // Load CSV file and update data source
        string fileName = stockCodeInputField.text.ToUpper() + "-" + _stockTimeList[stockTimeDropdown.value] + ".csv";
        string filePath = Path.Combine(DataPath, fileName);

        if (File.Exists(filePath))
        {
            TextAsset csvFile = new TextAsset(File.ReadAllText(filePath));
            
            if (csvFile != null)
            {
                // Assign the CSV file content to the Data field of CSVDataSource
                DataSource currentDataSource = transform.Find("DataSource").GetComponent<DataSource>();
                currentDataSource.data = csvFile;
                currentDataSource.ParseCsv();
                
                // Update the dropdown options on the menu
                GameObject vrMenu = transform.Find("VRMenu").gameObject;
                vrMenu.GetComponent<VRMenuInteractor>().UpdateDropdown();
            }
            else
            {
                Debug.LogError("CSV file not found in Resources folder.");
            }
        }
    }
}
