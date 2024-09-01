using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using TMPro;
using Newtonsoft.Json;

public class GeneralStockManager : MonoBehaviour
{
    // References the StockItemPrefab
    public GameObject StockItemPrefab;
    public Transform GeneralStockContent;

    [System.Serializable]
    public class GeneralStockData
    {
        public string stockSymbol;
        public string stockName;
        public double stockPrice;
        public double stockChange;
        public double stockChangePercentage;
    }

    public List<GeneralStockData> generalStockData = new List<GeneralStockData>();

    private const string GeneralStockDataScriptPath = "./Assets/Scripts/general_stock_data_script.py";

    // Start is called before the first frame update
    void Start()
    {
        InitialiseGeneralStockData();
        PopulateGeneralStockDataUI();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PopulateGeneralStockDataUI()
    {
        foreach (Transform child in GeneralStockContent)
        {
            Destroy(child.gameObject);
        }


        foreach (GeneralStockData stockData in generalStockData)
        {
            GameObject newStock = Instantiate(StockItemPrefab, GeneralStockContent);

            TextMeshProUGUI stockSymbol = newStock.transform.Find("StockSymbol").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI stockName = newStock.transform.Find("StockName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI stockPrice = newStock.transform.Find("StockPrice").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI stockChange = newStock.transform.Find("StockChange").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI stockChangePercentage = newStock.transform.Find("StockChangePercentage").GetComponent<TextMeshProUGUI>();

            // Set The Values
            stockSymbol.text = stockData.stockSymbol;
            stockName.text = stockData.stockName;
            stockPrice.text = stockData.stockPrice.ToString();
            stockChange.text = stockData.stockChange.ToString();
            stockChangePercentage.text = stockData.stockChangePercentage.ToString();

            // Debug.Log($"Populated stock item: {stockData.stockSymbol}");

        }
    }

    public void RunPythonScript()
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "python";
        start.Arguments = $"{GeneralStockDataScriptPath}";
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

                var stockDataDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(result);
                Debug.Log(stockDataDict);

                foreach (var stock in stockDataDict)
                {
                    string stockSymbol = stock.Key;
                    var stockDetails = stock.Value;

                    string stockName = stockDetails["Name"].ToString();
                    double stockPrice = Convert.ToDouble(stockDetails["Current Price"]);
                    double stockChange = Convert.ToDouble(stockDetails["Change Figure"]);
                    double stockChangePercentage = Convert.ToDouble(stockDetails["Change Percentage"]);

                    Debug.Log($"Symbol: {stockSymbol}, Name: {stockName}, Price: {stockPrice}, Change: {stockChange}, Change Percentage: {stockChangePercentage}");
                    AddGeneralStockData(stockSymbol, stockName, stockPrice, stockChange, stockChangePercentage);
                }
            }
            string error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
            }
        }
    }

    private void InitialiseGeneralStockData()
    {
        RunPythonScript();
    }

    private void AddGeneralStockData(
        string newStockSymbol,
        string newStockName,
        double newStockPrice,
        double newStockChange,
        double newStockChangePercentage
        )
    {
        GeneralStockData newStockData = new GeneralStockData
        {
            stockSymbol = newStockSymbol,
            stockName = newStockName,
            stockPrice = newStockPrice,
            stockChange = newStockChange,
            stockChangePercentage = newStockChangePercentage
        };

        generalStockData.Add(newStockData);
    }
}
