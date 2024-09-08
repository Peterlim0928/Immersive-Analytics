using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents a Treemap class that uses the squarify algorithm to visualize data
/// as rectangles on a canvas.
/// </summary>
public class Treemap
{
    private readonly double _canvasWidth;   // The width of the canvas
    private readonly double _canvasHeight;  // The height of the canvas
    
    private double _minArea;   // The minimum area of a rectangle
    private double _maxArea;   // The maximum area of a rectangle
    
    private List<RectangleData> _rectangles;   // The list of rectangles to be rendered
    
    private readonly Color _positiveColor = Color.green;   // The starting color for the gradient
    private readonly Color _negativeColor = Color.red;        // The ending color for the gradient
    
    /// <summary>
    /// Initializes a new instance of the Treemap class and updates the data.
    /// </summary>
    /// <param name="data">The input data to be visualised as a treemap.</param>
    /// <param name="canvasWidth">The width of the canvas</param>
    /// <param name="canvasHeight">The height of the canvas.</param>
    public Treemap(List<TreemapData> data, double canvasWidth, double canvasHeight)
    {
        _canvasWidth = canvasWidth;
        _canvasHeight = canvasHeight;
        
        Update(data);
    }
    
    /// <summary>
    /// Updates the treemap with new data.
    /// </summary>
    /// <param name="data">The input data to be visualised as a treemap.</param>
    public void Update(List<TreemapData> data)
    {
        Normalise(data, _canvasWidth * _canvasHeight);
        _rectangles = new List<RectangleData>(); // Reset Data

        _minArea = data.Min(e => e.normalisedData);
        _maxArea = data.Max(e => e.normalisedData);
        
        Squarify(data, new List<TreemapData>(), _canvasWidth, _canvasHeight);
    }

    /// <summary>
    /// Recursively squarifies the data into a treemap layout.
    /// </summary>
    /// <param name="children">The list of unplaced data points.</param>
    /// <param name="group">The current group of data points being processed.</param>
    /// <param name="w">The available width for the current group.</param>
    /// <param name="h">The available height for the current group.</param>
    private void Squarify(List<TreemapData> children, List<TreemapData> group, double w, double h)
    {
        Orientation orientation = w >= h ? Orientation.Vertical : Orientation.Horizontal;

        if (children.Count == 0)
        {
            StoreGroup(group, w, h, orientation);
            return;
        }

        TreemapData c = children[0];

        if (orientation == Orientation.Horizontal)
        {
            if (Worst(group, w) <= Worst(new List<TreemapData>(group) { c }, w))
            {
                Squarify(children.GetRange(1, children.Count - 1), new List<TreemapData>(group) { c }, w, h);
            }
            else
            {
                StoreGroup(group, w, h, orientation);
                Squarify(children, new List<TreemapData>(), w, h - ComputeSize(group, w));
            }
        }
        else
        {
            if (Worst(group, h) <= Worst(new List<TreemapData>(group) { c }, h))
            {
                Squarify(children.GetRange(1, children.Count - 1), new List<TreemapData>(group) { c }, w, h);
            }
            else
            {
                StoreGroup(group, w, h, orientation);
                Squarify(children, new List<TreemapData>(), w - ComputeSize(group, h), h);
            }
        }
    }

    /// <summary>
    /// Calculates the worst aspect ratio in a group of rectangles.
    /// </summary>
    /// <param name="group">The list of areas of rectangles in the group.</param>
    /// <param name="sideLength">The length of the side of the enclosing rectangle.</param>
    /// <returns>The worst aspect ratio in the group.</returns>
    private double Worst(List<TreemapData> group, double sideLength)
    {
        if (group.Count == 0) return 0;

        double smallest = group.Min(e => e.normalisedData);
        double height = group.Sum(e => e.normalisedData) / sideLength;
        double width = (smallest / group.Sum(e => e.normalisedData)) * sideLength;

        return Math.Min(height / width, width / height);
    }

    /// <summary>
    /// Computes the size of the group of rectangles based on the total area.
    /// </summary>
    /// <param name="group">The list of areas of rectangles in the group.</param>
    /// <param name="sideLength">The length of the side of the enclosing rectangle.</param>
    /// <returns>The size of the group.</returns>
    private double ComputeSize(List<TreemapData> group, double sideLength)
    {
        double totalArea = group.Sum(e => e.normalisedData);
        return totalArea / sideLength;
    }

    /// <summary>
    /// Normalizes the input data based on the total area available.
    /// </summary>
    /// <param name="data">The input data to be normalised.</param>
    /// <param name="total">The total area of the canvas.</param>
    private void Normalise(List<TreemapData> data, double total)
    {
        double sum = data.Sum(e => e.data);
        foreach (var d in data)
        {
            d.normalisedData = d.data / sum * total;
        }
    }
    
    /// <summary>
    /// Stores a group of rectangles with the specified orientation in the treemap layout.
    /// </summary>
    /// <param name="group">The group of data points being stored.</param>
    /// <param name="w">The width of the enclosing rectangle.</param>
    /// <param name="h">The height of the enclosing rectangle.</param>
    /// <param name="orientation">The orientation of the group (horizontal or vertical).</param>
    private void StoreGroup(List<TreemapData> group, double w, double h, Orientation orientation)
    {
        double totalArea = group.Sum(e => e.normalisedData);
        double xPos = (_canvasWidth - w) / _canvasWidth;
        double yPos = (_canvasHeight - h) / _canvasHeight;

        if (orientation == Orientation.Horizontal)
        {
            double rowHeight = totalArea / w;

            foreach (TreemapData data in group)
            {
                double rectWidth = (data.normalisedData / totalArea) * w;

                _rectangles.Add(new RectangleData
                {
                    data = data,
                    x = xPos,
                    y = yPos,
                    width = rectWidth / _canvasWidth,
                    height = rowHeight / _canvasHeight,
                    color = CalculateColor(data)
                });

                xPos += rectWidth / _canvasWidth;
            }
        }
        else if (orientation == Orientation.Vertical)
        {
            double columnWidth = totalArea / h;

            foreach (TreemapData data in group)
            {
                double rectHeight = (data.normalisedData / totalArea) * h;

                _rectangles.Add(new RectangleData
                {
                    data = data,
                    x = xPos,
                    y = yPos,
                    width = columnWidth / _canvasWidth,
                    height = rectHeight / _canvasHeight,
                    color = CalculateColor(data)
                });

                yPos += rectHeight / _canvasHeight;
            }
        }
    }
    
    /// <summary>
    /// Calculates the color for a rectangle based on its data.
    /// </summary>
    /// <param name="data">The data of the rectangle.</param>
    /// <returns>The color of the rectangle based on its data.</returns>
    private Color CalculateColor(TreemapData data)
    {
        double normalisedArea = (data.normalisedData - _minArea) / (_maxArea - _minArea);
        Color chosenColor = data.positive ? _positiveColor : _negativeColor;

        float r = (float)(chosenColor.r * (0.5 + 0.5 * normalisedArea));
        float g = (float)(chosenColor.g * (0.5 + 0.5 * normalisedArea));
        float b = (float)(chosenColor.b * (0.5 + 0.5 * normalisedArea));
        
        return new Color(r, g, b);
    }

    /// <summary>
    /// Returns the generated treemap data.
    /// </summary>
    /// <returns>Treemap rectangles.</returns>
    public List<RectangleData> GetRectangleData()
    {
        return _rectangles;
    }
}

/// <summary>
/// Represents data for a single rectangle in the treemap.
/// </summary>
public class RectangleData
{
    public TreemapData data { get; set; }     // The data value represented by this rectangle
    public double x { get; set; }        // The x-position of the rectangle on the canvas
    public double y { get; set; }        // The y-position of the rectangle on the canvas
    public double width { get; set; }    // The width of the rectangle
    public double height { get; set; }   // The height of the rectangle
    public Color color { get; set; }     // The color of the rectangle
}

/// <summary>
/// Represents the orientation of the rectangles in the treemap (horizontal or vertical).
/// </summary>
enum Orientation {
    Horizontal,
    Vertical
}
