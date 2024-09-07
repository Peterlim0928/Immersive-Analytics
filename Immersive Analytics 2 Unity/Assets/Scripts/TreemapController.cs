using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TreemapController : MonoBehaviour
{
    public TextAsset data;
    private List<TreemapData> _parsedData;
    private Treemap _treemap;
    private GameObject _parent;
    
    private double _canvasWidth;
    private double _canvasHeight;
    
    // Start is called before the first frame update
    void Start()
    {
        _parent = transform.Find("CanvasBackground").Find("Treemap").GameObject();
        RectTransform treemapTransform = _parent.GetComponent<RectTransform>();
        _canvasWidth = treemapTransform.rect.width;
        _canvasHeight = treemapTransform.rect.height;
        
        ParseCsv();
        _treemap = new Treemap(_parsedData, _canvasWidth, _canvasHeight);
        
        List<RectangleData> processedData = _treemap.GetRectangleData();
        RenderData(processedData);
    }
    
    private void ParseCsv()
    {
        if (data == null)
            return;
        
        // Clear any previously parsed data
        _parsedData = new List<TreemapData>();

        // Read the CSV file line by line
        StringReader reader = new StringReader(data.text);
        
        // Read header and store as key
        string[] headers = reader.ReadLine()?.Split(',');
        if (headers == null)
        {
            Debug.LogWarning("CSV File is empty");
            return;
        }

        // Read the rest of the file and store as value (List)
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] values = line.Split(',');
            TreemapData currentData = new TreemapData();
            
            for (int i = 0; i < headers.Length; i++)
            {
                switch (i)
                {
                    case 0: // Stock Code
                        currentData.stockCode = values[i];
                        break;
                    case 1: // Data
                        currentData.data = double.Parse(values[i]);
                        break;
                }
            }
            
            _parsedData.Add(currentData);
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
        
        // For each rectangle data
        foreach (var renderData in renderDataList)
        {
            // Create a cube
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(_parent.transform);
            
            // Adjust scale (set size)
            float xScale = (float)(renderData.width * _canvasWidth);
            float yScale = (float)((renderData.data.normalisedData - minData) / (maxData - minData) * heightRange + minHeight); // [1, 50]
            float zScale = (float)(renderData.height * _canvasHeight);
            cube.transform.localScale = new Vector3(xScale, yScale, zScale); // Scale along the y-axis for 3D effect
            
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
            cube.AddComponent<OnHoverEnterEffect>().highlightMaterial = highlightedMaterial;
            cube.AddComponent<XRGrabInteractable>();
            cube.GetComponent<Rigidbody>().useGravity = false;
            cube.GetComponent<Rigidbody>().isKinematic = true;
            
            // Add text on the cube
            GameObject textObject = new GameObject("CubeLabel");
            textObject.transform.SetParent(cube.transform);
        
            // Add TextMesh component for displaying text
            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = renderData.data.stockCode; // Display the data value as text
            textMesh.fontSize = 72;
            textMesh.color = Color.black; // Set text color
            textMesh.anchor = TextAnchor.MiddleCenter;
        
            // Adjust text position
            textObject.transform.localPosition = new Vector3(0, yScale / 100 + 0.1f, 0); // Place it slightly above the cube
            textObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
            textObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            // TODO: Add OnClick events
            // var clickable = cube.AddComponent<Clickable>();
            // clickable.onClick = () => OnRectangleClicked(renderData);
        }
    }
}

public class TreemapData
{
    public string stockCode { get; set; }
    public double data { get; set; }
    public double normalisedData { get; set; }
}
