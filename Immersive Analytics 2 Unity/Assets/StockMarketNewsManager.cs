using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using UnityEngine.UI;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class StockMarketNewsManager : MonoBehaviour
{
    // References the GeneralNewsItemPrefab
    public GameObject GeneralNewsItemPrefab;
    public Transform GeneralNewsContent;

    // ScrollView for specific news image
    public ScrollRect SpecificNewsImageScrollView;
    public Image SpecificNewsImage;  // This will hold the image inside the ScrollView

    [System.Serializable]
    public class GeneralNews
    {
        public string generalNewsTitle;
        public string generalNewsSource;
        public string generalNewsDateTime;
        public string generalNewsImageURL;
        public string generalNewsSentimentCategory;
        public string specificNewsURL;
    }

    public List<GeneralNews> generalNewsData = new List<GeneralNews>();

    private const string GeneralStockMarketNewsScriptPath = "./Assets/Scripts/script_general_stock_market_news.py";
    private const string RetrieveWebpageScriptPath = "./Assets/Scripts/script_url_screenshot_retrieval.py";

    // Start is called before the first frame update
    void Start()
    {
        InitialiseGeneralStockNewsData();
        PopulateGeneralStockNewsDataUI();
        // LoadImageFromFile("./Assets/webpage_screenshot.png"); // Load the specific image on Start
        // SpecificNewsImageScrollView.verticalNormalizedPosition = 1f;
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
                    generalNews.specificNewsURL = news["url"];
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
        // GeneralNews generalNews = new GeneralNews();
        // generalNews.generalNewsTitle = "Huge News for Apple Stock Investors";
        // generalNews.generalNewsSource = "NASDAQ";
        // generalNews.generalNewsDateTime = "Oct_03_20224";
        // generalNews.generalNewsImageURL = "https://i.natgeofe.com/n/548467d8-c5f1-4551-9f58-6817a8d2c45e/NationalGeographic_2572187_square.jpg";
        // generalNews.generalNewsSentimentCategory = "bearish";
        // generalNews.specificNewsURL = "https://finance.yahoo.com/news/apple-plans-open-four-stores-131707836.html";
        // generalNewsData.Add(generalNews);
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

            // Find the SubWatchlistButton Button
            Button addToWatchlistButton = newGeneralNews.transform.Find("ReadMoreButton").GetComponent<Button>();
            Image buttonImage = addToWatchlistButton.GetComponent<Image>();

            addToWatchlistButton.onClick.AddListener(() => ReadMoreButtonHandler(news.specificNewsURL));
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
                rectTransform.sizeDelta = new Vector2(110, 1100 / aspectRatio);
            }
            else // If the image is taller than wide or square
            {
                rectTransform.sizeDelta = new Vector2(110 * aspectRatio, 110);
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

    public void ReadMoreButtonHandler(string newsUrl)
    {
        Debug.Log("ReadMore");
        string webpageImageFilePath = "./Assets/WebpageImage/image.png";
        string loadingImageFilePath = "./Assets/WebpageImage/loading.png";
        Debug.Log($"{RetrieveWebpageScriptPath} {newsUrl} {webpageImageFilePath}");
        LoadImageFromFile(loadingImageFilePath);
        RunPythonDownloadImgScript(newsUrl, webpageImageFilePath);
    }

    public async Task RunPythonDownloadImgScript(string newsUrl, string webpageImageFilePath)
    {
        ProcessStartInfo start = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"{RetrieveWebpageScriptPath} {newsUrl} {webpageImageFilePath}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        await Task.Run(() =>
        {
            using (Process process = Process.Start(start))
            {
                string error = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError(error);
                }
            }
        });
        LoadImageFromFile(webpageImageFilePath);
        SpecificNewsImageScrollView.verticalNormalizedPosition = 1f;
    }

    // New function to load specific image in original size
    public void LoadImageFromFile(string filePath)
    {
        StartCoroutine(LoadImageWithOriginalSize(filePath));
    }

    IEnumerator LoadImageWithOriginalSize(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            yield break;
        }

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);

        if (!texture.LoadImage(fileData))
        {
            Debug.LogError("Failed to load image from file.");
            yield break;
        }

        // Create a sprite from the texture
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        // Assign the sprite to the SpecificNewsImage Image component
        SpecificNewsImage.sprite = sprite;

        // Calculate the aspect ratio of the image
        float aspectRatio = (float)texture.width / texture.height;

        // Set the size of the image to the original size of the texture
        RectTransform rectTransform = SpecificNewsImage.GetComponent<RectTransform>();
        float imageWidth = 900;
        rectTransform.sizeDelta = new Vector2(imageWidth, imageWidth / aspectRatio);

    }
}
