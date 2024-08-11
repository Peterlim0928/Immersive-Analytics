using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        //holds the string names of the data attributes
        private List<string> DataAttributesNames;

        // stores the list of data source options
        private List<string> DataSourceNames;

        // Use this for initialization
        void Start()
        {
            // Retrieve Data Source Names
            DataSourceNames = GetDataSourceNames();
            DataSourceDropDown.AddOptions(DataSourceNames);
            
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
    }
}
