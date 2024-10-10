using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class VRMenuInteractor : MonoBehaviour
{

    public VisualisationGraph visualisationGraph;
    public Slider slider;
    public Dropdown xAxisDropDown;
    public Dropdown yAxisDropDown;
    public Dropdown zAxisDropDown;
    public Dropdown sizeAttributeDropDown;
    
    /**
    * Sliders for filtering the data
    */
    public Slider xAxisSliderMinFilter;
    public Slider xAxisSliderMaxFilter;

    public Slider yAxisSliderMinFilter;
    public Slider yAxisSliderMaxFilter;

    public Slider zAxisSliderMinFilter;
    public Slider zAxisSliderMaxFilter;

    // Holds the string names of the data attributes
    private List<string> _dataAttributesNames;
    
    // Use this for initialization
    void Start()
    {
        UpdateDropdown();
    }

    public void UpdateDropdown()
    {
        _dataAttributesNames = new[] { "Undefined" }.Concat(visualisationGraph.dataSource.GetHeaders()).ToList();
        PopulateAttributeDropdowns();
        SetInitialDropDownValues();
    }
    
    public void PopulateAttributeDropdowns()
    {
        xAxisDropDown.AddOptions(_dataAttributesNames);
        yAxisDropDown.AddOptions(_dataAttributesNames);
        zAxisDropDown.AddOptions(_dataAttributesNames);
        // sizeAttributeDropDown.AddOptions(_dataAttributesNames);
    }

    public void SetInitialDropDownValues()
    {
        xAxisDropDown.value = visualisationGraph.xAxisIndex;
        yAxisDropDown.value = visualisationGraph.yAxisIndex;
        zAxisDropDown.value = visualisationGraph.zAxisIndex;
        // sizeAttributeDropDown.value = _dataAttributesNames.IndexOf(visualisationGraph.sizeDimension);

    }

    public void ValidateX_AxisDropdown()
    {
        if (visualisationGraph != null)
        {
            visualisationGraph.xAxisIndex = xAxisDropDown.value;
            visualisationGraph.UpdateGraph();
        }
    }

    public void ValidateY_AxisDropdown()
    {
        if (visualisationGraph != null)
        {
            visualisationGraph.yAxisIndex = yAxisDropDown.value;
            visualisationGraph.UpdateGraph();
        }
    }

    public void ValidateZ_AxisDropdown()
    {
        if (visualisationGraph != null)
        {
            visualisationGraph.zAxisIndex = zAxisDropDown.value;
            visualisationGraph.UpdateGraph();
        }
    }

    // public void ValidateSizeChangeSlider()
    // {
    //     if (visualisationGraph != null)
    //     {
    //         visualisationGraph.size = slider.value;
    //         visualisationGraph.updateViewProperties(AbstractVisualisation.PropertyType.SizeValues);
    //     }
    // }

    // public void ValidateAttributeSizeDropDown()
    // {
    //     if (visualisationGraph != null)
    //     {
    //         visualisationGraph.sizeDimension = _dataAttributesNames[sizeAttributeDropDown.value];
    //         visualisationGraph.updateViewProperties(AbstractVisualisation.PropertyType.Size);
    //     }
    // }

    // public void ValidateXAxisSliderMinFilter()
    // {
    //     if (visualisationGraph != null)
    //     {
    //         visualisationGraph.xDimension.minFilter = xAxisSliderMinFilter.value;
    //         visualisationGraph.updateProperties();
    //     }
    // }
    //
    // public void ValidateXAxisSliderMaxFilter()
    // {
    //     if (visualisationGraph != null)
    //     {
    //         visualisationGraph.xDimension.maxFilter = xAxisSliderMaxFilter.value;
    //         visualisationGraph.updateProperties();
    //     }
    // }
    //
    // public void ValidateYAxisSliderMinFilter()
    // {
    //     if (visualisationGraph != null)
    //     {
    //         visualisationGraph.yDimension.minFilter = yAxisSliderMinFilter.value;
    //         visualisationGraph.updateProperties();
    //     }
    // }
    //
    // public void ValidateYAxisSliderMaxFilter()
    // {
    //     if (visualisationGraph != null)
    //     {
    //         visualisationGraph.yDimension.maxFilter = yAxisSliderMaxFilter.value;
    //         visualisationGraph.updateProperties();
    //     }
    // }
    //
    // public void ValidateZAxisSliderMinFilter()
    // {
    //     if (visualisationGraph != null)
    //     {
    //         visualisationGraph.zDimension.minFilter = zAxisSliderMinFilter.value;
    //         visualisationGraph.updateProperties();
    //     }
    // }
    //
    // public void ValidateZAxisSliderMaxFilter()
    // {
    //     if (visualisationGraph != null)
    //     {
    //         visualisationGraph.zDimension.maxFilter = zAxisSliderMaxFilter.value;
    //         visualisationGraph.updateProperties();
    //     }
    // }
}
