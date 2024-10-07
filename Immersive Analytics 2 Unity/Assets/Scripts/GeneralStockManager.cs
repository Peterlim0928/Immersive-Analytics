using System;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.Networking;


public class GeneralStockManager : MonoBehaviour
{
    // References the StockItemPrefab
    public GameObject StockItemPrefab;
    public Transform GeneralStockContent;
    public GameObject WatchlistItemPrefab;
    public Transform WatchlistContent;

    [System.Serializable]
    public class StockDataBase
    {
        public string stockSymbol;
        public string stockName;
        public double stockPrice;
        public double stockChangePercentage;
        public string stockLogo;

    }

    [System.Serializable]
    public class GeneralStockData : StockDataBase
    {
        public double stockChange;
    }

    [System.Serializable]
    public class WatchlistStockData : StockDataBase
    {
    }

    // Correct the list declarations
    public List<StockDataBase> generalStockData = new List<StockDataBase>();
    private List<StockDataBase> watchlistData = new List<StockDataBase>();
    private Dictionary<string, Image> stockHeartIcons = new Dictionary<string, Image>();



    private const string GeneralStockDataScriptPath = "./Assets/Scripts/general_stock_data_script.py";

    // Sprites for heart icons
    public Sprite emptyHeartSprite;
    public Sprite fullHeartSprite;

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

        int counter = 0;
        foreach (StockDataBase stockData in generalStockData)
        {
            GeneralStockData castedStockData = (GeneralStockData)stockData;

            GameObject newStock = Instantiate(StockItemPrefab, GeneralStockContent);
            newStock.GetComponent<Image>().color = counter++ % 2 == 0
                ? new Color(182 / 255f, 182 / 255f, 182 / 255f, 190 / 255f)
                : new Color(221 / 255f, 221 / 255f, 221 / 255f, 190 / 255f);

            TextMeshProUGUI stockSymbol = newStock.transform.Find("StockSymbol").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI stockName = newStock.transform.Find("StockName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI stockPrice = newStock.transform.Find("StockPrice").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI stockChange = newStock.transform.Find("StockChange").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI stockChangePercentage = newStock.transform.Find("StockChangePercentage").GetComponent<TextMeshProUGUI>();

            // Set The Values
            stockSymbol.text = castedStockData.stockSymbol;
            stockName.text = castedStockData.stockName;
            stockPrice.text = castedStockData.stockPrice.ToString();
            stockChange.text = castedStockData.stockChange.ToString();
            stockChangePercentage.text = castedStockData.stockChangePercentage.ToString();

            if (castedStockData.stockChange > 0)
            {
                stockChange.color = new Color(0.0f, 0.5f, 0.0f);
                stockChangePercentage.color = new Color(0.0f, 0.5f, 0.0f);
            }
            else
            {
                stockChange.color = Color.red;
                stockChangePercentage.color = Color.red;
            }

            StartCoroutine(LoadImageFromURL(castedStockData.stockLogo, newStock.transform.Find("StockLogo").GetComponent<Image>(), 42));
            // Find the AddToWatchlist Button
            Button addToWatchlistButton = newStock.transform.Find("AddWatchlistButton").GetComponent<Button>();
            Image buttonImage = addToWatchlistButton.GetComponent<Image>();

            // Store the heart icon reference in the dictionary
            stockHeartIcons[castedStockData.stockSymbol] = buttonImage;

            addToWatchlistButton.onClick.AddListener(() => MainWatchlistButtonHandler(castedStockData, buttonImage));


        }
    }

    public void PopulateWatchlistDataUI()
    {
        foreach (Transform child in WatchlistContent)
        {
            Destroy(child.gameObject);
        }

        foreach (StockDataBase stockData in watchlistData)
        {
            WatchlistStockData castedStockData = (WatchlistStockData)stockData;

            GameObject newStock = Instantiate(WatchlistItemPrefab, WatchlistContent);

            TextMeshProUGUI stockSymbol = newStock.transform.Find("StockSymbol").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI stockName = newStock.transform.Find("StockName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI stockPrice = newStock.transform.Find("StockPrice").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI stockChangePercentage = newStock.transform.Find("StockChangePercentage").GetComponent<TextMeshProUGUI>();

            // Set The Values
            stockSymbol.text = castedStockData.stockSymbol;
            stockName.text = castedStockData.stockName;
            stockPrice.text = castedStockData.stockPrice.ToString();
            stockChangePercentage.text = castedStockData.stockChangePercentage.ToString();

            StartCoroutine(LoadImageFromURL(castedStockData.stockLogo, newStock.transform.Find("StockIcon").GetComponent<Image>(), 50));

            // Find the SubWatchlistButton Button
            Button addToWatchlistButton = newStock.transform.Find("SubWatchlistButton").GetComponent<Button>();
            Image buttonImage = addToWatchlistButton.GetComponent<Image>();

            addToWatchlistButton.onClick.AddListener(() => SubWatchlistButtonHandler(castedStockData, buttonImage));
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

        int counter = 0;
        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                Debug.Log(result);

                var stockDataDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(result);
                // Debug.Log(stockDataDict);

                foreach (var stock in stockDataDict)
                {
                    string stockSymbol = stock.Key;
                    var stockDetails = stock.Value;

                    string stockName = stockDetails["Name"].ToString();
                    double stockPrice = Convert.ToDouble(stockDetails["Current Price"]);
                    double stockChange = Convert.ToDouble(stockDetails["Change Figure"]);
                    double stockChangePercentage = Convert.ToDouble(stockDetails["Change Percentage"]);
                    string stockLogo = $"https://assets.parqet.com/logos/symbol/{stockSymbol}?format=jpg";

                    // Debug.Log($"Symbol: {stockSymbol}, Name: {stockName}, Price: {stockPrice}, Change: {stockChange}, Change Percentage: {stockChangePercentage}");
                    AddGeneralStockData(stockSymbol, stockName, stockPrice, stockChange, stockChangePercentage, stockLogo);
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

    private void AddGeneralStockData(string newStockSymbol, string newStockName, double newStockPrice, double newStockChange, double newStockChangePercentage, string newStockLogo)
    {
        GeneralStockData newStockData = new GeneralStockData
        {
            stockSymbol = newStockSymbol,
            stockName = newStockName,
            stockPrice = newStockPrice,
            stockChange = newStockChange,
            stockChangePercentage = newStockChangePercentage,
            stockLogo = newStockLogo
        };

        generalStockData.Add(newStockData);
    }

    public void MainWatchlistButtonHandler(GeneralStockData stockData, Image buttonImage)
    {

        // Add to Watchlist
        if (buttonImage.sprite == emptyHeartSprite)
        {
            buttonImage.sprite = fullHeartSprite;
            AddToWatchlist(stockData);
        }
        // Remove from Watchlist
        else
        {
            buttonImage.sprite = emptyHeartSprite;
            RemoveFromWatchlist(stockData);
        }
    }

    public void SubWatchlistButtonHandler(WatchlistStockData stockData, Image buttonImage)
    {
        // Remove from Watchlist
        if (buttonImage.sprite == fullHeartSprite)
        {
            buttonImage.sprite = emptyHeartSprite;
            RemoveFromWatchlist(stockData);

            // Update heart icon in the main panel
            if (stockHeartIcons.ContainsKey(stockData.stockSymbol))
            {
                stockHeartIcons[stockData.stockSymbol].sprite = emptyHeartSprite;
            }
        }
    }

    private void AddToWatchlist(GeneralStockData stockData)
    {
        WatchlistStockData newWatchlistData = new WatchlistStockData
        {
            stockSymbol = stockData.stockSymbol,
            stockName = stockData.stockName,
            stockPrice = stockData.stockPrice,
            stockChangePercentage = stockData.stockChangePercentage,
            stockLogo = stockData.stockLogo
        };

        watchlistData.Add(newWatchlistData);
        PopulateWatchlistDataUI();
    }

    private void RemoveFromWatchlist(StockDataBase stockData)
    {
        WatchlistStockData stockToRemove = (WatchlistStockData)watchlistData.Find(watchlistStock => watchlistStock.stockSymbol == stockData.stockSymbol);

        if (stockToRemove != null)
        {
            watchlistData.Remove(stockToRemove);
            PopulateWatchlistDataUI();
        }
    }

    IEnumerator LoadImageFromURL(string url, Image imageComponent, int imageSize)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

            // Create a sprite from the texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // Assign the sprite to the Image component
            imageComponent.sprite = sprite;

            RectTransform rectTransform = imageComponent.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(imageSize, imageSize);

            // Optionally, enable "Preserve Aspect" on the Image component to avoid distortion
            imageComponent.preserveAspect = true;
        }
    }
}