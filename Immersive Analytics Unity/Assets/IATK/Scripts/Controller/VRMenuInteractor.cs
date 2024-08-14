using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace IATK
{

    public class VRMenuInteractor : MonoBehaviour
    {

        public Visualisation visualisation;
        public Slider slider;
        public Dropdown X_AxisDropDown;
        public Dropdown Y_AxisDropDown;
        public Dropdown Z_AxisDropDown;
        public Dropdown SizeAttributeDropDown;
        public TMP_Dropdown DataSourceDropDown;
        
        /**
        * Sliders for filtering the data
        */
        public Slider X_Axis_SliderMinFilter;
        public Slider X_Axis_SliderMaxFilter;

        public Slider Y_Axis_SliderMinFilter;
        public Slider Y_Axis_SliderMaxFilter;

        public Slider Z_Axis_SliderMinFilter;
        public Slider Z_Axis_SliderMaxFilter;

        /**
         * Input Fields for Stock Options
         */
        public TMP_InputField Stock_Code_Option;
        public TMP_Dropdown Stock_Time_Interval_DropDown;
        public Button Stock_Search_Button;

        //holds the string names of the data attributes
        private List<string> DataAttributesNames;

        // stores the list of data source options
        private List<string> DataSourceNames;
        
        // stores the list of stock time options
        private List<string> StockTimeOptions;
        
        // mapping the stock time options
        private Dictionary<string, string> stockOptionsDictionary = new Dictionary<string, string>()
        {
            {"1 Day", "1d"},
            {"5 Days", "5d"},
            {"1 Month", "1mo"},
            {"3 Months", "3mo"},
            {"6 Months", "6mo"},
            {"1 Year", "1y"},
            {"2 Years", "2y"},
            {"5 Years", "5y"},
            {"10 Years", "10y"},
            {"Year To Date", "ytd"},
            {"All", "max"},
        };

        // Use this for initialization
        void Start()
        {
            // Retrieve Data Source Names & Populate Dropdown UI 
            DataSourceNames = GetDataSourceNames();
            DataSourceDropDown.AddOptions(DataSourceNames);
            
            // Retrieve Stock Time Options & Populate Dropdown UI 
            StockTimeOptions = GetStockTimeOptions();
            Stock_Time_Interval_DropDown.AddOptions(StockTimeOptions);
            
                
            DataAttributesNames = GetAttributesList();
            PopulateAttributeDropdowns();
            SetInitialDropDownValues();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        private List<string> GetDataSourceNames()
        {
            return new List<string> {
                "msft_1y.csv",
                "cereal.csv"
            };
        }

        private List<string> GetStockTimeOptions()
        {
            return stockOptionsDictionary.Keys.ToList();
        }

        private List<string> GetAttributesList()
        {
            List<string> dimensions = new List<string>();
            dimensions.Add("Undefined");
            for (int i = 0; i < visualisation.dataSource.DimensionCount; ++i)
            {
                dimensions.Add(visualisation.dataSource[i].Identifier);
            }
            return dimensions;
        }

        private void PopulateAttributeDropdowns()
        {
            X_AxisDropDown.AddOptions(DataAttributesNames);
            Y_AxisDropDown.AddOptions(DataAttributesNames);
            Z_AxisDropDown.AddOptions(DataAttributesNames);
            SizeAttributeDropDown.AddOptions(DataAttributesNames);
        }

        private void SetInitialDropDownValues()
        {
            X_AxisDropDown.value = DataAttributesNames.IndexOf(visualisation.xDimension.Attribute);
            Y_AxisDropDown.value = DataAttributesNames.IndexOf(visualisation.yDimension.Attribute);
            Z_AxisDropDown.value = DataAttributesNames.IndexOf(visualisation.zDimension.Attribute);

            SizeAttributeDropDown.value = DataAttributesNames.IndexOf(visualisation.sizeDimension);

        }

        public void ValidateX_AxisDropdown()
        {
            if (visualisation != null)
            {
                visualisation.xDimension = DataAttributesNames[X_AxisDropDown.value];
                visualisation.updateViewProperties(AbstractVisualisation.PropertyType.X);
            }
        }

        public void ValidateY_AxisDropdown()
        {
            if (visualisation != null)
            {
                visualisation.yDimension = DataAttributesNames[Y_AxisDropDown.value];
                visualisation.updateViewProperties(AbstractVisualisation.PropertyType.Y);

            }
        }

        public void ValidateZ_AxisDropdown()
        {
            if (visualisation != null)
            {
                visualisation.zDimension = DataAttributesNames[Z_AxisDropDown.value];
                visualisation.updateViewProperties(AbstractVisualisation.PropertyType.Z);

            }
        }

        public void ValidateSizeChangeSlider()
        {
            if (visualisation != null)
            {
                visualisation.size = slider.value;
                visualisation.updateViewProperties(AbstractVisualisation.PropertyType.SizeValues);
            }
        }

        public void ValidateAttributeSizeDropDown()
        {
            if (visualisation != null)
            {
                visualisation.sizeDimension = DataAttributesNames[SizeAttributeDropDown.value];
                visualisation.updateViewProperties(AbstractVisualisation.PropertyType.Size);
            }
        }

        public void ValidateXAxisSliderMinFilter()
        {
            if (visualisation != null)
            {
                visualisation.xDimension.minFilter = X_Axis_SliderMinFilter.value;
                visualisation.updateProperties();
            }
        }

        public void ValidateXAxisSliderMaxFilter()
        {
            if (visualisation != null)
            {
                visualisation.xDimension.maxFilter = X_Axis_SliderMaxFilter.value;
                visualisation.updateProperties();
            }
        }

        public void ValidateYAxisSliderMinFilter()
        {
            if (visualisation != null)
            {
                visualisation.yDimension.minFilter = Y_Axis_SliderMinFilter.value;
                visualisation.updateProperties();
            }
        }
        
        public void ValidateYAxisSliderMaxFilter()
        {
            if (visualisation != null)
            {
                visualisation.yDimension.maxFilter = Y_Axis_SliderMaxFilter.value;
                visualisation.updateProperties();
            }
        }

        public void ValidateZAxisSliderMinFilter()
        {
            if (visualisation != null)
            {
                visualisation.zDimension.minFilter = Z_Axis_SliderMinFilter.value;
                visualisation.updateProperties();
            }
        }

        public void ValidateZAxisSliderMaxFilter()
        {
            if (visualisation != null)
            {
                visualisation.zDimension.maxFilter = Z_Axis_SliderMaxFilter.value;
                visualisation.updateProperties();
            }
        }

        public void ReadStockOptions()
        {
            if (visualisation != null)
            {
                int index = Stock_Time_Interval_DropDown.value;
                string chosenTime = stockOptionsDictionary.ElementAt(index).Value;                
                Debug.Log(String.Format("Stock Code: {0} | Stock Time Option: {1}", Stock_Code_Option.text, chosenTime));
                
                // Run the Download Python Script
                string pythonScriptPath = "Assets/IATK/Scripts/Controller/script.py";
                string pythonArgs = $"{Stock_Code_Option.text} {chosenTime}";
                RunPythonScript(pythonScriptPath, pythonArgs);
            }
        }

        public void RunPythonScript(string scriptPath, string arguments)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "python";
            start.Arguments = $"{scriptPath} {arguments}";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.CreateNoWindow = true;

            using (Process process = Process.Start(start))
            {
                using (System.IO.StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Debug.Log(result);
                }
                string error = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError(error);
                }
            }
        }
    }
}
