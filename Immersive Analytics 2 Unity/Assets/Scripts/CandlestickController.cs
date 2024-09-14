using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Handles the Candlestick graph rendering, real-time updates, and
/// communication with Python script for fetching stock data.
/// </summary>
public class CandlestickController : MonoBehaviour
{
    // Path for downloading the Python script
    private const string ScriptPath = "./Assets/Scripts/script.py";
    private const string DataPath = "./Assets/Datasets/";

    // Stock code input field and dropdowns
    public TMP_InputField stockCodeInputField;
    public TMP_Dropdown stockTimeDropdown;
    public TMP_Dropdown stockTimeIntervalDropdown;
    public TMP_Dropdown stockRealTimeIntervalDropdown;

    // Real-time data variables
    private string _stockCode;
    private string _stockTimeOption;
    private string _stockTimeInterval;
    private int _stockTimePeriod;

    // Parsed data and graph dimensions
    private Candlestick _parsedData;
    private readonly int _canvasWidth = 500;
    private readonly int _canvasHeight = 500;

    private readonly string[] _stockTimeOptionList = 
    {
        "1 Day", "5 Days", "1 Month", "3 Months", "6 Months", "1 Year", "2 Years", "5 Years", "10 Years",
        "Year To Date", "All"
    };

    private readonly string[] _stockTimeList = 
    {
        "1d", "5d", "1mo", "3mo", "6mo", "1y", "2y", "5y", "10y", "ytd", "max"
    };

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

    /// <summary>
    /// Start is called before the first frame update.
    /// It initializes dropdown options for stock time periods and intervals.
    /// </summary>
    void Start()
    {
        stockTimeDropdown.AddOptions(_stockTimeOptionList.ToList());
        stockRealTimeIntervalDropdown.AddOptions(_realTimeUpdateIntervalOptions.ToList());
        stockTimeIntervalDropdown.AddOptions(_stockTimeIntervalOptionsList.ToList());
    }

    /// <summary>
    /// Reads stock options from the input field and dropdowns, then runs
    /// the Python script to download stock data. Also sets up real-time
    /// updating for the candlestick graph.
    /// </summary>
    public void ReadStockOptions()
    {
        _stockTimePeriod = stockTimeDropdown.value;
        _stockTimeInterval = _stockTimeIntervalOptions[stockTimeIntervalDropdown.value];
        string chosenTime = _stockTimeList[_stockTimePeriod];

        // Run the Python script to download stock data
        string pythonScriptPath = ScriptPath;
        string pythonArgs = $"{stockCodeInputField.text} {chosenTime} {_stockTimeInterval}";

        // Set up variables for real-time updates
        _stockCode = stockCodeInputField.text.ToUpper();
        _stockTimeOption = chosenTime;

        RunPythonScript(pythonScriptPath, pythonArgs);
        UpdateGraphRealTime();
    }

    /// <summary>
    /// Executes a Python script using the specified script path and arguments.
    /// The script fetches stock data based on the user's input.
    /// </summary>
    /// <param name="scriptPath">The path to the Python script.</param>
    /// <param name="arguments">Arguments to pass to the Python script.</param>
    public void RunPythonScript(string scriptPath, string arguments)
    {
        ProcessStartInfo start = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"{scriptPath} {arguments}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                reader.ReadToEnd();
            }
            string error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
            }
        }
    }

    /// <summary>
    /// Updates the graph at regular intervals by fetching real-time data.
    /// The update frequency is controlled by the real-time interval dropdown.
    /// </summary>
    private async void UpdateGraphRealTime()
    {
        int index = stockRealTimeIntervalDropdown.value;
        int interval = _realTimeUpdateIntervalInMS[index];

        while (true)
        {
            // Fetch new data and update the graph
            string pythonScriptPath = ScriptPath;
            string pythonArgs = $"{_stockCode} {_stockTimeOption} {_stockTimeInterval}";
            RunPythonScript(pythonScriptPath, pythonArgs);

            UpdateGraph();

            // Wait for the next interval
            await Task.Delay(interval);
        }
    }

    /// <summary>
    /// Updates the graph by reading the CSV file containing stock data,
    /// parsing it, and rendering it on the graph.
    /// </summary>
    public void UpdateGraph()
    {
        // Construct the file name and path based on stock code and time period
        string fileName = $"{_stockCode}-{_stockTimeList[_stockTimePeriod]}.csv";
        string filePath = Path.Combine(DataPath, fileName);

        if (File.Exists(filePath))
        {
            TextAsset csvFile = new TextAsset(File.ReadAllText(filePath));

            if (csvFile != null)
            {
                // Parse the CSV data into a list of DataPoints
                List<DataPoint> dataPoints = ReadCsvToDataPoints(filePath);

                // Create and parse candlestick data
                _parsedData = new Candlestick(_canvasWidth, _canvasHeight);
                CandlestickData renderData = _parsedData.ParseCandlestickData(dataPoints);

                // Render the parsed data
                RenderData(renderData);
            }
            else
            {
                Debug.LogError("CSV file not found in Resources folder.");
            }
        }
        else
        {
            Debug.LogError($"File not found: {filePath}");
        }
    }

    /// <summary>
    /// Reads CSV data from the given file path and converts it into a list of DataPoint objects.
    /// </summary>
    /// <param name="filePath">The path to the CSV file.</param>
    /// <returns>A list of DataPoint objects parsed from the CSV file.</returns>
    public List<DataPoint> ReadCsvToDataPoints(string filePath)
    {
        List<DataPoint> dataPoints = new List<DataPoint>();

        // Read all lines from the CSV file
        string[] csvLines = File.ReadAllLines(filePath);

        // Skip the header line (the first line)
        for (int i = 1; i < csvLines.Length; i++)
        {
            string line = csvLines[i];

            // Split the line by commas (CSV format)
            string[] columns = line.Split(',');

            // Parse each column to its respective field
            DataPoint dataPoint = new DataPoint
            {
                time = DateTime.Parse(columns[0], CultureInfo.InvariantCulture),   // Parse the date
                open = float.Parse(columns[1], CultureInfo.InvariantCulture),      // Open price
                high = float.Parse(columns[2], CultureInfo.InvariantCulture),      // High price
                low = float.Parse(columns[3], CultureInfo.InvariantCulture),       // Low price
                close = float.Parse(columns[4], CultureInfo.InvariantCulture)      // Close price
            };

            // Add the parsed DataPoint to the list
            dataPoints.Add(dataPoint);
        }

        Debug.Log($"Number of data points in this CSV: {dataPoints.Count}");

        return dataPoints;
    }

    /// <summary>
    /// Renders the candlestick data on the graph by creating 3D objects
    /// (candle bodies and wicks) based on the parsed stock data.
    /// </summary>
    /// <param name="renderData">The data to be rendered on the graph.</param>
    public void RenderData(CandlestickData renderData)
    {
        // Find the object that contains all data
        Transform parent = transform.Find("CandlestickGraph").Find("Datapoints");

        // Clear all old data in reverse order (avoid mutating while iterating)
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }

        foreach (var dataPoint in renderData.chartData)
        {
            GameObject candle = new GameObject();
            candle.transform.SetParent(parent);
            candle.transform.localRotation = Quaternion.identity;

            // 1. Create a new GameObject for the candlestick body (Cube)
            GameObject candleBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            candleBody.transform.SetParent(candle.transform);

            // 2. Position the candle in the x-axis (centered on the xCenter value)
            candleBody.transform.localPosition = new Vector3(dataPoint.xCenter, (dataPoint.openY + dataPoint.closeY) / 2, 0);

            // 3. Scale the body height (difference between open and close prices)
            candleBody.transform.localScale = new Vector3(dataPoint.dayWidth, Math.Abs(dataPoint.closeY - dataPoint.openY), 1);

            // 4. Color the body (Green for bullish, Red for bearish)
            Renderer bodyRenderer = candleBody.GetComponent<Renderer>();
            bodyRenderer.material.color = dataPoint.isBullish ? Color.green : Color.red;

            // 5. Create the wick (Cylinder) for the high-low range
            GameObject candleWick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            candleWick.transform.SetParent(candle.transform);

            // 6. Position the wick in the center (same x as body, but full height from low to high)
            candleWick.transform.localPosition = new Vector3(dataPoint.xCenter, (dataPoint.highY + dataPoint.lowY) / 2, 0);

            // 7. Scale the wick (thin and tall)
            candleWick.transform.localScale = new Vector3(0.1f, (dataPoint.highY - dataPoint.lowY) / 2, 0.1f);
        }

        // Scale down the graph
        parent.localScale = new Vector3(0.001f, 0.001f, 0.001f);
    }

    /// <summary>
    /// Prints detailed candlestick data for debugging purposes, including
    /// chart data, X-axis labels, and Y-axis labels.
    /// </summary>
    /// <param name="data">The candlestick data to be printed.</param>
    public void PrintRenderData(CandlestickData data)
    {
        string str = "";

        // Print graph data
        str += $"Canvas Width: {data.graphData.width}\n";
        str += $"Canvas Height: {data.graphData.height}\n";
        str += $"Min Value: {data.graphData.minValue}\n";
        str += $"Max Value: {data.graphData.maxValue}\n\n";

        // Print first 3 chart data points
        for (int i = 0; i < 3; i++)
        {
            str += $"Open Y: {data.chartData[i].openY} | ";
            str += $"Close Y: {data.chartData[i].closeY} | ";
            str += $"High Y: {data.chartData[i].highY} | ";
            str += $"Low Y: {data.chartData[i].lowY} | ";
            str += $"X Center: {data.chartData[i].xCenter} | ";
            str += $"Day Width: {data.chartData[i].dayWidth} | ";
            str += $"Is Bullish: {data.chartData[i].isBullish}\n";
        }

        str += "\n";

        // Print X labels
        foreach (var xData in data.xAxis)
        {
            str += $"X: {xData.x}, Label: {xData.labelText}\n";
        }

        str += "\n";

        // Print Y labels
        foreach (var yData in data.yAxis)
        {
            str += $"Y: {yData.y}, Label: {yData.labelText}\n";
        }

        Debug.Log(str);
    }
}
