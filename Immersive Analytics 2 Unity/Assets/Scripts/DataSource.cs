using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataSource : MonoBehaviour
{
    [Tooltip("Text asset containing the data")]
    public TextAsset data;
    private Dictionary<string, List<object>> _parsedData; // List to store the parsed data
    
    public List<string> GetHeaders()
    {
        if (data == null)
            return null;
        if (_parsedData == null)
            ParseCsv();
        return new List<string>(_parsedData.Keys);
    }

    public List<object> GetColumnData(string key)
    {
        if (key == null)
        {
            return null;
        }
        return _parsedData[key];
    }

    private void OnValidate()
    {
        if (data != null)
        {
            // Debug.Log("Data has been updated!");
            ParseCsv(); // Reparse the CSV data whenever the data field is updated
        }
    }

    public void ParseCsv()
    {
        if (data == null)
            return;
        
        // Clear any previously parsed data
        _parsedData = new Dictionary<string, List<object>>();

        // Read the CSV file line by line
        StringReader reader = new StringReader(data.text);
        
        // Read header and store as key
        string[] headers = reader.ReadLine()?.Split(',');
        if (headers == null)
        {
            Debug.LogWarning("CSV File is empty");
            return;
        }
        
        foreach (var header in headers)
        {
            _parsedData.Add(header, new List<object>());
        }

        // Read the rest of the file and store as value (List)
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] values = line.Split(',');

            for (int i = 0; i < headers.Length; i++)
            {
                // Try to convert the value to a double
                if (double.TryParse(values[i], out double parsedDoubleValue))
                {
                    _parsedData[headers[i]].Add(parsedDoubleValue);
                }
                // Try to convert the value to a DateTime
                else if (DateTime.TryParse(values[i], out DateTime parsedDateValue))
                {
                    _parsedData[headers[i]].Add(parsedDateValue);
                }
                // Preserve string
                else
                {
                    _parsedData[headers[i]].Add(values[i]);
                }
            }
        }
    }
}
