using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class StatsManager : MonoBehaviour
{
    // TODO: EXTRACT INTO SEPERATE .APPDATA SETTINGS FILE
    public static readonly int[] SplitDistances = { 100, 500, 1000, 1500, 5000 };
    public static readonly float KILOMETERS_METER = 1000;
    public static readonly float SECS_MINUTE = 60;

    private const int MAX_DATA_POINTS = 2000;

    // Basic Data
    [SerializeField] private TMP_Text TimeDisplay;
    [SerializeField] private TMP_Text DistanceDisplay;
    [SerializeField] private TMP_Text SpeedDisplay;

    // Split Data
    [SerializeField] private TMP_Text SplitTimeDisplay;
    [SerializeField] private TMP_Text[] SplitDistDisplays;
    [SerializeField] private TMP_Text SplitAvgPaceDisplay;
    [SerializeField] private TMP_Text SplitAvgPowerDisplay;

    // Projection Data
    [SerializeField] private TMP_Text ProjectedWorkTimeDisplay;
    [SerializeField] private TMP_Text ProjectedWorkDistanceDisplay;

    // Stroke Data
    [SerializeField] private TMP_Text StrokesPerMinDisplay;
    [SerializeField] private TMP_Text StrokePowerDisplay;
    [SerializeField] private TMP_Text DriveLengthDisplay;

    // Drag Data
    [SerializeField] private TMP_Text DragFactorDisplay;

    // Live Data
    private float Distance;
    private float Time;
    private float Speed;

    private float SplitTime;
    private float SplitDist;
    private float SplitAvgPace;
    private float SplitAvgPower;

    private int ProjectedWorkTime;
    private int ProjectedWorkDistance;

    private int StrokesPerMin;
    private int StrokePower;
    private int StrokeState;
    private float DriveLength;

    private float DragFactor;

    public static Queue<float> DistanceData { get; private set; }
    public static Queue<float> SpeedData { get; private set; }
    public static Queue<float> TimeData { get; private set; }

    public static Queue<float> SplitTimeData { get; private set; }
    public static Queue<float> SplitAvgPaceData { get; private set; }
    public static Queue<float> SplitAvgPowerData { get; private set; }

    public static Queue<int> ProjectedWorkTimeData { get; private set; }
    public static Queue<int> ProjectedWorkDistanceData { get; private set; }

    public static Queue<int> StrokesPerMinData { get; private set; }
    public static Queue<float> StrokePowerData { get; private set; }

    public static Queue<float> DriveLengthData { get; private set; }
    public static Queue<float> DragFactorData { get; private set; }

    private void Start()
    {
        ResetDisplay();
        BuildDataStores();

        // Update stats every 0.1 seconds
        InvokeRepeating("UpdateStats", 0f, 0.5f);
    }

    private void ResetDisplay()
    {
        ResetStats();
        UpdateDisplay();
    }

    private void BuildDataStores()
    {
        DistanceData = new Queue<float>();
        SpeedData = new Queue<float>();
        TimeData = new Queue<float>();

        SplitTimeData = new Queue<float>();
        SplitAvgPaceData = new Queue<float>();
        SplitAvgPowerData = new Queue<float>();

        ProjectedWorkTimeData = new Queue<int>();
        ProjectedWorkDistanceData = new Queue<int>();

        StrokesPerMinData = new Queue<int>();
        StrokePowerData = new Queue<float>();

        DriveLengthData = new Queue<float>();
        DragFactorData = new Queue<float>();
    }

    private void UpdateStats()
    {
        RetrieveStats();
    }

    // Measured as 0.1 meters per least-significant bit
    private const float DISTANCE_L_METER_VALUE = 0.1f;                                      // Max        25.5 Meters
    private const float DISTANCE_M_METER_VALUE = DISTANCE_L_METER_VALUE * 256;              // Max     6,553.5 Meters
    private const float DISTANCE_H_METER_VALUE = DISTANCE_M_METER_VALUE * 256;              // Max 1,677,721.5 Meters

    private float DistanceL;
    private float DistanceM;
    private float DistanceH;

    // Measured as 0.01 seconds per least-significant bit
    private const float ELAPSED_TIME_L_S_VALUE = 0.01f;                                     // Max        2.55 Seconds
    private const float ELAPSED_TIME_M_S_VALUE = ELAPSED_TIME_L_S_VALUE * 256;              // Max      655.35 Seconds (11 Minutes)
    private const float ELAPSED_TIME_H_S_VALUE = ELAPSED_TIME_M_S_VALUE * 256;              // Max  167,769.60 Seconds (46 Hours)

    private float ElapsedTimeL;
    private float ElapsedTimeM;
    private float ElapsedTimeH;

    // Measured as 0.001 m/s per least-significant bit
    private const float SPEED_L_MPS_VALUE = 0.001f;                                         // Max        2.55 m/s
    private const float SPEED_H_MPS_VALUE = SPEED_L_MPS_VALUE * 256;                        // Max      655.35 m/s

    private float SpeedL;
    private float SpeedH;

    // Measured as 1 watt per least-significant bit
    private const int POWER_L_W_VALUE = 1;                                                  // Max         255 w
    private const int POWER_H_W_VALUE = POWER_L_W_VALUE * 256;                              // Max      65,535 w

    private int PowerL;
    private int PowerH;

    // Measured as 0.1 seconds per least-significant bit
    private const float SPLIT_INT_TIME_L_S_VALUE = 0.1f;                                    // Max        25.50 Seconds
    private const float SPLIT_INT_TIME_M_S_VALUE = SPLIT_INT_TIME_L_S_VALUE * 256;          // Max      6528.00 Seconds (108 Minutes)
    private const float SPLIT_INT_TIME_H_S_VALUE = SPLIT_INT_TIME_M_S_VALUE * 256;          // Max 1,671,168.00 Seconds (464 Hours)

    private float SplitTimeL;
    private float SplitTimeM;
    private float SplitTimeH;

    // Measured as 0.1 meters per least-significant bit
    private const int SPLIT_INT_DISTANCE_L_M_VALUE = 1;                                     // Max         255 Meters
    private const int SPLIT_INT_DISTANCE_M_M_VALUE = SPLIT_INT_DISTANCE_L_M_VALUE * 256;    // Max      65,280 Meters
    private const int SPLIT_INT_DISTANCE_H_M_VALUE = SPLIT_INT_DISTANCE_M_M_VALUE * 256;    // Max  16,711,680 Meters

    private float SplitDistL;
    private float SplitDistM;
    private float SplitDistH;

    // Measured as 0.01 seconds per least-significant bit
    private const float SPLIT_INT_AVG_PACE_L_S_VALUE = 0.01f;
    private const float SPLIT_INT_AVG_PACE_H_S_VALUE = SPLIT_INT_AVG_PACE_L_S_VALUE * 256;

    private int SplitAvgPaceL;
    private int SplitAvgPaceH;

    // Measured as 1 watt per least-significant bit
    private const float SPLIT_INT_AVG_POWER_L_W_VALUE = 1f;
    private const float SPLIT_INT_AVG_POWER_H_W_VALUE = SPLIT_INT_AVG_POWER_L_W_VALUE * 256;

    private int SplitIntAvgPowerL;
    private int SplitIntAvgPowerH;

    // Measured as 1 second per least-significant bit
    private const int PROJECTED_WORK_TIME_L_S_VALUE = 1;
    private const int PROJECTED_WORK_TIME_M_S_VALUE = PROJECTED_WORK_TIME_L_S_VALUE * 256;
    private const int PROJECTED_WORK_TIME_H_S_VALUE = PROJECTED_WORK_TIME_M_S_VALUE * 256;

    private int ProjectedWorkTimeL;
    private int ProjectedWorkTimeH;
    private int ProjectedWorkTimeM;

    // Measured as 1 meter per least-significant bit
    private const int PROJECTED_WORK_DISTANCE_L_M_VALUE = 1;
    private const int PROJECTED_WORK_DISTANCE_M_M_VALUE = PROJECTED_WORK_DISTANCE_L_M_VALUE * 256;
    private const int PROJECTED_WORK_DISTANCE_H_M_VALUE = PROJECTED_WORK_DISTANCE_M_M_VALUE * 256;

    private int ProjectedWorkDistanceL;
    private int ProjectedWorkDistanceH;
    private int ProjectedWorkDistanceM;

    private float PreviousDistance = 0;
    private float PreviousTime = 0;

    private void RetrieveStats()
    {
        // Distance Covered (meters)
        DistanceL = BluetoothManager.RowingStatusData[3];           // Distance Lo
        DistanceM = BluetoothManager.RowingStatusData[4];           // Distance Mid
        DistanceH = BluetoothManager.RowingStatusData[5];           // Distance Hi

        Distance = (DistanceH * DISTANCE_H_METER_VALUE)
                 + (DistanceM * DISTANCE_M_METER_VALUE)
                 + (DistanceL * DISTANCE_L_METER_VALUE);

        // Time Rowing (seconds)
        ElapsedTimeL = BluetoothManager.RowingStatusData[0];        // Elapsed Time Lo
        ElapsedTimeM = BluetoothManager.RowingStatusData[1];        // Elapsed Time Mid
        ElapsedTimeH = BluetoothManager.RowingStatusData[2];        // Elapsed Time Hi

        Time = (ElapsedTimeH * ELAPSED_TIME_H_S_VALUE)
                   + (ElapsedTimeM * ELAPSED_TIME_M_S_VALUE)
                   + (ElapsedTimeL * ELAPSED_TIME_L_S_VALUE);

        // Return if no sizeable change since last update
        // Prevents unnecessary calculations (such as when the user is stationary)
        //if ((DistanceMeters == PreviousDistance) && (TimeSeconds == PreviousTime)) return;

        // Otherwise, update values
        PreviousDistance = Distance;
        PreviousTime = Time;

        // Speed (m/s)
        SpeedL = BluetoothManager.RowingStatusData1[3];              // Speed Lo
        SpeedH = BluetoothManager.RowingStatusData1[4];              // Speed Hi

        Speed = (SpeedL * SPEED_L_MPS_VALUE)
              + (SpeedH * SPEED_H_MPS_VALUE);

        // Power (w)
        PowerL = BluetoothManager.StrokeData1[3];                   // Stroke Power Lo
        PowerH = BluetoothManager.StrokeData1[4];                   // Stroke Power Hi

        StrokePower = (PowerL * POWER_L_W_VALUE)
                    + (PowerH * POWER_H_W_VALUE);

        // Split Time
        SplitTimeL = BluetoothManager.SplitIntervalData[6];
        SplitTimeM = BluetoothManager.SplitIntervalData[7];
        SplitTimeH = BluetoothManager.SplitIntervalData[8];

        SplitTime = (SplitTimeL * SPLIT_INT_TIME_L_S_VALUE)
                  + (SplitTimeM * SPLIT_INT_TIME_M_S_VALUE)
                  + (SplitTimeH * SPLIT_INT_TIME_H_S_VALUE);

        // Split Distance
        SplitDistL = BluetoothManager.SplitIntervalData[9];
        SplitDistM = BluetoothManager.SplitIntervalData[10];
        SplitDistH = BluetoothManager.SplitIntervalData[11];

        SplitDist = (SplitDistL * SPLIT_INT_DISTANCE_L_M_VALUE)
                  + (SplitDistM * SPLIT_INT_DISTANCE_M_M_VALUE)
                  + (SplitDistH * SPLIT_INT_DISTANCE_H_M_VALUE);

        // Avg Split Pace
        SplitAvgPaceL = BluetoothManager.RowingStatusData2[6];
        SplitAvgPaceH = BluetoothManager.RowingStatusData2[7];

        SplitAvgPace = (SplitAvgPaceL * SPLIT_INT_AVG_PACE_L_S_VALUE)
                     + (SplitAvgPaceH * SPLIT_INT_AVG_PACE_H_S_VALUE);

        // Average Split Power
        SplitIntAvgPowerL = BluetoothManager.RowingStatusData2[8];
        SplitIntAvgPowerH = BluetoothManager.RowingStatusData2[9];

        SplitAvgPower = (SplitIntAvgPowerL * SPLIT_INT_AVG_POWER_L_W_VALUE)
                      + (SplitIntAvgPowerH * SPLIT_INT_AVG_POWER_H_W_VALUE);

        // Projected Workout Time
        ProjectedWorkTimeL = BluetoothManager.StrokeData1[9];
        ProjectedWorkTimeM = BluetoothManager.StrokeData1[10];
        ProjectedWorkTimeH = BluetoothManager.StrokeData1[11];

        ProjectedWorkTime = (ProjectedWorkTimeL * PROJECTED_WORK_TIME_L_S_VALUE)
                          + (ProjectedWorkTimeM * PROJECTED_WORK_TIME_M_S_VALUE)
                          + (ProjectedWorkTimeH * PROJECTED_WORK_TIME_H_S_VALUE);

        // Projected Workout Distance
        ProjectedWorkDistanceL = BluetoothManager.StrokeData1[12];
        ProjectedWorkDistanceM = BluetoothManager.StrokeData1[13];
        ProjectedWorkDistanceH = BluetoothManager.StrokeData1[14];

        ProjectedWorkDistance = (ProjectedWorkDistanceL * PROJECTED_WORK_DISTANCE_L_M_VALUE)
                              + (ProjectedWorkDistanceM * PROJECTED_WORK_DISTANCE_M_M_VALUE)
                              + (ProjectedWorkDistanceH * PROJECTED_WORK_DISTANCE_H_M_VALUE);

        // Stroke data
        DragFactor = BluetoothManager.RowingStatusData[18];         // Drag Factor
        StrokeState = BluetoothManager.RowingStatusData[10];        // Stroke State
        StrokesPerMin = BluetoothManager.RowingStatusData1[5];      // Stroke Rate
        DriveLength = BluetoothManager.StrokeData[6];               // Drive Length

        // Update data stores
        UpdateDataStores();

        // Update display
        UpdateDisplay();
    }

    private void ResetStats()
    {
        Distance = 0;
        Speed = 0;
        Time = 0;
        SplitDist = 500;
        SplitTime = 0;
        SplitAvgPace = 0;
        SplitAvgPower = 0;
        ProjectedWorkTime = 0;
        ProjectedWorkDistance = 0;
        StrokesPerMin = 0;
        StrokePower = 0;
        DriveLength = 0;
        DragFactor = 0;
    }

    private void UpdateDisplay()
    {
        SetDistanceDisplay(Distance);
        SetSpeedDisplay(Speed);
        SetTimeDisplay(Time);

        SetSplitDistDisplay((SplitDist == 0) ? 500 : SplitDist); // Default 500 unless set by user
        SetSplitTimeDisplay(SplitTime);
        SetSplitAvgPaceDisplay(SplitAvgPace);
        SetSplitAvgPowerDisplay(SplitAvgPower);

        SetProjectedWorkTimeDisplay(ProjectedWorkTime);
        SetProjectedWorkDistanceDisplay(ProjectedWorkDistance);

        SetStrokesPerMinDisplay(StrokesPerMin);
        SetPowerDisplay(StrokePower);

        SetDriveLengthDisplay(DriveLength);
        SetDragFactorDisplay(DragFactor);
    }

    private int dataPointCount = 0;
    private void UpdateDataStores()
    {
        dataPointCount++;
        if (dataPointCount > MAX_DATA_POINTS)
        {
            Dequeue();
        }

        Enqueue();

        StatsGraph.Instance.UpdateGraph();
    }

    private void Dequeue()
    {
        DistanceData.Dequeue();
        TimeData.Dequeue();
        SpeedData.Dequeue();

        SplitTimeData.Dequeue();
        SplitAvgPaceData.Dequeue();
        SplitAvgPowerData.Dequeue();

        ProjectedWorkDistanceData.Dequeue();
        ProjectedWorkTimeData.Dequeue();

        StrokesPerMinData.Dequeue();
        StrokePowerData.Dequeue();

        DriveLengthData.Dequeue();
        DragFactorData.Dequeue();
    }

    private void Enqueue()
    {
        DistanceData.Enqueue(Distance);
        SpeedData.Enqueue(Speed);
        TimeData.Enqueue(Time);
        
        SplitTimeData.Enqueue(SplitTime);
        SplitAvgPaceData.Enqueue(SplitAvgPace);
        SplitAvgPowerData.Enqueue(SplitAvgPower);

        ProjectedWorkDistanceData.Enqueue(ProjectedWorkDistance);
        ProjectedWorkTimeData.Enqueue(ProjectedWorkTime);

        StrokesPerMinData.Enqueue(StrokesPerMin);
        StrokePowerData.Enqueue(StrokePower);

        DriveLengthData.Enqueue(DriveLength);
        DragFactorData.Enqueue(DragFactor);
    }

    private void SetDistanceDisplay(float distance)
    {
        if (!DistanceDisplay.enabled) return;

        DistanceDisplay.text = (int) distance + "m";
    }

    private void SetSpeedDisplay(float speed)
    {
        if (!SpeedDisplay.enabled) return;

        SpeedDisplay.text = string.Format("{0:0.00}", speed) + "m/s";
    }

    private void SetTimeDisplay(float seconds)
    {
        if (!TimeDisplay.enabled) return;

        int[] hms = HelperFunctions.SecondsToHMS((int) seconds);

        TimeDisplay.text = hms[0].ToString("D2") + ":" + hms[1].ToString("D2") + ":" + hms[2].ToString("D2");
    }

    private void SetSplitDistDisplay(float distance)
    {
        foreach(TMP_Text SplitDistDisplay in SplitDistDisplays)
        {
            if (!SplitDistDisplay.enabled) continue;

            SplitDistDisplay.text = distance.ToString();
        }
    }

    private void SetSplitTimeDisplay(float seconds)
    {
        if (!SplitTimeDisplay.enabled) return;

        int[] hms = HelperFunctions.SecondsToHMS((int)seconds);

        SplitTimeDisplay.text = hms[1] + ":" + hms[2].ToString("D2");
    }

    private void SetSplitAvgPaceDisplay(float splitAvgPace)
    {
        if (!SplitAvgPaceDisplay.enabled) return;

        int[] hms = HelperFunctions.SecondsToHMS((int)splitAvgPace);

        SplitAvgPaceDisplay.text = hms[1].ToString("D2") + ":" + hms[2].ToString("D2");
    }

    private void SetSplitAvgPowerDisplay(float splitAvgPower)
    {
        if (!SplitAvgPowerDisplay.enabled) return;

        SplitAvgPowerDisplay.text = splitAvgPower + "w";
    }

    private void SetStrokesPerMinDisplay(int strokesPerMin)
    {
        if (!StrokesPerMinDisplay.enabled) return;

        StrokesPerMinDisplay.text = strokesPerMin + " s/m";
    }

    private void SetPowerDisplay(int power)
    {
        if (!StrokePowerDisplay.enabled) return;

        StrokePowerDisplay.text = power + "w";
    }

    private void SetDriveLengthDisplay(float driveLength)
    {
        if (!DriveLengthDisplay.enabled) return;

        DriveLengthDisplay.text = (driveLength / 100) + "m";
    }

    private void SetDragFactorDisplay(float dragFactor)
    {
        if (!DragFactorDisplay.enabled) return;

        DragFactorDisplay.text = dragFactor.ToString();
    }

    private void SetProjectedWorkTimeDisplay(float projectedWorkTime)
    {
        if (!ProjectedWorkTimeDisplay.enabled) return;

        ProjectedWorkTimeDisplay.text = projectedWorkTime + "s";
    }

    private void SetProjectedWorkDistanceDisplay(float projectedWorkDist)
    {
        if (!ProjectedWorkDistanceDisplay.enabled) return;

        ProjectedWorkDistanceDisplay.text = projectedWorkDist + "m";
    }

    public int GetMetersRowed()
    {
        return (int) Distance;
    }

    public int GetSecondsRowing()
    {
        return (int) Time;
    }

    public float GetSpeed()
    {
        return Speed;
    }

    public float GetStrokePower()
    {
        return StrokePower;
    }

    public int GetStrokeState()
    {
        return StrokeState;
    }
}
