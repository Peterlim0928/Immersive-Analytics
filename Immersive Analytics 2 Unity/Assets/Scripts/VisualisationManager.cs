using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    private string _stockTimeInterval;
    private int _stockTimePeriod;
    public TMP_Dropdown stockRealTimeIntervalDropdown;
    
    public TMP_Dropdown stockTimeIntervalDropdown;

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
    
    private readonly string[] _stockTimeIntervalOptionsList =
    {
        "1 min", "2 mins", "5 mins", "15 mins", "30 mins", "60 mins", "1 hour", "1.5 hour", "1 day", "5 days", "1 week", "1 month", "3 months"
    };
    
    private readonly string[] _stockTimeIntervalOptions =
    {
        "1m", "2m", "5m", "15m", "30m", "60m", "90m", "1h", "1d", "5d", "1wk", "1mo", "3mo"
    };

    void Start()
    {
        stockTimeDropdown.AddOptions(_stockTimeOptionList.ToList());
        stockRealTimeIntervalDropdown.AddOptions(_realTimeUpdateIntervalOptions.ToList());
        stockTimeIntervalDropdown.AddOptions(_stockTimeIntervalOptionsList.ToList());
    }
    
    public void ReadStockOptions()
    { 
        _stockTimePeriod = stockTimeDropdown.value; 
        string chosenTime = _stockTimeList[_stockTimePeriod];
        _stockTimeInterval = _stockTimeIntervalOptions[stockTimeIntervalDropdown.value];
        Debug.Log($"Stock Code: {stockCodeInputField.text} | Stock Time Period Option: {_stockTimeOption} | Stock Time Interval: {_stockTimeInterval}");
                
        // Run the Download Python Script
        string pythonScriptPath = ScriptPath;
        string pythonArgs = $"{stockCodeInputField.text} {chosenTime} {_stockTimeInterval}";
        
        //Adjust variable for Real-Time update
        _stockCode = stockCodeInputField.text.ToUpper();
        Debug.Log(_stockCode);
        _stockTimeOption = chosenTime;
        
        RunPythonScript(pythonScriptPath, pythonArgs);
        
        UpdateGraph();
        UpdateGraphRealTime();
    }
    
    private void UpdateGraphRealTimeHelper()
    {
        Debug.Log($"(Real Time Update) Stock Code: {_stockCode} | Stock Time Period Option: {_stockTimeOption} | Stock Time Interval: {_stockTimeInterval}");
        
        // Run the Download Python Script
        string pythonScriptPath = ScriptPath;
        string pythonArgs = $"{_stockCode} {_stockTimeOption} {_stockTimeInterval}";
        RunPythonScript(pythonScriptPath, pythonArgs);
        
        UpdateGraphForRealTime();
        Debug.Log("After Calling");
    }
    
    private async void UpdateGraphRealTime()
    {
        int index = stockRealTimeIntervalDropdown.value; 
        int interval = _realTimeUpdateIntervalInMS[index];
        while (true)
        {
            for (int i = 0; i < interval; i++) // Adjust the loop limit as needed
            {
                await Task.Delay(1);
            }
            UpdateGraphRealTimeHelper();
        }
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
                Debug.Log("Run Python Script");
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
                
                transform.Find("VisualisationGraph").GetComponent<VisualisationGraph>().UpdateGraph();
                
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
    
    private void UpdateGraphForRealTime()
    {
        Debug.Log("Starting Update Graph");
        // Load CSV file and update data source
        string fileName = _stockCode+ "-" + _stockTimeList[_stockTimePeriod] + ".csv";
        string filePath = Path.Combine(DataPath, fileName);
        
        Debug.Log("After File Path");
        
        TextAsset csvFile = new TextAsset(File.ReadAllText(filePath));

        Debug.Log("Updating Real Time");
        
        if (csvFile != null)
        {
            Debug.Log("Updating Real Time Inner");
            // Assign the CSV file content to the Data field of CSVDataSource
            DataSource currentDataSource = transform.Find("DataSource").GetComponent<DataSource>();
            currentDataSource.data = csvFile;
            currentDataSource.ParseCsv();
            
            transform.Find("VisualisationGraph").GetComponent<VisualisationGraph>().UpdateGraph();
            
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
