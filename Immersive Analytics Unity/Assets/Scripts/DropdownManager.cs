using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropdownManager : MonoBehaviour
{
    public TMP_Dropdown xDimension;
    public TMP_Dropdown yDimension;
    public TMP_Dropdown zDimension;

    public void PopulateDropdown(List<string> options)
    {
        xDimension.ClearOptions();
        yDimension.ClearOptions();
        zDimension.ClearOptions();

        List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();
        dropdownOptions.Add(new TMP_Dropdown.OptionData("Undefined"));
        foreach (string option in options)
        {
            dropdownOptions.Add(new TMP_Dropdown.OptionData(option));
        }

        xDimension.AddOptions(dropdownOptions);
        yDimension.AddOptions(dropdownOptions);
        zDimension.AddOptions(dropdownOptions);
    }
}

