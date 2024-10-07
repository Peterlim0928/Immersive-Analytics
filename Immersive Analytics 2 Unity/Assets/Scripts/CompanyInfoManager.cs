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

public class CompanyInfoManager : MonoBehaviour
{
    public class CompanyInfo
    {
        public string companyName;
        public string companyStockCode;
        public string companyCountry;
        public string companySector;
        public string companyLongBusinessSummary;
        public string companyTotalRevenue;
    }

    private const string CompanyInfoScriptPath = "./Assets/Scripts/general_stock_info_script.py";

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PopulateCompanyInfo(string stockCode)
    {
        Debug.Log("Populating company info for stock code: " + stockCode);
        RunPythonScript(stockCode);
    }

    public void RunPythonScript(string stockCode)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "python";
        start.Arguments = $"{CompanyInfoScriptPath} {stockCode}";
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.CreateNoWindow = true;

        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();

                var companyInfoDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

                CompanyInfo companyInfo = new CompanyInfo
                {
                    companyName = companyInfoDict["Name"],
                    companyStockCode = companyInfoDict["Stock Code"],
                    companyCountry = companyInfoDict["Country"],
                    companySector = companyInfoDict["Sector"],
                    companyLongBusinessSummary = companyInfoDict["Long Business Summary"],
                    companyTotalRevenue = $"${companyInfoDict["Total Revenue"]}",
                };

                PopulateCompanyInfoUI(companyInfo);

            }
            string error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
            }
        }
    }

    public void PopulateCompanyInfoUI(CompanyInfo newCompanyInfo)
    {
        string stockLogo = $"https://assets.parqet.com/logos/symbol/{newCompanyInfo.companyStockCode}?format=jpg";

        GameObject.Find("CompanyName").GetComponent<TextMeshProUGUI>().text = newCompanyInfo.companyName;
        GameObject.Find("CompanyStockCode").GetComponent<TextMeshProUGUI>().text = newCompanyInfo.companyStockCode;
        GameObject.Find("CompanyCountry").GetComponent<TextMeshProUGUI>().text = newCompanyInfo.companyCountry;
        GameObject.Find("CompanySector").GetComponent<TextMeshProUGUI>().text = newCompanyInfo.companySector;
        GameObject.Find("CompanySummary").GetComponent<TextMeshProUGUI>().text = newCompanyInfo.companyLongBusinessSummary;
        GameObject.Find("CompanyRevenue").GetComponent<TextMeshProUGUI>().text = newCompanyInfo.companyTotalRevenue;
        StartCoroutine(LoadImageFromURL(stockLogo, GameObject.Find("CompanyLogo").GetComponent<Image>(), 70));

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