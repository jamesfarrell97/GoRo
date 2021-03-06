using System.Collections.Generic;
using System.Linq;
using System;

using UnityEngine.UI;
using UnityEngine;
using TMPro;

using Random = UnityEngine.Random;

// Code referenced: https://www.youtube.com/playlist?list=PLzDRvYVwl53v5ur4GluoabyckImZz3TVQ
//
//
//
public class StatsGraph : MonoBehaviour
{
    public static StatsGraph Instance;

    private const int MAX_POINTS = 30;
    private const int X_SEPERATOR = 5;

    [SerializeField] private Transform graph;
    [SerializeField] private TMP_Text graphLabel;

    [SerializeField] private Image graphPoint;
    [SerializeField] private Image graphConnection;

    [SerializeField] private RectTransform labelTemplateX;
    [SerializeField] private RectTransform labelTemplateY;

    [SerializeField] private RectTransform dashTemplateX;
    [SerializeField] private RectTransform dashTemplateY;

    private RectTransform graph1;
    private RectTransform graph2;
    private RectTransform graph3;
    private RectTransform graph4;
    private RectTransform graph5;

    private List<RectTransform> graphs;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;

        graph1 = graph.Find("Graph 1").GetComponent<RectTransform>();
        graph2 = graph.Find("Graph 2").GetComponent<RectTransform>();
        graph3 = graph.Find("Graph 3").GetComponent<RectTransform>();
        graph4 = graph.Find("Graph 4").GetComponent<RectTransform>();
        graph5 = graph.Find("Graph 5").GetComponent<RectTransform>();

        graphs = new List<RectTransform>() { graph1, graph2, graph3, graph4, graph5 };
    }

    private List<float> data1 = new List<float>();
    private List<float> data2 = new List<float>();
    private List<float> data3 = new List<float>();
    private List<float> data4 = new List<float>();
    private List<float> data5 = new List<float>();

    private int count1 = 0;
    private int count2 = 0;
    private int count3 = 0;
    private int count4 = 0;
    private int count5 = 0;

    public void UpdateGraph()
    {
        count1 = (MAX_POINTS < StatsManager.DragFactorData.Count) ? MAX_POINTS : StatsManager.DragFactorData.Count;
        data1 = StatsManager.DragFactorData.ToList().GetRange(
            StatsManager.DragFactorData.Count - count1,
            count1
        );

        count2 = (MAX_POINTS < StatsManager.SpeedData.Count) ? MAX_POINTS : StatsManager.SpeedData.Count;
        data2 = StatsManager.SpeedData.ToList().GetRange(
            StatsManager.SpeedData.Count - count2,
            count2
        );

        count3 = (MAX_POINTS < StatsManager.StrokePowerData.Count) ? MAX_POINTS : StatsManager.StrokePowerData.Count;
        data3 = StatsManager.StrokePowerData.ToList().GetRange(
            StatsManager.StrokePowerData.Count - count3, 
            count3
        );

        count4 = (MAX_POINTS < StatsManager.SplitAvgPaceData.Count) ? MAX_POINTS : StatsManager.SplitTimeData.Count;
        data4 = StatsManager.SplitAvgPaceData.ToList().GetRange(
            StatsManager.SplitAvgPaceData.Count - count4, 
            count4
        );

        count5 = (MAX_POINTS < StatsManager.StrokesPerMinData.Count) ? MAX_POINTS : StatsManager.StrokesPerMinData.Count;
        data5 = StatsManager.StrokesPerMinData.ToList().GetRange(
            StatsManager.StrokesPerMinData.Count - count5,
            count5
        );

        ClearGraph(graph1);
        ClearGraph(graph2);
        ClearGraph(graph3);
        ClearGraph(graph4);
        ClearGraph(graph5);

        PlotDataOnGraph(
            data1, 
            graph1,
            Color.green,
            (int i) => i + " s",
            (float f) => Mathf.RoundToInt(f).ToString()
        );

        PlotDataOnGraph(
            data2, 
            graph2,
            Color.magenta,
            (int i) => i + " s",
            (float f) => string.Format("{0:0.00}", f)
        );

        PlotDataOnGraph(
            data3, 
            graph3,
            Color.red,
            (int i) => i + " s",
            (float f) => Mathf.RoundToInt(f).ToString()
        );

        PlotDataOnGraph(
            data4,
            graph4,
            Color.yellow,
            (int i) => i + " s",
            (float f) => HelperFunctions.SecondsToHMS((int) f)[1] + ":" + HelperFunctions.SecondsToHMS((int) f)[2].ToString("D2")
        );

        PlotDataOnGraph(
            data5,
            graph5,
            Color.cyan,
            (int i) => i + " s",
            (float f) => ((int) f).ToString()
        );
    }

    private GameObject CreateCircle(RectTransform graphContainer, Vector2 anchoredPosition, bool showCircles = true)
    {
        GameObject gameObject = Instantiate(graphPoint).gameObject;
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.SetActive(showCircles);

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;

        rectTransform.sizeDelta = new Vector2(6, 6);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return gameObject;
    }

    private void ClearGraph(RectTransform graphContainer)
    {
        foreach (Transform child in graphContainer.transform)
        {
            if (child.name.Contains("Graph"))
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    private void PlotDataOnGraph(List<float> dataPoints, RectTransform graphContainer, Color color, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        if (getAxisLabelX == null)
        {
            getAxisLabelX = delegate (int i) { return i.ToString(); };
        }

        if (getAxisLabelY == null)
        {
            getAxisLabelY = delegate (float f) { return Mathf.RoundToInt(f).ToString(); };
        }

        float graphHeight = graph1.sizeDelta.y;
        float graphWidth = graph1.sizeDelta.x;

        float unitWidth = graphWidth / ((dataPoints.Count != 0) ? dataPoints.Count : 1);
        float unitHeight = graphHeight / ((dataPoints.Max() != 0) ? dataPoints.Max() : 1);

        float yMinimum = dataPoints.Min();
        float yMaximum = dataPoints.Max();

        int index = 0;
        GameObject previousDataPoint = null;
        foreach (float dataPoint in dataPoints)
        {
            // Determine x, y position
            float xPosition = (index * unitWidth) + (unitWidth / 2);
            float yPosition = (unitHeight * dataPoint);

            // Create data point
            GameObject currentDataPoint = CreateCircle(graphContainer, new Vector2(xPosition, yPosition), false);

            // Create connection between two valid points
            if (previousDataPoint != null)
            {
                CreateDotConnection(
                    graphContainer,
                    previousDataPoint.GetComponent<RectTransform>().anchoredPosition,
                    currentDataPoint.GetComponent<RectTransform>().anchoredPosition,
                    color
                );
            }

            // Update data point
            previousDataPoint = currentDataPoint;

            // Iterate
            index++;
        }

        int seperatorCount = 3;
        for (int i = 0; i <= seperatorCount; i++)
        {
            float normalisedValue = i * 1f / seperatorCount;

            // Create seperators along y-axis
            RectTransform dashY = Instantiate(dashTemplateY);
            dashY.SetParent(graphContainer, false);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(0f, normalisedValue * graphHeight);

            // Create labels along y-axis
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            labelY.anchoredPosition = new Vector2(-65f, normalisedValue * graphHeight);
            labelY.GetComponent<TMP_Text>().text = getAxisLabelY((yMaximum / seperatorCount) * i);
        }
    }

    private void CreateDotConnection(RectTransform graphContainer, Vector2 dotPositionA, Vector2 dotPositionB, Color color)
    {
        // Create line
        GameObject gameObject = Instantiate(graphConnection).gameObject;
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.SetActive(true);

        // Update line color
        gameObject.GetComponent<Image>().color = color;

        // Determine line orientation
        var direction = (dotPositionB - dotPositionA).normalized;
        var distance = Vector2.Distance(dotPositionA, dotPositionB);

        // Draw line
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3);

        rectTransform.anchoredPosition = dotPositionA + direction * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, HelperFunctions.GetAngleFromVectorFloat(direction));
    }

    public void ActivateGraph(string name)
    {
        // Set graph correlating to graph name to active
        // Set all others graphs to inactive
        foreach (RectTransform graph in graphs)
        {
            graph.gameObject.SetActive(graph.name.Equals(name));
        }
    }

    public void UpdateLabel(string label)
    {
        // Update label
        graphLabel.text = label;
    }
}