using System.Collections.Generic;
using System.Linq;
using System;

using UnityEngine.UI;
using UnityEngine;
using TMPro;

// Code referenced: https://www.youtube.com/playlist?list=PLzDRvYVwl53v5ur4GluoabyckImZz3TVQ
//
//
//
public class StatsGraph : MonoBehaviour
{
    public static StatsGraph Instance;

    private const int MAX_POINTS = 30;
    private const int X_SEPERATOR = 5;
    private const int SEPERATOR_COUNT = 3;

    [SerializeField] private Transform graph;
    [SerializeField] private TMP_Text graphLabel;

    [SerializeField] private Image dataPoint;
    [SerializeField] private Image dataConnection;

    [SerializeField] private RectTransform labelTemplateX;
    [SerializeField] private RectTransform labelTemplateY;

    [SerializeField] private RectTransform dashTemplateX;
    [SerializeField] private RectTransform dashTemplateY;

    [SerializeField] private GameObject[] statButtons;

    private RectTransform graph1;
    private RectTransform graph2;
    private RectTransform graph3;
    private RectTransform graph4;
    private RectTransform graph5;

    private List<RectTransform> graphs;

    private List<GameObject> graph1Points;
    private List<GameObject> graph2Points;
    private List<GameObject> graph3Points;
    private List<GameObject> graph4Points;
    private List<GameObject> graph5Points;

    private List<GameObject> graph1Connections;
    private List<GameObject> graph2Connections;
    private List<GameObject> graph3Connections;
    private List<GameObject> graph4Connections;
    private List<GameObject> graph5Connections;

    private List<RectTransform> graph1Labels;
    private List<RectTransform> graph2Labels;
    private List<RectTransform> graph3Labels;
    private List<RectTransform> graph4Labels;
    private List<RectTransform> graph5Labels;

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

        graph1Points = new List<GameObject>();
        graph2Points = new List<GameObject>();
        graph3Points = new List<GameObject>();
        graph4Points = new List<GameObject>();
        graph5Points = new List<GameObject>();

        graph1Connections = new List<GameObject>();
        graph2Connections = new List<GameObject>();
        graph3Connections = new List<GameObject>();
        graph4Connections = new List<GameObject>();
        graph5Connections = new List<GameObject>();

        graph1Labels = new List<RectTransform>();
        graph2Labels = new List<RectTransform>();
        graph3Labels = new List<RectTransform>();
        graph4Labels = new List<RectTransform>();
        graph5Labels = new List<RectTransform>();

        CreateGraph(graph1, graph1Points, graph1Connections, graph1Labels, new Color(1, 0.49f, 0));
        CreateGraph(graph2, graph2Points, graph2Connections, graph2Labels, new Color(1, 0.49f, 0));
        CreateGraph(graph3, graph3Points, graph3Connections, graph3Labels, new Color(1, 0.49f, 0));
        CreateGraph(graph4, graph4Points, graph4Connections, graph4Labels, new Color(1, 0.49f, 0));
        CreateGraph(graph5, graph5Points, graph5Connections, graph5Labels, new Color(1, 0.49f, 0));

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

        PlotDataOnGraph(
            data1, 
            graph1,
            graph1Points,
            graph1Connections,
            graph1Labels,
            (int i) => i + " s",
            (float f) => Mathf.RoundToInt(f).ToString()
        );

        PlotDataOnGraph(
            data2, 
            graph2,
            graph2Points,
            graph2Connections,
            graph2Labels,
            (int i) => i + " s",
            (float f) => string.Format("{0:0.00}", f)
        );

        PlotDataOnGraph(
            data3, 
            graph3,
            graph3Points,
            graph3Connections,
            graph3Labels,
            (int i) => i + " s",
            (float f) => Mathf.RoundToInt(f).ToString()
        );

        PlotDataOnGraph(
            data4,
            graph4,
            graph4Points,
            graph4Connections,
            graph4Labels,
            (int i) => i + " s",
            (float f) => HelperFunctions.SecondsToHMS((int) f)[1] + ":" + HelperFunctions.SecondsToHMS((int) f)[2].ToString("D2")
        );

        PlotDataOnGraph(
            data5,
            graph5,
            graph5Points,
            graph5Connections,
            graph5Labels,
            (int i) => i + " s",
            (float f) => ((int) f).ToString()
        );
    }

    private void CreateGraph(RectTransform graphContainer, List<GameObject> pointList, List<GameObject> connectionList, List<RectTransform> labelList, Color color)
    {
        // Calculate values
        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        float unitWidth = graphWidth / MAX_POINTS;
        float unitHeight = graphHeight / SEPERATOR_COUNT;

        GameObject previousPoint = null;

        // Create data points
        for (int i = 0; i < MAX_POINTS; i++)
        {
            // Determine x, y position
            float xPosition = (i * unitWidth) + (unitWidth / 2);
            float yPosition = 0;

            // Create data point
            GameObject point = CreateDataPoint(graphContainer, new Vector2(xPosition, yPosition), false);

            // Create data connection
            GameObject connection = CreateDataConnection(graphContainer, color);

            // Update data point
            previousPoint = point;

            // Add point to point list
            pointList.Add(point);

            // Add connection to connection list
            connectionList.Add(connection);
        }

        // Create labels
        for (int i = 0; i <= SEPERATOR_COUNT; i++)
        {
            float normalisedValue = i * 1f / SEPERATOR_COUNT;

            // Create seperators along y-axis
            RectTransform dashY = Instantiate(dashTemplateY);
            dashY.SetParent(graphContainer, false);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(0f, normalisedValue * graphHeight);

            // Create labels along y-axis
            RectTransform label = Instantiate(labelTemplateY);
            label.SetParent(graphContainer, false);
            label.gameObject.SetActive(true);
            label.anchoredPosition = new Vector2(-65f, normalisedValue * graphHeight);

            // Add label to label list
            labelList.Add(label);
        }
    }

    private void PlotDataOnGraph(List<float> dataPoints, RectTransform graphContainer, List<GameObject> graphPoints, List<GameObject> graphConnections, List<RectTransform> graphLabels, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        if (getAxisLabelX == null)
        {
            getAxisLabelX = delegate (int i) { return i.ToString(); };
        }

        if (getAxisLabelY == null)
        {
            getAxisLabelY = delegate (float f) { return Mathf.RoundToInt(f).ToString(); };
        }

        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        float yMinimum = dataPoints.Min();
        float yMaximum = dataPoints.Max();

        float dataRange = yMaximum - yMinimum;

        float unitWidth = graphWidth / ((dataPoints.Count != 0) ? dataPoints.Count : 1);
        float unitHeight = graphHeight / dataRange / ((dataRange != 0) ? dataRange : 1);

        GameObject previousDataPoint = null;
        for (int i = 0; i < dataPoints.Count; i++)
        {
            // Determine x, y position
            float xPosition = (i * unitWidth) + (unitWidth / 2);
            float yPosition = ((dataPoints[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;

            // Retrieve currnt point
            GameObject currentDataPoint = graphPoints[i];

            // Plot point
            PlotPoint(currentDataPoint, xPosition, yPosition);
            
            // Create connection between two valid points
            if (previousDataPoint != null)
            {
                // Retrieve current connection
                GameObject currentConnection = graphConnections[i];

                // Draw connection
                DrawConnection(currentConnection, previousDataPoint.GetComponent<RectTransform>().anchoredPosition, currentDataPoint.GetComponent<RectTransform>().anchoredPosition);
            }

            // Update data point
            previousDataPoint = currentDataPoint;
        }

        // Update labels
        for (int i = 0; i < graphLabels.Count; i++)
        {
            float normalizedValue = i * 1f / SEPERATOR_COUNT;
            graphLabels[i].GetComponent<TMP_Text>().text = getAxisLabelY(yMinimum + (normalizedValue * (yMaximum - yMinimum)));
        }
    }

    private GameObject CreateDataPoint(RectTransform graphContainer, Vector2 anchoredPosition, bool showCircles = true)
    {
        // Create data point
        GameObject gameObject = Instantiate(dataPoint).gameObject;
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.SetActive(showCircles);

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;

        rectTransform.sizeDelta = new Vector2(6, 6);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return gameObject;
    }

    private GameObject CreateDataConnection(RectTransform graphContainer, Color color)
    {
        // Create connection
        GameObject gameObject = Instantiate(dataConnection).gameObject;
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.SetActive(true);

        // Update connection color
        gameObject.GetComponent<Image>().color = color;

        return gameObject;
    }

    private void PlotPoint(GameObject point, float xPosition, float yPosition)
    {
        RectTransform rectTransform = point.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);
    }

    private void DrawConnection(GameObject connection, Vector2 dotPositionA, Vector2 dotPositionB)
    {
        // Determine line orientation
        var direction = (dotPositionB - dotPositionA).normalized;
        var distance = Vector2.Distance(dotPositionA, dotPositionB);

        // Draw line
        RectTransform rectTransform = connection.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3);

        rectTransform.anchoredPosition = dotPositionA + direction * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, HelperFunctions.GetAngleFromVectorFloat(direction));
    }

    private const int STAT_COUNT = 5;
    public void ActivateGraph(int index)
    {
        for(int i = 0; i < STAT_COUNT; i++)
        {
            var active = (i == index);

            statButtons[i].gameObject.SetActive(active);
            graphs[i].gameObject.SetActive(active);
        }
    }

    public void UpdateLabel(string label)
    {
        // Update label
        graphLabel.text = label;
    }
}