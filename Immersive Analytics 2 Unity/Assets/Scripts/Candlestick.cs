using System;
using System.Collections.Generic;

/// <summary>
/// Represents a candlestick chart and handles the parsing of candlestick data.
/// </summary>
public class Candlestick
{
    /// <summary>
    /// The width of the canvas where the chart will be drawn.
    /// </summary>
    private float canvasWidth { get; }

    /// <summary>
    /// The height of the canvas where the chart will be drawn.
    /// </summary>
    private float canvasHeight { get; }

    /// <summary>
    /// The minimum number of labels to be displayed on the Y-axis.
    /// </summary>
    private int minYLabels { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Candlestick"/> class.
    /// </summary>
    /// <param name="canvasWidth">The width of the canvas.</param>
    /// <param name="canvasHeight">The height of the canvas.</param>
    public Candlestick(float canvasWidth, float canvasHeight)
    {
        this.canvasWidth = canvasWidth;
        this.canvasHeight = canvasHeight;
        minYLabels = 10;
    }

    /// <summary>
    /// Parses raw candlestick data into a structured format suitable for rendering.
    /// </summary>
    /// <param name="rawData">A list of raw data points containing candlestick information.</param>
    /// <returns>A <see cref="CandlestickData"/> object containing parsed chart data and axis labels.</returns>
    public CandlestickData ParseCandlestickData(List<DataPoint> rawData)
    {
        int numPoints = rawData.Count;
        float dayWidth = canvasWidth / numPoints;

        // Find min and max values across all data for scaling
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        foreach (var dataPoint in rawData)
        {
            minValue = Math.Min(minValue, Math.Min(Math.Min(dataPoint.open, dataPoint.high), Math.Min(dataPoint.low, dataPoint.close)));
            maxValue = Math.Max(maxValue, Math.Max(Math.Max(dataPoint.open, dataPoint.high), Math.Max(dataPoint.low, dataPoint.close)));
        }

        // Generate X-axis labels at the start of each month
        List<Label> xLabels = new List<Label>();
        int lastMonth = -1;

        for (int i = 0; i < rawData.Count; i++)
        {
            var dataPoint = rawData[i];
            int currentMonth = dataPoint.time.Month;

            if (currentMonth != lastMonth)
            {
                xLabels.Add(new Label
                {
                    x = i * dayWidth + dayWidth / 2, // Position in the middle of the day
                    labelText = dataPoint.time.ToString("MMM yyyy") // e.g., 'Mar 2023'
                });
                lastMonth = currentMonth; // Update the last month processed
            }
        }

        // Y-axis labels remain unchanged, based on value range
        int numYLabels = Math.Max(minYLabels, (int)(canvasHeight / 80)) - 1;
        List<Label> yLabels = new List<Label>();

        for (int i = 0; i <= numYLabels; i++)
        {
            float y = canvasHeight - (canvasHeight / numYLabels) * i;
            float labelValue = minValue + (maxValue - minValue) * (i / (float)numYLabels);
            yLabels.Add(new Label
            {
                y = y,
                labelText = labelValue.ToString("F2")
            });
        }

        // Generate parsed candlestick data
        List<CandlestickDataPoint> parsedData = new List<CandlestickDataPoint>();

        foreach (var dataPoint in rawData)
        {
            float xCenter = rawData.IndexOf(dataPoint) * dayWidth + dayWidth / 2;
            Func<float, float> scaleY = value =>
                canvasHeight - ((value - minValue) / (maxValue - minValue)) * canvasHeight;

            float openY = scaleY(dataPoint.open);
            float closeY = scaleY(dataPoint.close);
            float highY = scaleY(dataPoint.high);
            float lowY = scaleY(dataPoint.low);

            parsedData.Add(new CandlestickDataPoint
            {
                xCenter = xCenter,
                openY = openY,
                closeY = closeY,
                highY = highY,
                lowY = lowY,
                isBullish = dataPoint.close > dataPoint.open,
                dayWidth = dayWidth * 0.8f
            });
        }

        return new CandlestickData
        {
            graphData = new GraphData
            {
                width = canvasWidth,
                height = canvasHeight,
                minValue = minValue,
                maxValue = maxValue
            },
            chartData = parsedData,
            xAxis = xLabels,
            yAxis = yLabels
        };
    }
}

/// <summary>
/// Represents a single data point in the candlestick chart.
/// </summary>
public class DataPoint
{
    /// <summary>
    /// The timestamp of the data point.
    /// </summary>
    public DateTime time { get; set; }

    /// <summary>
    /// The opening price of the data point.
    /// </summary>
    public float open { get; set; }

    /// <summary>
    /// The highest price of the data point.
    /// </summary>
    public float high { get; set; }

    /// <summary>
    /// The lowest price of the data point.
    /// </summary>
    public float low { get; set; }

    /// <summary>
    /// The closing price of the data point.
    /// </summary>
    public float close { get; set; }
}

/// <summary>
/// Contains the data needed to render a candlestick chart.
/// </summary>
public class CandlestickData
{
    /// <summary>
    /// The graph's dimensions and value range.
    /// </summary>
    public GraphData graphData { get; set; }

    /// <summary>
    /// The parsed data points for the candlestick chart.
    /// </summary>
    public List<CandlestickDataPoint> chartData { get; set; }

    /// <summary>
    /// The X-axis labels.
    /// </summary>
    public List<Label> xAxis { get; set; }

    /// <summary>
    /// The Y-axis labels.
    /// </summary>
    public List<Label> yAxis { get; set; }
}

/// <summary>
/// Contains the dimensions and value range of the chart.
/// </summary>
public class GraphData
{
    /// <summary>
    /// The width of the canvas.
    /// </summary>
    public float width { get; set; }

    /// <summary>
    /// The height of the canvas.
    /// </summary>
    public float height { get; set; }

    /// <summary>
    /// The minimum value on the Y-axis.
    /// </summary>
    public float minValue { get; set; }

    /// <summary>
    /// The maximum value on the Y-axis.
    /// </summary>
    public float maxValue { get; set; }
}

/// <summary>
/// Represents a single data point on the candlestick chart after parsing.
/// </summary>
public class CandlestickDataPoint
{
    /// <summary>
    /// The X position for drawing the middle of the candle.
    /// </summary>
    public float xCenter { get; set; }

    /// <summary>
    /// The Y position for the open price.
    /// </summary>
    public float openY { get; set; }

    /// <summary>
    /// The Y position for the close price.
    /// </summary>
    public float closeY { get; set; }

    /// <summary>
    /// The Y position for the high price.
    /// </summary>
    public float highY { get; set; }

    /// <summary>
    /// The Y position for the low price.
    /// </summary>
    public float lowY { get; set; }

    /// <summary>
    /// Indicates whether the candle is bullish (close > open).
    /// </summary>
    public bool isBullish { get; set; }

    /// <summary>
    /// The width of the candle body.
    /// </summary>
    public float dayWidth { get; set; }
}

/// <summary>
/// Represents a label on the X or Y axis of the chart.
/// </summary>
public class Label
{
    /// <summary>
    /// The X position of the label on the canvas.
    /// </summary>
    public float x { get; set; }

    /// <summary>
    /// The Y position of the label on the canvas.
    /// </summary>
    public float y { get; set; }

    /// <summary>
    /// The text of the label.
    /// </summary>
    public string labelText { get; set; }
}
