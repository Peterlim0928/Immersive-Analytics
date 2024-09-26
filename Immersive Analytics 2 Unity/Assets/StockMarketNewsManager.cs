using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class StockMarketNewsManager : MonoBehaviour
{
    // References the GenerelNewsItemPrefab
    public GameObject GeneralNewsItemPrefab;
    public Transform GeneralNewsContent;

    [System.Serializable]
    public class GeneralNews
    {
        public string generalNewsTitle;
        public string generalNewsSource;
        public string generalNewsDateTime;
        public string generalNewsImageURL;
        public string generalNewsSentimentCategory;
    }

    public List<GeneralNews> generalNewsData = new List<GeneralNews>();

    private const string GeneralStockMarketNewsScriptPath = "./Assets/Scripts/script_general_stock_market_news.py";

    // Start is called before the first frame update
    void Start()
    {
        InitialiseGeneralStockNewsData();
        PopulateGeneralStockNewsDataUI();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RunPythonScript()
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "python";
        start.Arguments = $"{GeneralStockMarketNewsScriptPath}";
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.CreateNoWindow = true;

        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();

                var generalStockMarketNewsDict = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(result);

                foreach (var news in generalStockMarketNewsDict)
                {
                    GeneralNews generalNews = new GeneralNews();
                    generalNews.generalNewsTitle = news["title"];
                    generalNews.generalNewsSource = news["source"];
                    generalNews.generalNewsDateTime = news["time_published"];
                    generalNews.generalNewsImageURL = news["image_url"];
                    generalNews.generalNewsSentimentCategory = news["sentiment_label"];
                    generalNewsData.Add(generalNews);
                }
            }
            string error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
            }
        }
    }

    public void InitialiseGeneralStockNewsData()
    {
        RunPythonScript();
    }

    public void PopulateGeneralStockNewsDataUI()
    {
        foreach (Transform child in GeneralNewsContent)
        {
            Destroy(child.gameObject);
        }

        foreach (GeneralNews news in generalNewsData)
        {
            GameObject newGeneralNews = Instantiate(GeneralNewsItemPrefab, GeneralNewsContent);

            newGeneralNews.transform.Find("GeneralNewsTitle").GetComponent<TextMeshProUGUI>().text = news.generalNewsTitle;
            newGeneralNews.transform.Find("GeneralNewsSource").GetComponent<TextMeshProUGUI>().text = news.generalNewsSource;
            newGeneralNews.transform.Find("GeneralNewsDateTime").GetComponent<TextMeshProUGUI>().text = news.generalNewsDateTime;
            newGeneralNews.transform.Find("GeneralNewsSentiment/GeneralNewsSentimentCategory").GetComponent<TextMeshProUGUI>().text = news.generalNewsSentimentCategory;

            Image sentimentPanelImage = newGeneralNews.transform.Find("GeneralNewsSentiment").GetComponent<Image>();
            sentimentPanelImage.color = GetColourForSentiment(news.generalNewsSentimentCategory);

            StartCoroutine(LoadImageFromURL(news.generalNewsImageURL, newGeneralNews.transform.Find("GeneralNewsImage").GetComponent<Image>()));

        }
    }

    IEnumerator LoadImageFromURL(string url, Image imageComponent)
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

            // Calculate the aspect ratio of the image
            float aspectRatio = (float)texture.width / texture.height;

            // Fit the image within 100x100 while maintaining aspect ratio
            RectTransform rectTransform = imageComponent.GetComponent<RectTransform>();
            if (aspectRatio > 1) // If the image is wider than tall
            {
                rectTransform.sizeDelta = new Vector2(70, 700 / aspectRatio);
            }
            else // If the image is taller than wide or square
            {
                rectTransform.sizeDelta = new Vector2(70 * aspectRatio, 70);
            }

            // Optionally, enable "Preserve Aspect" on the Image component to avoid distortion
            imageComponent.preserveAspect = true;
        }
    }

    private Color GetColourForSentiment(string sentimentCategory)
    {
        string hexColor = "#FFFFFF";

        switch (sentimentCategory.ToLower())
        {
            case "bearish":
                hexColor = "#FF0000";
                break;
            case "somewhat-bearish":
                hexColor = "#FDBC5A";
                break;
            case "neutral":
                hexColor = "#47A6FF";
                break;
            case "somewhat-bullish":
                hexColor = "#9BFF9E";
                break;
            case "bullish":
                hexColor = "#38AB3D";
                break;
            default:
                hexColor = "#FFFFFF";
                break;
        }

        Color sentimentColor;
        if (ColorUtility.TryParseHtmlString(hexColor, out sentimentColor))
        {
            return sentimentColor;
        }
        else
        {
            Debug.LogError("Invalid hex color code: " + hexColor);
            return Color.white;
        }
    }

    public void ReadMoreButtonHandler()
    {
        Debug.Log("Read more button clicked!");
    }
}
