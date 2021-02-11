using UnityEngine;
using TMPro;

public class StatsManager : MonoBehaviour
{
    // TODO: EXTRACT INTO APPDATA SETTINGS FILE
    public static readonly int[] SplitDistances = { 100, 500, 1000, 1500, 5000 };
    public static readonly float KILOMETERS_METER = 1000;
    public static readonly float SECS_MINUTE = 60;
    
    [SerializeField] private TMP_Text SplitTimeDisplay;
    [SerializeField] private TMP_Text SplitDistDisplay;
    [SerializeField] private TMP_Text TimeDisplay;
    [SerializeField] private TMP_Text DistanceDisplay;

    [SerializeField] private TMP_Text StrokesPerMinDisplay;
    [SerializeField] private TMP_Text StrokePowerDisplay;
    [SerializeField] private TMP_Text DriveLengthDisplay;

    private float DistanceCovered;
    private float TimeRowing;
    private float Speed;

    private float SplitTime;
    private float SplitDist;
    private float DriveLength;

    private int StrokesPerMin;
    private int StrokePower;
    private int StrokeState;

    private void Awake()
    {
        ResetStats();
        ResetDisplay();
    }

    private void Update()
    {
        RetrieveStats();
        UpdateSplit();
        UpdateDisplay();
    }

    // Measured as 0.1 meters per least-significant bit
    private const float DISTANCE_L_METER_VALUE = 0.1f;                                      // Max        25.5 Meters
    private const float DISTANCE_M_METER_VALUE = DISTANCE_L_METER_VALUE * 256;              // Max     6,553.5 Meters
    private const float DISTANCE_H_METER_VALUE = DISTANCE_M_METER_VALUE * 256;              // Max 1,677,721.5 Meters

    private float DistanceL;
    private float DistanceM;
    private float DistanceH;

    // Measured as 0.01 seconds per least-significant bit
    private const float ELAPSED_TIME_L_SECOND_VALUE = 0.01f;                                // Max        2.55 Seconds
    private const float ELAPSED_TIME_M_SECOND_VALUE = ELAPSED_TIME_L_SECOND_VALUE * 256;    // Max      655.35 Seconds (11 Minutes)
    private const float ELAPSED_TIME_H_SECOND_VALUE = ELAPSED_TIME_M_SECOND_VALUE * 256;    // Max  167,769.60 Seconds (46 Hours)

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

    private void RetrieveStats()
    {
        // Distance Covered (meters)
        DistanceL = BluetoothManager.RowingStatusData[3];           // Distance Lo
        DistanceM = BluetoothManager.RowingStatusData[4];           // Distance Mid
        DistanceH = BluetoothManager.RowingStatusData[5];           // Distance Hi

        DistanceCovered = (DistanceH * DISTANCE_H_METER_VALUE)
                        + (DistanceM * DISTANCE_M_METER_VALUE)
                        + (DistanceL * DISTANCE_L_METER_VALUE);

        // Time Rowing (seconds)
        ElapsedTimeL = BluetoothManager.RowingStatusData[0];        // Elapsed Time Lo
        ElapsedTimeM = BluetoothManager.RowingStatusData[1];        // Elapsed Time Mid
        ElapsedTimeH = BluetoothManager.RowingStatusData[2];        // Elapsed Time Hi

        TimeRowing = (ElapsedTimeH * ELAPSED_TIME_H_SECOND_VALUE)
                   + (ElapsedTimeM * ELAPSED_TIME_M_SECOND_VALUE)
                   + (ElapsedTimeL * ELAPSED_TIME_L_SECOND_VALUE);

        // Speed (m/s)
        SpeedL = BluetoothManager.RowingStatusData[0];              // Speed Lo
        SpeedH = BluetoothManager.RowingStatusData[1];              // Speed Hi

        Speed = (SpeedH * SPEED_H_MPS_VALUE)
              + (SpeedL * SPEED_L_MPS_VALUE);

        // Power (w)
        PowerL = BluetoothManager.StrokeData1[3];                   // Stroke Power Lo
        PowerH = BluetoothManager.StrokeData1[4];                   // Stroke Power Hi

        StrokePower = (PowerL * POWER_L_W_VALUE)
                    + (PowerH * POWER_H_W_VALUE);

        // Stroke data
        StrokeState = BluetoothManager.RowingStatusData[10];        // Stroke State
        StrokesPerMin = BluetoothManager.RowingStatusData1[5];      // Stroke Rate
        DriveLength = BluetoothManager.StrokeData[6];               // Drive Length
    }

    private int SplitIterator = 0;
    private float PreviousDistance = 0;
    private float PreviousTime = 0;

    private void UpdateSplit()
    {
        // The amount of distance that the user covered (deltaDist) and
        // the amount of time it took them to cover it (deltatime) can
        // be used to determine how long it will take the user to reach
        // their desired split distance
        //
        // This is done by dividing their deltaDist by their desired
        // splitDistance, and by then multiplying their deltaTime by
        // the resulting deltaFactor

        int deltaDist = (int) (DistanceL - PreviousDistance);
        int deltaTime = (int) (ElapsedTimeL - PreviousTime);

        if (deltaDist == 0) return;

        float deltaFactor = int.Parse(SplitDistDisplay.text) / deltaDist;

        int seconds = (int) (deltaFactor * deltaTime);

        PreviousDistance = DistanceL;
        PreviousTime = ElapsedTimeL;
    }

    private void UpdateDisplay()
    {
        SetSplitTimeDisplay(SplitTime);
        SetTimeDisplay(TimeRowing);
        SetDistanceDiplay(DistanceCovered);
        SetStrokesPerMinDisplay(StrokesPerMin);
        SetPowerDisplay(StrokePower);
        SetDriveLengthDisplay(DriveLength);
    }

    private void ResetStats()
    {
        SplitTime = 0;
        DistanceL = 0;
        ElapsedTimeL = 0;
        StrokePower = 0;
        StrokesPerMin = 0;
        DriveLength = 0;
    }

    private void ResetDisplay()
    {
        SplitIterator = 0;

        SetSplitDistDisplay(SplitDistances[SplitIterator]);
        SetSplitTimeDisplay(0);
        SetTimeDisplay(0);
        SetDistanceDiplay(0);
        SetStrokesPerMinDisplay(0);
        SetPowerDisplay(0);
        SetDriveLengthDisplay(0);
    }

    private void SetSplitDistDisplay(float distance)
    {
        if (!SplitDistDisplay.enabled) return;

        SplitDistDisplay.text = distance.ToString();
    }

    private void SetSplitTimeDisplay(float seconds)
    {
        if (!SplitTimeDisplay.enabled) return;

        int[] hms = HelperFunctions.SecondsToHMS((int) seconds);

        SplitTimeDisplay.text = hms[1] + ":" + hms[2].ToString("D2");
    }

    private void SetTimeDisplay(float seconds)
    {
        if (!TimeDisplay.enabled) return;

        int[] hms = HelperFunctions.SecondsToHMS((int) seconds);

        TimeDisplay.text = hms[0].ToString("D2") + ":" + hms[1].ToString("D2") + ":" + hms[2].ToString("D2");
    }

    private void SetDistanceDiplay(float distance)
    {
        if (!DistanceDisplay.enabled) return;

        DistanceDisplay.text = (int) distance + "m";
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

    public void CycleSplitDistance()
    {
        if (SplitIterator < SplitDistances.Length - 1)
        {
            SplitIterator++;
        }
        else
        {
            SplitIterator = 0;
        }

        SplitDist = SplitDistances[SplitIterator];
        SetSplitDistDisplay(SplitDist);
    }

    public int GetMetersRowed()
    {
        return (int) DistanceCovered;
    }

    public int GetSecondsRowing()
    {
        return (int) TimeRowing;
    }

    public float GetSpeed()
    {
        return Speed;
    }

    public int GetStrokeState()
    {
        return StrokeState;
    }
}
