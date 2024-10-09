using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Debug = UnityEngine.Debug;

public class TreemapController : MonoBehaviour
{
    private List<TreemapData> _parsedData;
    private Treemap _treemap;
    private GameObject _parent;
    
    private double _canvasWidth;
    private double _canvasHeight;
    
    private const string TreeDataScriptPath = "./Assets/Scripts/script_tree_map.py";
    
    // Start is called before the first frame update
    void Start()
    {
        _parent = transform.Find("CanvasBackground").Find("Treemap").GameObject();
        RectTransform treemapTransform = _parent.GetComponent<RectTransform>();
        _canvasWidth = treemapTransform.rect.width;
        _canvasHeight = treemapTransform.rect.height;

        _parsedData = new List<TreemapData>();
        RunPythonScript();
        
        _treemap = new Treemap(_parsedData, _canvasWidth, _canvasHeight);
        RenderData(_treemap.GetRectangleData());
    }
    
    private void RunPythonScript()
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "python";
        start.Arguments = $"{TreeDataScriptPath}";
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.CreateNoWindow = true;

        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                var treemapDataDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, double>>>(result);
                
                // For each industry
                foreach (KeyValuePair<string, Dictionary<string, double>> outerTreeData in treemapDataDict)
                {
                    List<TreemapData> child = new List<TreemapData>();

                    // For each company
                    foreach (KeyValuePair<string, double> innerTreeData in outerTreeData.Value)
                    {
                        TreemapData fieldData = new TreemapData();

                        double value = innerTreeData.Value;
                        fieldData.data = Math.Abs(value);
                        fieldData.stockCode = innerTreeData.Key;
                        fieldData.positive = value > 0;

                        if (fieldData.data > 0)
                        {
                            child.Add(fieldData);
                        }
                    }
                    
                    // Sort in descending order
                    child.Sort((x, y) => y.data.CompareTo(x.data));

                    TreemapData parent = new TreemapData();

                    double tempData = child.Average(item => item.data * (item.positive ? 1 : -1));

                    parent.child = child;
                    parent.data = Math.Abs(tempData);
                    parent.stockCode = outerTreeData.Key;
                    parent.positive = tempData > 0;
                    
                    _parsedData.Add(parent);
                }
                
                // Sort in descending order
                _parsedData.Sort((x, y) => y.data.CompareTo(x.data));
            }
            string error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
            }
        }
    }

    private void RenderData(List<RectangleData> renderDataList)
    {
        double maxHeight = 50;
        double minHeight = 1;
        double heightRange = maxHeight - minHeight;
        
        // Find the min and max data for scaling
        double minData = renderDataList.Min(e => e.data.normalisedData);
        double maxData = renderDataList.Max(e => e.data.normalisedData);
        
        // Delete old cubes
        foreach (Transform cubeChild in _parent.transform)
        {
            Destroy(cubeChild.gameObject);
        }
        
        // For each rectangle data
        foreach (var renderData in renderDataList)
        {
            // Create a cube
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(_parent.transform);
            
            // Adjust scale (set size)
            float xScale = (float)(renderData.width * _canvasWidth);
            float yScale;
            if (maxData - minData < 0.0001)
            {
                yScale = 1;
            }
            else
            {
                yScale = (float)((renderData.data.normalisedData - minData) / (maxData - minData) * heightRange + minHeight); // [1, 50]
            }
            float zScale = (float)(renderData.height * _canvasHeight);
            cube.transform.localScale = new Vector3(xScale - 1f, yScale, zScale - 1f); // Offset to give space
            
            // Adjust position (set location)
            float xPos = (float)(renderData.x * _canvasWidth - _canvasWidth / 2 + renderData.width * _canvasWidth / 2);
            float yPos = (float)(renderData.y * _canvasHeight - _canvasHeight / 2 + renderData.height * _canvasHeight / 2);
            float zPos = -(yScale / 2);
            cube.transform.localPosition = new Vector3(xPos, yPos, zPos);
            
            // Apply colours, transparency
            Renderer cubeRenderer = cube.GetComponent<Renderer>();

            Material material = cubeRenderer.material;
            material.color = renderData.color;
            material.color = new Color(material.color.r, material.color.g, material.color.b, 0.5f); // Set transparency (Not working)
            
            cubeRenderer.material = material;
            
            // Create highlighted version of the material
            Material highlightedMaterial = new Material(material);
            highlightedMaterial.EnableKeyword("_EMISSION");
            highlightedMaterial.SetColor("_EmissionColor", material.color);
            
            // Add relevant components
            XRSimpleInteractable interactable = cube.AddComponent<XRSimpleInteractable>();
            cube.AddComponent<OnHoverEnterEffect>().highlightMaterial = highlightedMaterial;
            Rigidbody cubeRigidbody = cube.AddComponent<Rigidbody>();
            cubeRigidbody.useGravity = false;
            cubeRigidbody.isKinematic = true;
            
            // Add text on the cube
            GameObject textObject = new GameObject("CubeLabel");
            textObject.transform.SetParent(cube.transform);
        
            // Add TextMesh component for displaying text
            TextMeshPro textMesh = textObject.AddComponent<TextMeshPro>();
            textMesh.rectTransform.sizeDelta = new Vector2(100, 20);
            textMesh.text = $"{renderData.data.stockCode}\n{(renderData.data.positive ? "+" : "-")}{Math.Round(renderData.data.data, 3)}%"; // Display the data value as text
            textMesh.fontSize = 72;
            textMesh.color = Color.black; // Set text color
            textMesh.alignment = TextAlignmentOptions.Center;
        
            // Adjust text position
            textObject.transform.localPosition = new Vector3(0, (float)(maxHeight / 100 + 0.1), 0); // Place it slightly above the cube
            textObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
            double scaleMultiplier = xScale > zScale ? zScale / xScale : xScale / zScale;
            double xTextScale = xScale > zScale ? scaleMultiplier : 1;
            double yTextScale = zScale > xScale ? scaleMultiplier : 1;
            textObject.transform.localScale = new Vector3((float)xTextScale * 0.01f, (float)yTextScale * 0.01f, 0.01f);

            interactable.firstSelectEntered = new SelectEnterEvent();
            interactable.firstSelectEntered.AddListener(_ => CubeOnPressed(renderData));
        }
    }

    public void CubeOnPressed(RectangleData renderData)
    {
        if (renderData.data.child != null)
        {
            _treemap = new Treemap(renderData.data.child, _canvasWidth, _canvasHeight);
            RenderData(_treemap.GetRectangleData());
        }
        else
        {
            Debug.Log("No child data to render");
        }
    }

    public void ReturnToHome()
    {
        _treemap = new Treemap(_parsedData, _canvasWidth, _canvasHeight);
        RenderData(_treemap.GetRectangleData());
    }
}

public class TreemapData
{
    public List<TreemapData> child { get; set; }
    public string stockCode { get; set; }
    public double data { get; set; }
    public bool positive { get; set; }
    public double normalisedData { get; set; }
}
