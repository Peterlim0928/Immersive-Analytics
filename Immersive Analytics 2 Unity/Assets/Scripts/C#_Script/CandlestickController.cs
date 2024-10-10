using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Debug = UnityEngine.Debug;

/// <summary>
/// Handles the Candlestick graph rendering, real-time updates, and
/// communication with Python script for fetching stock data.
/// </summary>
public class CandlestickController : MonoBehaviour
{
    // Path for downloading the Python script
    private const string ScriptPath = "./Assets/Scripts/Python_API_Script/script.py";
    private const string DataPath = "./Assets/Datasets/";

    // Stock code input field and dropdowns
    public TMP_InputField stockCodeInputField;
    public TMP_Dropdown stockTimeDropdown;
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
    private readonly int _minScaling = 2;
    private readonly int _maxScaling = 8;
    private float _rodInitialScaling;
    private float _tipInitialPosition;
    private float _detailCanvasInitialPosition;
    private bool _isRunning;

    private readonly string[] _stockTimeOptionList = 
    {
        "1 Day", "5 Days", "1 Month", "6 Months", "Year To Date", "1 Year", "5 Years", "Max"
    };

    public readonly string[] StockTimeList = 
    {
        "1d", "5d", "1mo", "6mo", "ytd", "1y", "5y", "max"
    };

    private readonly string[] _realTimeUpdateIntervalOptions = 
    {
        "15 secs", "30 secs", "1 min", "5 mins", "15 mins", "30 mins", "1 hour", "3 hour", "5 hours"
    };

    private readonly int[] _realTimeUpdateIntervalInMS = 
    {
        15000, 30000, 60000, 300000, 900000, 1800000, 3600000, 10800000, 18000000
    };

    /// <summary>
    /// Start is called before the first frame update.
    /// It initializes dropdown options for stock time periods and intervals.
    /// </summary>
    void Start()
    {
        _isRunning = true;
        
        _rodInitialScaling = transform.Find("CandlestickGraph").Find("X Axis Rod").localScale.x;
        _tipInitialPosition = transform.Find("CandlestickGraph").Find("X Axis Tip").localPosition.x;
        _detailCanvasInitialPosition = transform.Find("DetailCanvas").localPosition.x;
        
        stockTimeDropdown.AddOptions(_stockTimeOptionList.ToList());
        stockRealTimeIntervalDropdown.AddOptions(_realTimeUpdateIntervalOptions.ToList());
    }

    private void OnApplicationQuit()
    {
        _isRunning = false;
    }

    private async void DisplayErrorMessage()
    {
        transform.Find("SearchCanvas").Find("Image").Find("ErrorMessage").GetComponent<TextMeshProUGUI>().text = "Invalid Stock Code";
        await Task.Delay(5000);
        transform.Find("SearchCanvas").Find("Image").Find("ErrorMessage").GetComponent<TextMeshProUGUI>().text = "";
    }

    /// <summary>
    /// Reads stock options from the input field and dropdowns, then runs
    /// the Python script to download stock data. Also sets up real-time
    /// updating for the candlestick graph.
    /// </summary>
    public async void ReadStockOptions()
    {
        // Regex pattern for validating the ticker symbol
        string pattern = @"^[A-Z]{1,5}(\.[A-Z]{1,2})?$";

        // Get the text from the input field
        string stockCode = stockCodeInputField.text.ToUpper();

        // Perform regex validation
        if (!Regex.IsMatch(stockCode, pattern))
        {
            Debug.LogError("Invalid stock code. Please enter a valid ticker symbol.");
            DisplayErrorMessage(); // Display the error message if the input is invalid
        }
        
        _stockTimePeriod = stockTimeDropdown.value;
        string chosenTime = StockTimeList[_stockTimePeriod];

        switch (stockTimeDropdown.value)
        {
            case 0:
                _stockTimeInterval = "15m";
                break;
            case 1:
                _stockTimeInterval = "90m";
                break;
            case 2:
                _stockTimeInterval = "1d";
                break;
            case 3:
                _stockTimeInterval = "5d";
                break;
            case 4:
                _stockTimeInterval = "5d";
                break;
            case 5:
                _stockTimeInterval = "1wk";
                break;
            case 6:
                _stockTimeInterval = "1mo";
                break;
            case 7:
                _stockTimeInterval = "3mo";
                break;
        }
        
        // Run the Python script to download stock data
        string pythonScriptPath = ScriptPath;
        string pythonArgs = $"{stockCodeInputField.text} {chosenTime} {_stockTimeInterval}";

        // Set up variables for real-time updates
        _stockCode = stockCode;
        _stockTimeOption = chosenTime;

        await RunPythonScript(pythonScriptPath, pythonArgs);
        UpdateGraphRealTime();
        
    }

    /// <summary>
    /// Executes a Python script using the specified script path and arguments.
    /// The script fetches stock data based on the user's input.
    /// </summary>
    /// <param name="scriptPath">The path to the Python script.</param>
    /// <param name="arguments">Arguments to pass to the Python script.</param>
    public async Task RunPythonScript(string scriptPath, string arguments)
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

        await Task.Run(() =>
        {
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
        });
    }

    /// <summary>
    /// Updates the graph at regular intervals by fetching real-time data.
    /// The update frequency is controlled by the real-time interval dropdown.
    /// </summary>
    private async void UpdateGraphRealTime()
    {
        int index = stockRealTimeIntervalDropdown.value;
        int interval = _realTimeUpdateIntervalInMS[index];

        while (_isRunning)
        {
            // Fetch new data and update the graph
            string pythonScriptPath = ScriptPath;
            string pythonArgs = $"{_stockCode} {_stockTimeOption} {_stockTimeInterval}";
            await RunPythonScript(pythonScriptPath, pythonArgs);

            if (IsFileValid(Path.Combine(DataPath, $"{_stockCode}-{StockTimeList[_stockTimePeriod]}.csv")))
            {
                UpdateGraph();
            }
            else
            {
                DisplayErrorMessage();
            }

            // Wait for the next interval
            await Task.Delay(interval);
        }
    }

    public bool IsFileValid(string filePath)
    {
        try
        {
            using (var reader = new StreamReader(filePath))
            {
                int rowCount = 0;
                
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        rowCount++;
                    }

                    // If we have more than one row, it's valid (header + data)
                    if (rowCount > 1)
                    {
                        return true;
                    }
                }
            }

            // If rowCount <= 1, it means only header or no rows at all
            return false;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("File not found.");
            return false;
        }
    }

    /// <summary>
    /// Updates the graph by reading the CSV file containing stock data,
    /// parsing it, and rendering it on the graph.
    /// </summary>
    public void UpdateGraph()
    {
        // Construct the file name and path based on stock code and time period
        string fileName = $"{_stockCode}-{StockTimeList[_stockTimePeriod]}.csv";
        string filePath = Path.Combine(DataPath, fileName);

        if (File.Exists(filePath))
        {
            TextAsset csvFile = new TextAsset(File.ReadAllText(filePath));

            if (csvFile != null)
            {
                // Parse the CSV data into a list of DataPoints
                List<DataPoint> dataPoints = ReadCsvToDataPoints(filePath);

                // Create and parse candlestick data
                float scaling = dataPoints.Count / 50f > _maxScaling ? _maxScaling : dataPoints.Count / 50f;
                scaling = scaling < _minScaling ? _minScaling : scaling;
                _parsedData = new Candlestick(_canvasWidth * scaling, _canvasHeight * scaling);
                CandlestickData renderData = _parsedData.ParseCandlestickData(dataPoints);
                
                // Edit the graph frame
                Transform graphTransform = transform.Find("CandlestickGraph");
                
                // Edit the rod scaling
                Transform xAxisRod = graphTransform.Find("X Axis Rod");
                Transform yAxisRod = graphTransform.Find("Y Axis Rod");
                xAxisRod.localScale = new Vector3(_rodInitialScaling * scaling + 16, xAxisRod.localScale.y, xAxisRod.localScale.z);
                yAxisRod.localScale = new Vector3(_rodInitialScaling * scaling + 16, yAxisRod.localScale.y, yAxisRod.localScale.z);
                
                // Edit the tip position
                Transform xAxisTip = graphTransform.Find("X Axis Tip");
                Transform yAxisTip = graphTransform.Find("Y Axis Tip");
                xAxisTip.localPosition = new Vector3((_tipInitialPosition - 0.02f) * scaling + 0.08f, xAxisTip.localPosition.y,
                    xAxisTip.localPosition.z);
                yAxisTip.localPosition = new Vector3(yAxisTip.localPosition.x, (_tipInitialPosition - 0.02f) * scaling + 0.08f,
                    yAxisTip.localPosition.z);
                
                // Move the details canvas based on how much the graph scaled
                Transform detailCanvasTransform = transform.Find("DetailCanvas").transform;
                Vector3 detailCanvasPos = detailCanvasTransform.localPosition;
                detailCanvasPos.x = _detailCanvasInitialPosition + scaling / 2.75f;
                detailCanvasTransform.transform.localPosition = detailCanvasPos;

                // Render the parsed data
                RenderData(renderData);
                
                // Render the stock code x-axis label
                transform.Find("CandlestickGraph").Find("LabelCanvas").Find("StockCodeBackground").Find("StockCode").GetComponent<TextMeshProUGUI>().text = _stockCode;
                transform.Find("SearchCanvas").Find("StockCodeBackground").Find("StockCode").GetComponent<TextMeshProUGUI>().text = _stockCode;
                transform.Find("DetailCanvas").Find("StockCodeBackground").Find("StockCode").GetComponent<TextMeshProUGUI>().text = _stockCode;
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
        Transform datapointParent = transform.Find("CandlestickGraph").Find("Datapoints");
        datapointParent.localScale = new Vector3(1f, 1f, 1f);
        Transform labelParent = transform.Find("CandlestickGraph").Find("Labels");

        // Clear all old data in reverse order (avoid mutating while iterating)
        for (int i = datapointParent.childCount - 1; i >= 0; i--)
        {
            Destroy(datapointParent.GetChild(i).gameObject);
        }
        
        // Clear all old labels in reverse order (both x-axis and y-axis labels)
        for (int i = labelParent.childCount - 1; i >= 0; i--)
        {
            Destroy(labelParent.GetChild(i).gameObject);
        }
        
        for (int i = 0; i < renderData.chartData.Count; i++)
        {
            var dataPoint = renderData.chartData[i];
            var rawDataPoint = renderData.rawData[i];
            
            GameObject candle = new GameObject("Candle");
            candle.transform.SetParent(datapointParent);
            candle.transform.localRotation = Quaternion.identity;

            // Create a new GameObject for the candlestick body (Cube)
            GameObject candleBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            candleBody.transform.SetParent(candle.transform);
            candleBody.transform.localRotation = Quaternion.identity;

            // Position the candle in the x-axis (centered on the xCenter value)
            candleBody.transform.localPosition = new Vector3(dataPoint.xCenter, (dataPoint.openY + dataPoint.closeY) / 2, 0);

            // Scale the body height (difference between open and close prices)
            candleBody.transform.localScale = new Vector3(dataPoint.dayWidth, Math.Abs(dataPoint.closeY - dataPoint.openY), 10);

            // Create the wick (Cylinder) for the high-low range
            GameObject candleWick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            candleWick.transform.SetParent(candle.transform);
            candleWick.transform.localRotation = Quaternion.identity;
            
            // Create a material for both the wick and body
            Material material = candleBody.GetComponent<Renderer>().material;
            material.color = dataPoint.isBullish ? Color.green : Color.red;
            
            // Color the body and wick (Green for bullish, Red for bearish)
            Renderer bodyRenderer = candleBody.GetComponent<Renderer>();
            bodyRenderer.material = material;
            Renderer wickRenderer = candleWick.GetComponent<Renderer>();
            wickRenderer.material = material;
            
            // Create highlighted version of the material
            Material highlightedMaterial = new Material(material);
            highlightedMaterial.EnableKeyword("_EMISSION");
            highlightedMaterial.SetColor("_EmissionColor", material.color);
            
            // Add relevant components
            XRSimpleInteractable candleBodyInteractable = candleBody.AddComponent<XRSimpleInteractable>();
            candleBody.AddComponent<OnHoverEnterEffect>().highlightMaterial = highlightedMaterial;
            Rigidbody candleBodyRigidbody = candleBody.AddComponent<Rigidbody>();
            candleBodyRigidbody.useGravity = false;
            candleBodyRigidbody.isKinematic = true;
            
            XRSimpleInteractable candleWickInteractable = candleWick.AddComponent<XRSimpleInteractable>();
            candleWick.AddComponent<OnHoverEnterEffect>().highlightMaterial = highlightedMaterial;
            Rigidbody candleWickRigidbody = candleWick.AddComponent<Rigidbody>();
            candleWickRigidbody.useGravity = false;
            candleWickRigidbody.isKinematic = true;

            // Position the wick in the center (same x as body, but full height from low to high)
            candleWick.transform.localPosition = new Vector3(dataPoint.xCenter, (dataPoint.highY + dataPoint.lowY) / 2, 0);

            // Scale the wick (thin and tall)
            candleWick.transform.localScale = new Vector3(3f, (dataPoint.highY - dataPoint.lowY) / 2, 3f);
            
            candleBodyInteractable.firstSelectEntered = new SelectEnterEvent();
            candleWickInteractable.firstSelectEntered = new SelectEnterEvent();
            candleBodyInteractable.firstSelectEntered.AddListener(_ => CandleOnPressed(rawDataPoint));
            candleWickInteractable.firstSelectEntered.AddListener(_ => CandleOnPressed(rawDataPoint));
        }

        // Scale down the graph
        datapointParent.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        
        // Add x-axis labels
        foreach (var xAxisData in renderData.xAxis)
        {
            // Create a group that contains the label cylinder and text
            Transform xLabelGroup = new GameObject("X Label Group").transform;
            xLabelGroup.transform.SetParent(labelParent);
            
            // Create a new cylinder object for the X axis label marker
            GameObject xAxisLabel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            xAxisLabel.transform.SetParent(xLabelGroup);
            xAxisLabel.transform.localPosition = new Vector3(0, 1f, 0);
            xAxisLabel.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);  // Thin cylinder
            
            // Create the text for the label using TextMesh or TMP
            GameObject xAxisText = new GameObject("X Axis Label");
            xAxisText.transform.SetParent(xLabelGroup);
            xAxisText.transform.localPosition = new Vector3(0, -3.5f, 0);  // Slightly below the cylinder marker

            // Add TextMesh component to render text
            TextMeshPro textMesh = xAxisText.AddComponent<TextMeshPro>();
            textMesh.text = xAxisData.labelText;
            textMesh.fontSize = 20;
            textMesh.color = Color.black;
            textMesh.alignment = TextAlignmentOptions.Center;
            
            // Adjust the group transform
            xLabelGroup.localScale = new Vector3(1f, 1f, 1f);
            xLabelGroup.localRotation = new Quaternion(0, 0, 0, 0);
            xLabelGroup.localPosition = new Vector3(xAxisData.x / 10 + 4, 0, 0);
        }

        // Add y-axis labels
        foreach (var yAxisData in renderData.yAxis)
        {
            // Create a group that contains the label cylinder and text
            Transform yLabelGroup = new GameObject("Y Label Group").transform;
            yLabelGroup.transform.SetParent(labelParent);
            
            // Create a new cylinder object for the X axis label marker
            GameObject yAxisLabel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            yAxisLabel.transform.SetParent(yLabelGroup);
            yAxisLabel.transform.localPosition = new Vector3(1, 0, 0);
            yAxisLabel.transform.localRotation = Quaternion.Euler(0, 0, 90);
            yAxisLabel.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);  // Thin cylinder
            
            // Create the text for the label using TextMesh or TMP
            GameObject yAxisText = new GameObject("Y Axis Label");
            yAxisText.transform.SetParent(yLabelGroup);
            yAxisText.transform.localPosition = new Vector3(-6, 0, 0);  // Slightly to the left of the cylinder marker

            // Add TextMesh component to render text
            TextMeshPro textMesh = yAxisText.AddComponent<TextMeshPro>();
            textMesh.text = yAxisData.labelText;
            textMesh.fontSize = 20;
            textMesh.color = Color.black;
            textMesh.alignment = TextAlignmentOptions.Center;
            
            // Adjust the group transform
            yLabelGroup.localScale = new Vector3(1f, 1f, 1f);
            yLabelGroup.localRotation = new Quaternion(0, 0, 0, 0);
            yLabelGroup.localPosition = new Vector3(0, yAxisData.y / 10 + 4, 0);
        }
    }

    private void CandleOnPressed(DataPoint data)
    {
        Debug.Log($"The data that is selected: {data}");
        // Find the details canvas
        Transform detailCanvasTransform = transform.Find("DetailCanvas");

        // Update the details canvas
        detailCanvasTransform.Find("Image").Find("Time").GetComponent<TextMeshProUGUI>().text = $"{data.time.ToShortDateString()}";
        detailCanvasTransform.Find("Image").Find("Open").GetComponent<TextMeshProUGUI>().text = $"{Math.Round(data.open, 2)}";
        detailCanvasTransform.Find("Image").Find("High").GetComponent<TextMeshProUGUI>().text = $"{Math.Round(data.high, 2)}";
        detailCanvasTransform.Find("Image").Find("Low").GetComponent<TextMeshProUGUI>().text = $"{Math.Round(data.low, 2)}";
        detailCanvasTransform.Find("Image").Find("Close").GetComponent<TextMeshProUGUI>().text = $"{Math.Round(data.close, 2)}";
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
