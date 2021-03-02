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

    [SerializeField] private Image graphPoint;
    [SerializeField] private Image graphConnection;

    [SerializeField] private RectTransform labelTemplateX;
    [SerializeField] private RectTransform labelTemplateY;

    [SerializeField] private RectTransform dashTemplateX;
    [SerializeField] private RectTransform dashTemplateY;


    private RectTransform graphContainer1;
    private RectTransform graphContainer2;
    private RectTransform graphContainer3;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;

        graphContainer1 = graph.Find("Graph Container 1").GetComponent<RectTransform>();
        graphContainer2 = graph.Find("Graph Container 2").GetComponent<RectTransform>();
        graphContainer3 = graph.Find("Graph Container 3").GetComponent<RectTransform>();
    }

    private List<float> data1 = new List<float>();
    private List<float> data2 = new List<float>();
    private List<float> data3 = new List<float>();
    
    public void UpdateGraph()
    {
        int count1 = (MAX_POINTS < StatsManager.StrokePowerData.Count) ? MAX_POINTS : StatsManager.StrokePowerData.Count;
        data1 = StatsManager.StrokePowerData.ToList().GetRange(
            StatsManager.StrokePowerData.Count - count1, 
            count1
        );

        int count2 = (MAX_POINTS < StatsManager.SpeedData.Count) ? MAX_POINTS : StatsManager.SpeedData.Count;
        data2 = StatsManager.SpeedData.ToList().GetRange(
            StatsManager.SpeedData.Count - count2,
            count2
        );

        int count3 = (MAX_POINTS < StatsManager.SplitTimeData.Count) ? MAX_POINTS : StatsManager.SplitTimeData.Count;
        data3 = StatsManager.SplitTimeData.ToList().GetRange(
            StatsManager.SplitTimeData.Count - count3, 
            count3
        );
        
        //data1.Add(Random.Range(0, 200));
        //data1 = (StatsManager.DistanceData.Count > MAX_POINTS)
        //        ? Enumerable.Range(0, MAX_POINTS).Select(i => StatsManager.DistanceData.Dequeue()).ToList()
        //        : new List<float>() { 0 };

        //data2.Add(Random.Range(0, 5));
        //data2 = (StatsManager.SpeedData.Count > MAX_POINTS)
        //        ? Enumerable.Range(0, MAX_POINTS).Select(i => StatsManager.SpeedData.Dequeue()).ToList()
        //        : new List<float>() { 0 };

        //data3.Add(Random.Range(0, 3));
        //data3 = (StatsManager.TimeData.Count > MAX_POINTS)
        //        ? Enumerable.Range(0, MAX_POINTS).Select(i => StatsManager.TimeData.Dequeue()).ToList()
        //        : new List<float>() { 0 };

        //Enumerable.Range(0, MAX_POINTS).Select(i => StatsManager.SpeedData.Dequeue()).ToList();
        //Enumerable.Range(0, MAX_POINTS).Select(i => StatsManager.TimeData.Dequeue()).ToList();

        ClearGraph(graphContainer1);
        ClearGraph(graphContainer2);
        ClearGraph(graphContainer3);

        PlotDataOnGraph(
            data1, 
            graphContainer1, 
            Color.green,
            (int i) => i + " s",
            (float f) => Mathf.RoundToInt(f).ToString() + " w"
        );

        PlotDataOnGraph(
            data2, 
            graphContainer2, 
            Color.magenta,
            (int i) => i + " s", 
            (float f) => Mathf.RoundToInt(f).ToString() + " m/s"
        );

        PlotDataOnGraph(
            data3, 
            graphContainer3, 
            Color.red,
            (int i) => i + " s",
            (float f) => string.Format("{0:0.00}", f) + "/500"
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

        float graphHeight = graphContainer1.sizeDelta.y;
        float graphWidth = graphContainer1.sizeDelta.x;

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
            labelY.anchoredPosition = new Vector2(-45f, normalisedValue * graphHeight);
            labelY.GetComponent<TMP_Text>().text = getAxisLabelY(normalisedValue * (yMaximum - yMinimum));
        }
    }

    private void CreateDotConnection(RectTransform graphContainer, Vector2 dotPositionA, Vector2 dotPositionB, Color color)
    {
        // Create line
        GameObject gameObject = Instantiate(graphConnection).gameObject;
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.SetActive(true);

        // Update line color
        gameObject.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.5f);

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
}