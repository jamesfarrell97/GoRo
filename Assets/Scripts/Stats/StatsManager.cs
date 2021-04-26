using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Random = UnityEngine.Random;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    // TODO: EXTRACT INTO SEPERATE .APPDATA SETTINGS FILE
    public static readonly int[] SplitDistances = { 100, 500, 1000, 1500, 5000 };
    public static readonly float KILOMETERS_METER = 1000;
    public static readonly float SECS_MINUTE = 60;

    public static readonly float STATS_SAMPLE_RATE = 10;    // Number of stats samples per second
    public static readonly float GRAPH_UPDATE_RATE = 2;     // Number of graph updates per second
    public static readonly float MOVE_SAMPLE_RATE = 1;      // Number of movement samples per second
    public static readonly float SAMPLE_MINS = 60;          // Maximum minutes worth of data samples to store

    // Maximum samples to store
    private readonly int MAX_DATA_POINTS = (int) (SECS_MINUTE * SAMPLE_MINS * STATS_SAMPLE_RATE);

    // Basic Data
    [SerializeField] private TMP_Text TimeDisplay;
    [SerializeField] private TMP_Text DistanceDisplay;
    [SerializeField] private TMP_Text SpeedDisplay;
    [SerializeField] private TMP_Text PaceDisplay;

    // Split Data
    [SerializeField] private TMP_Text[] SplitDistDisplays;

    // Stroke Data
    [SerializeField] private TMP_Text StrokesPerMinDisplay;
    [SerializeField] private TMP_Text StrokePowerDisplay;
    [SerializeField] private TMP_Text AvgForceDisplay;

    // Debug
    [SerializeField] private TMP_Text DebugDisplay;

    // Split Data
    // [SerializeField] private TMP_Text SplitTimeDisplay;
    // [SerializeField] private TMP_Text SplitAvgPowerDisplay;
    // [SerializeField] private TMP_Text SplitAvgPaceDisplay;
    
    // Projection Data
    // [SerializeField] private TMP_Text ProjectedWorkTimeDisplay;
    // [SerializeField] private TMP_Text ProjectedWorkDistanceDisplay;

    // Stroke Data
    // [SerializeField] private TMP_Text DriveLengthDisplay;

    // Drag Data
    // [SerializeField] private TMP_Text DragFactorDisplay;

    // Live Data
    private float Distance;
    private float Time;
    private float Speed;
    private float Pace;

    private float SplitTime;
    private float SplitDist;
    private float SplitAvgPace;
    private float SplitAvgPower;

    private int ProjectedWorkTime;
    private int ProjectedWorkDistance;

    private int StrokeState;

    private float StrokePower;
    private float StrokesPerMin;
    private float DriveLength;
    private float AvgForce;

    private float DragFactor;

    PlayerController Player;

    public static Queue<float> DistanceData { get; private set; }
    public static Queue<float> SpeedData { get; private set; }
    public static Queue<float> TimeData { get; private set; }
    public static Queue<float> PaceData { get; private set; }

    public static Queue<float> SplitTimeData { get; private set; }
    public static Queue<float> SplitAvgPaceData { get; private set; }
    public static Queue<float> SplitAvgPowerData { get; private set; }

    public static Queue<float> ProjectedWorkTimeData { get; private set; }

    public static Queue<float> ProjectedWorkDistanceData { get; private set; }

    public static Queue<float> StrokesPerMinData { get; private set; }
    public static Queue<float> StrokePowerData { get; private set; }

    public static Queue<float> DriveLengthData { get; private set; }
    public static Queue<float> DragFactorData { get; private set; }
    public static Queue<float> AvgForceData { get; private set; }

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    private void Start()
    {
        Reset();
        BuildDataStores();

        // Update player distance MOVE_SAMPLE_RATE times per second
        InvokeRepeating("UpdatePlayerDistance", 0f, (1f / MOVE_SAMPLE_RATE));

        // Update stats STATS_SAMPLE_RATE times per second
        InvokeRepeating("UpdateStats", 0f, (1f / STATS_SAMPLE_RATE));

        // Update graph GRAPH_UPDATE_RATE times per second
        // InvokeRepeating("UpdateGraph", 0f, (1f / GRAPH_UPDATE_RATE));
    }

    private void Update()
    {

#if !UNITY_EDITOR
        
        UpdateStrokeState();
    
#endif

        }

    private void Reset()
    {
        PerformanceMonitorManager.Instance.ResetPM();

        ResetStats();
        UpdateDisplay();
    }

    private void BuildDataStores()
    {
        DistanceData = new Queue<float>();
        SpeedData = new Queue<float>();
        TimeData = new Queue<float>();
        PaceData = new Queue<float>();

        SplitTimeData = new Queue<float>();
        SplitAvgPaceData = new Queue<float>();
        SplitAvgPowerData = new Queue<float>();

        ProjectedWorkTimeData = new Queue<float>();
        ProjectedWorkDistanceData = new Queue<float>();

        StrokesPerMinData = new Queue<float>();
        StrokePowerData = new Queue<float>();

        DriveLengthData = new Queue<float>();
        DragFactorData = new Queue<float>();
        AvgForceData = new Queue<float>();
    }

    private void UpdatePlayerDistance()
    {
#if !UNITY_EDITOR

        // Player must be assigned (by a local PlayerController) 
        // before this operation can take place
        // 
        // See PlayerController.Start();
        //
        if (Player != null)
        {

        
            // Sample player stats
            Player.SampleStats();
    
            // Update player
            Player.ERGUpdateDistance(Distance);


        }

#endif

    }
    
    private void UpdateStrokeState()
    {
        // Update often to sync rowing animation with user stroke state
        StrokeState = PerformanceMonitorManager.RowingStatusData[10];

        if (Player != null)
        {
            Player.Animate(StrokeState);
        }
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

    // Measured at 0.01 seconds per least-significant bit
    private const float PACE_L_S_VALUE = 0.01f;
    private const float PACE_H_S_VALUE = PACE_L_S_VALUE * 256;

    private float PaceL;
    private float PaceH;

    // Measured at 0.1lb of force per least-significant bit
    private const float AVG_FORCE_L_LB_VALUE = 0.1f;
    private const float AVG_FORCE_H_LB_VALUE = AVG_FORCE_L_LB_VALUE * 256;

    private float AvgForceL;
    private float AvgForceH;

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

    private int SampleCount = 0;

    private void UpdateStats()
    {
        Distance = (PerformanceMonitorManager.RowingStatusData[3] * DISTANCE_L_METER_VALUE)  // Distance Lo
                 + (PerformanceMonitorManager.RowingStatusData[4] * DISTANCE_M_METER_VALUE)  // Distance Mid
                 + (PerformanceMonitorManager.RowingStatusData[5] * DISTANCE_H_METER_VALUE); // Distance Hi

        Time = (PerformanceMonitorManager.RowingStatusData[0] * ELAPSED_TIME_L_S_VALUE)      // Elapsed Time Lo
             + (PerformanceMonitorManager.RowingStatusData[1] * ELAPSED_TIME_M_S_VALUE)      // Elapsed Time Mid
             + (PerformanceMonitorManager.RowingStatusData[2] * ELAPSED_TIME_H_S_VALUE);     // Elapsed Time Hi

#if !UNITY_EDITOR

        // Return if no sizeable change since last update
        // Prevents unnecessary calculations (such as when the user is stationary)
        if ((Distance == PreviousDistance) && (Time == PreviousTime)) return;

#endif

        // Otherwise, update values
        PreviousDistance = Distance;
        PreviousTime = Time;

        // Speed (m/s)
        SpeedL = PerformanceMonitorManager.RowingStatusData1[3];              // Speed Lo
        SpeedH = PerformanceMonitorManager.RowingStatusData1[4];              // Speed Hi

        Speed = (SpeedL * SPEED_L_MPS_VALUE)
              + (SpeedH * SPEED_H_MPS_VALUE);

        // Power (w)
        PowerL = PerformanceMonitorManager.StrokeData1[3];                   // Stroke Power Lo
        PowerH = PerformanceMonitorManager.StrokeData1[4];                   // Stroke Power Hi

        StrokePower = (PowerL * POWER_L_W_VALUE)
                    + (PowerH * POWER_H_W_VALUE);

        // Split Time
        SplitTimeL = PerformanceMonitorManager.SplitIntervalData[6];
        SplitTimeM = PerformanceMonitorManager.SplitIntervalData[7];
        SplitTimeH = PerformanceMonitorManager.SplitIntervalData[8];

        SplitTime = (SplitTimeL * SPLIT_INT_TIME_L_S_VALUE)
                  + (SplitTimeM * SPLIT_INT_TIME_M_S_VALUE)
                  + (SplitTimeH * SPLIT_INT_TIME_H_S_VALUE);

        // Pace
        PaceL = PerformanceMonitorManager.RowingStatusData1[7]; 
        PaceH = PerformanceMonitorManager.RowingStatusData1[8];

        Pace = (PaceL * PACE_L_S_VALUE)
             + (PaceH * PACE_H_S_VALUE);

        // Avg Force
        AvgForceL = PerformanceMonitorManager.StrokeData[14];
        AvgForceH = PerformanceMonitorManager.StrokeData[15];
 
        AvgForce = (AvgForceL * AVG_FORCE_L_LB_VALUE)
                 + (AvgForceH * AVG_FORCE_H_LB_VALUE);

        // Split Distance
        SplitDistL = PerformanceMonitorManager.SplitIntervalData[9];
        SplitDistM = PerformanceMonitorManager.SplitIntervalData[10];
        SplitDistH = PerformanceMonitorManager.SplitIntervalData[11];

        SplitDist = (SplitDistL * SPLIT_INT_DISTANCE_L_M_VALUE)
                  + (SplitDistM * SPLIT_INT_DISTANCE_M_M_VALUE)
                  + (SplitDistH * SPLIT_INT_DISTANCE_H_M_VALUE);

        // Avg Split Pace
        SplitAvgPaceL = PerformanceMonitorManager.RowingStatusData2[6];
        SplitAvgPaceH = PerformanceMonitorManager.RowingStatusData2[7];

        SplitAvgPace = (SplitAvgPaceL * SPLIT_INT_AVG_PACE_L_S_VALUE)
                     + (SplitAvgPaceH * SPLIT_INT_AVG_PACE_H_S_VALUE);

        // Average Split Power
        SplitIntAvgPowerL = PerformanceMonitorManager.RowingStatusData2[8];
        SplitIntAvgPowerH = PerformanceMonitorManager.RowingStatusData2[9];

        SplitAvgPower = (SplitIntAvgPowerL * SPLIT_INT_AVG_POWER_L_W_VALUE)
                      + (SplitIntAvgPowerH * SPLIT_INT_AVG_POWER_H_W_VALUE);

        // Projected Workout Time
        ProjectedWorkTimeL = PerformanceMonitorManager.StrokeData1[9];
        ProjectedWorkTimeM = PerformanceMonitorManager.StrokeData1[10];
        ProjectedWorkTimeH = PerformanceMonitorManager.StrokeData1[11];

        ProjectedWorkTime = (ProjectedWorkTimeL * PROJECTED_WORK_TIME_L_S_VALUE)
                          + (ProjectedWorkTimeM * PROJECTED_WORK_TIME_M_S_VALUE)
                          + (ProjectedWorkTimeH * PROJECTED_WORK_TIME_H_S_VALUE);

        // Projected Workout Distance
        ProjectedWorkDistanceL = PerformanceMonitorManager.StrokeData1[12];
        ProjectedWorkDistanceM = PerformanceMonitorManager.StrokeData1[13];
        ProjectedWorkDistanceH = PerformanceMonitorManager.StrokeData1[14];

        ProjectedWorkDistance = (ProjectedWorkDistanceL * PROJECTED_WORK_DISTANCE_L_M_VALUE)
                              + (ProjectedWorkDistanceM * PROJECTED_WORK_DISTANCE_M_M_VALUE)
                              + (ProjectedWorkDistanceH * PROJECTED_WORK_DISTANCE_H_M_VALUE);

        // Stroke data
        DragFactor = PerformanceMonitorManager.RowingStatusData[18];         // Drag Factor
        StrokesPerMin = PerformanceMonitorManager.RowingStatusData1[5];      // Stroke Rate
        DriveLength = PerformanceMonitorManager.StrokeData[6];               // Drive Length

        // Update graph every 0.5 seconds
        if (SampleCount > 5)
        {
            // Reset sample count
            SampleCount = 0;

            // Record graph data
            RecordGraphData();
        }
        else
        {
            SampleCount++;
        }

        UpdateDisplay();
    }

    private void UpdateGraph()
    {
        // Record graph data
        RecordGraphData();
    }

    public void ResetStats()
    {
        Distance = 0;
        Speed = 0;
        Time = 0;
        Pace = 0;
        SplitDist = 500;
        SplitTime = 0;
        SplitAvgPace = 0;
        SplitAvgPower = 0;
        ProjectedWorkTime = 0;
        ProjectedWorkDistance = 0;
        StrokesPerMin = 0;
        StrokePower = 0;
        AvgForce = 0;
        DriveLength = 0;
        DragFactor = 0;
    }

    private void UpdateDisplay()
    {
        SetDistanceDisplay(Distance);
        SetSpeedDisplay(Speed);
        SetTimeDisplay(Time);
        SetPaceDisplay(Pace);

        SetSplitDistDisplay((SplitDist == 0) ? 500 : SplitDist); // Default 500 unless set by user
        SetSplitTimeDisplay(SplitTime);
        SetSplitAvgPaceDisplay(SplitAvgPace);
        SetSplitAvgPowerDisplay(SplitAvgPower);

        SetProjectedWorkTimeDisplay(ProjectedWorkTime);
        SetProjectedWorkDistanceDisplay(ProjectedWorkDistance);

        SetStrokesPerMinDisplay(StrokesPerMin);
        SetPowerDisplay(StrokePower);
        SetAvgForceDisplay(AvgForce);

        SetDriveLengthDisplay(DriveLength);
        SetDragFactorDisplay(DragFactor);
    }

    private int dataPointCount = 0;
    private void RecordGraphData()
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
        PaceData.Dequeue();

        SplitTimeData.Dequeue();
        SplitAvgPaceData.Dequeue();
        SplitAvgPowerData.Dequeue();

        ProjectedWorkDistanceData.Dequeue();
        ProjectedWorkTimeData.Dequeue();

        StrokesPerMinData.Dequeue();
        StrokePowerData.Dequeue();

        DriveLengthData.Dequeue();
        DragFactorData.Dequeue();
        AvgForceData.Dequeue();
    }

    private void Enqueue()
    {
        DistanceData.Enqueue(Distance);
        TimeData.Enqueue(Time);
        SplitTimeData.Enqueue(SplitTime);
        SplitAvgPowerData.Enqueue(SplitAvgPower);
        ProjectedWorkDistanceData.Enqueue(ProjectedWorkDistance);
        ProjectedWorkTimeData.Enqueue(ProjectedWorkTime);
        DriveLengthData.Enqueue(DriveLength);
        SplitAvgPaceData.Enqueue(SplitAvgPace);

#if UNITY_EDITOR

        SpeedData.Enqueue(Random.Range(3, 5));
        PaceData.Enqueue(Random.Range(90, 126));
        DragFactorData.Enqueue(Random.Range(275, 300));
        AvgForceData.Enqueue(Random.Range(275, 300));
        StrokesPerMinData.Enqueue(Random.Range(36, 45));
        StrokePowerData.Enqueue(Random.Range(250, 350));

#else

        SpeedData.Enqueue(Speed);
        PaceData.Enqueue(Pace);
        SplitAvgPaceData.Enqueue(SplitAvgPace);
        DragFactorData.Enqueue(DragFactor);
        AvgForceData.Enqueue(AvgForce);
        StrokesPerMinData.Enqueue(StrokesPerMin);
        StrokePowerData.Enqueue(StrokePower);

#endif

    }

    private void SetDistanceDisplay(float distance)
    {
        DistanceDisplay.text = (int)distance + "m";
    }

    private void SetSpeedDisplay(float speed)
    {
        SpeedDisplay.text = string.Format("{0:0.00}", speed);
    }

    private void SetTimeDisplay(float seconds)
    {
        int[] hms = HelperFunctions.SecondsToHMS((int)seconds);

        TimeDisplay.text = hms[0].ToString("D2") + ":" + hms[1].ToString("D2") + ":" + hms[2].ToString("D2");
    }

    private void SetPaceDisplay(float seconds)
    {
        int[] hms = HelperFunctions.SecondsToHMS((int)seconds);

        PaceDisplay.text = hms[1].ToString("D1") + ":" + hms[2].ToString("D2");
    }

    private void SetSplitDistDisplay(float distance)
    {
        foreach (TMP_Text SplitDistDisplay in SplitDistDisplays)
        {
            SplitDistDisplay.text = distance.ToString();
        }
    }

    private void SetSplitTimeDisplay(float seconds)
    {
        //int[] hms = HelperFunctions.SecondsToHMS((int)seconds);

        //SplitTimeDisplay.text = hms[1] + ":" + hms[2].ToString("D2") + " /";
    }

    private void SetSplitAvgPaceDisplay(float splitAvgPace)
    {
        //int[] hms = HelperFunctions.SecondsToHMS((int)splitAvgPace);

        //SplitAvgPaceDisplay.text = hms[1].ToString("D1") + ":" + hms[2].ToString("D2");
    }

    private void SetSplitAvgPowerDisplay(float splitAvgPower)
    {
        //if (!SplitAvgPowerDisplay.enabled) return;

        //SplitAvgPowerDisplay.text = splitAvgPower.ToString();
    }

    private void SetStrokesPerMinDisplay(float strokesPerMin)
    {
        StrokesPerMinDisplay.text = strokesPerMin.ToString();
    }

    private void SetPowerDisplay(float power)
    {
        StrokePowerDisplay.text = power.ToString();
    }

    private void SetAvgForceDisplay(float avgForce)
    {
        AvgForceDisplay.text = ((int) avgForce).ToString();
    }

    private void SetDriveLengthDisplay(float driveLength)
    {
        //if (!DriveLengthDisplay.enabled) return;

        //DriveLengthDisplay.text = (driveLength / 100).ToString();
    }

    private void SetDragFactorDisplay(float dragFactor)
    {
        //DragFactorDisplay.text = dragFactor.ToString();
    }

    private void SetProjectedWorkTimeDisplay(float projectedWorkTime)
    {
        //if (!ProjectedWorkTimeDisplay.enabled) return;

        //ProjectedWorkTimeDisplay.text = projectedWorkTime.ToString();
    }

    private void SetProjectedWorkDistanceDisplay(float projectedWorkDist)
    {
        //if (!ProjectedWorkDistanceDisplay.enabled) return;

        //ProjectedWorkDistanceDisplay.text = projectedWorkDist.ToString();
    }

    public float GetDistance()
    {
        return Distance;
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

    public float GetAvgForce()
    {
        return (int) AvgForce;
    }

    public int GetStrokeState()
    {
        return StrokeState;
    }

    public float GetStrokeRate()
    {
        return StrokesPerMin;
    }

    public float GetPace()
    {
        return Pace;
    }

    public float GetAvgSplit()
    {
        return SplitAvgPace;
    }

    public void SetDebugDisplay(string value)
    {
        DebugDisplay.text = value;
    }

    public void SetPlayerController(PlayerController playerController)
    {
        Player = playerController;
    }
}