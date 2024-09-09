using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VisualisationGraph : MonoBehaviour
{
    [Tooltip("The data source of this graph")]
    public DataSource dataSource; // The actual variable that stores the data
    
    [Tooltip("The material of the data points")]
    public Material datapointMaterial; // The material of the data points
    
    [Tooltip("The material of the data points when highlighted")]
    public Material highlightedDatapointMaterial; // The material of the data point when highlighted
    
    
    // Indexes for dropdown selections
    public int xAxisIndex;
    public int yAxisIndex;
    public int zAxisIndex;

    // Properties for easier access with both getters and setters
    public string xAxis
    {
        get
        {
            List<String> header = dataSource?.GetHeaders();
            if (xAxisIndex > 0 && xAxisIndex < header?.Count)
            {
                return header[xAxisIndex];
            }
            return null;
        }
        set
        {
            int index = dataSource?.GetHeaders().IndexOf(value) ?? -1;
            xAxisIndex = index;
        }
    }

    public string yAxis
    {
        get
        {
            List<String> header = dataSource?.GetHeaders();
            if (yAxisIndex > 0 && yAxisIndex < header?.Count)
            {
                return header[yAxisIndex];
            }
            return null;
        }
        set
        {
            int index = dataSource?.GetHeaders().IndexOf(value) ?? -1;
            yAxisIndex = index;
        }
    }

    public string zAxis
    {
        get
        {
            List<String> header = dataSource?.GetHeaders();
            if (zAxisIndex > 0 && zAxisIndex < header?.Count)
            {
                return header[zAxisIndex];
            }
            return null;
        }
        set
        {
            int index = dataSource?.GetHeaders().IndexOf(value) ?? -1;
            zAxisIndex = index;
        }
    }

    public void UpdateGraph()
    {
        // Find the child named "Datapoints"
        Transform dataPointsParent = transform.Find("Datapoints");
        if (dataPointsParent == null)
        {
            Debug.LogError("No child object named 'Datapoints' found.");
            return;
        }

        // Safely clear existing data points by iterating in reverse order
        for (int i = dataPointsParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(dataPointsParent.GetChild(i).gameObject);
        }

        // Get data from the DataSource based on current axes
        if (dataSource != null)
        {
            List<object> xData = dataSource.GetColumnData(xAxis);
            List<object> yData = dataSource.GetColumnData(yAxis);
            List<object> zData = dataSource.GetColumnData(zAxis);

            // Normalize the data to the range [0, 1]
            float xMin = FindMinValue(xData);
            float xMax = FindMaxValue(xData);
            float yMin = FindMinValue(yData);
            float yMax = FindMaxValue(yData);
            float zMin = FindMinValue(zData);
            float zMax = FindMaxValue(zData);
            
            // Debug.Log(String.Format("{0} {1} {2} {3} {4} {5}", xMin, xMax, yMin, yMax, zMin, zMax));

            // Count number of data points
            int count;
            if (xData != null)
            {
                count = xData.Count;
            } 
            else if (yData != null)
            {
                count = yData.Count;
            } 
            else if (zData != null)
            {
                count = zData.Count;
            }
            else
            {
                count = 0;
            }
            
            // Create new data points
            for (int i = 0; i < count; i++)
            {
                float xValue = xData != null ? NormalizeValue(xData[i], xMin, xMax) * 0.5f : 0f;
                float yValue = yData != null ? NormalizeValue(yData[i], yMin, yMax) * 0.5f : 0f;
                float zValue = zData != null ? NormalizeValue(zData[i], zMin, zMax) * 0.5f : 0f;

                // Instantiate a new sphere at the position defined by the normalized data point
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(dataPointsParent);
                sphere.transform.localPosition = new Vector3(xValue, yValue, zValue);
                sphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                
                // Set the material of the sphere
                sphere.GetComponent<Renderer>().material = datapointMaterial;
                sphere.AddComponent<XRSimpleInteractable>();
                sphere.AddComponent<OnHoverEnterEffect>().highlightMaterial = highlightedDatapointMaterial;
                Rigidbody sphereRigidbody = sphere.AddComponent<Rigidbody>();
                sphereRigidbody.useGravity = false;
                sphereRigidbody.isKinematic = true;
            }
        }
    }
    
    private float FindMinValue(List<object> data)
    {
        if (data == null)
        {
            return 0;
        }
        
        float minValue = float.MaxValue;
        foreach (var value in data)
        {
            if (ConvertToFloat(value, out float floatValue))
            {
                minValue = Mathf.Min(minValue, floatValue);
            }
        }
        return minValue;
    }

    private float FindMaxValue(List<object> data)
    {
        if (data == null)
        {
            return 0;
        }
        
        float maxValue = float.MinValue;
        foreach (var value in data)
        {
            if (ConvertToFloat(value, out float floatValue))
            {
                maxValue = Mathf.Max(maxValue, floatValue);
            }
        }
        return maxValue;
    }

    private float NormalizeValue(object value, float minValue, float maxValue)
    {
        if (Mathf.Abs(minValue - maxValue) < 0.00001f)
        {
            return 0f;
        }
        if (ConvertToFloat(value, out float floatValue))
        {
            return (floatValue - minValue) / (maxValue - minValue);
        }
        return 0f; // Return 0 if conversion fails, but ideally handle this case more robustly
    }
    
    private bool ConvertToFloat(object value, out float result)
    {
        if (value is float f)
        {
            result = f;
            return true;
        }
        if (value is int i)
        {
            result = i;
            return true;
        }
        if (value is double d)
        {
            result = (float)d;
            return true;
        }
        if (value is DateTime dt)
        {
            // Convert DateTime to float based on Ticks (or any other suitable conversion)
            result = (float)(dt.Ticks / (double)TimeSpan.TicksPerDay);
            return true;
        }

        result = 0;
        return false;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
