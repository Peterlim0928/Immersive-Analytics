using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VisualisationGraph))]
public class VisualisationGraphEditor : Editor
{
    private VisualisationGraph _visualisationGraph;
    private SerializedProperty _dataSourceProperty;

    private void OnEnable()
    {
        _visualisationGraph = (VisualisationGraph)target;
        _dataSourceProperty = serializedObject.FindProperty("dataSource");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Display the DataSource field
        EditorGUILayout.PropertyField(_dataSourceProperty);

        if (_visualisationGraph.dataSource != null && _visualisationGraph.dataSource.data != null)
        {
            // Get headers from DataSource
            var headers = new[] { "Undefined" }.Concat(_visualisationGraph.dataSource.GetHeaders()).ToArray();

            int previousXIndex = _visualisationGraph.xAxisIndex;
            int previousYIndex = _visualisationGraph.yAxisIndex;
            int previousZIndex = _visualisationGraph.zAxisIndex;

            if (headers.Length > 0)
            {
                _visualisationGraph.xAxisIndex = EditorGUILayout.Popup("X Axis", _visualisationGraph.xAxisIndex, headers);
                _visualisationGraph.yAxisIndex = EditorGUILayout.Popup("Y Axis", _visualisationGraph.yAxisIndex, headers);
                _visualisationGraph.zAxisIndex = EditorGUILayout.Popup("Z Axis", _visualisationGraph.zAxisIndex, headers);
                
                if (_visualisationGraph.xAxisIndex != previousXIndex ||
                    _visualisationGraph.yAxisIndex != previousYIndex ||
                    _visualisationGraph.zAxisIndex != previousZIndex)
                {
                    // Dropdown changed, implement update graph here
                    Debug.Log("Dropdown changed");
                    _visualisationGraph.UpdateGraph();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No headers available in DataSource.");
            }
        }
        else
        {
            EditorGUILayout.LabelField("No DataSource assigned.");
        }

        serializedObject.ApplyModifiedProperties();
    }
}