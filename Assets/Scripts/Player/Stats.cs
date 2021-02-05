using UnityEngine;
using TMPro;

public class Stats : MonoBehaviour
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

    private float DistanceL;
    private float DistanceM;
    private float DistanceH;

    // Measured as 0.01 meters per least-significant bit
    private const float DISTANCE_L_METER_VALUE = 0.01f;                                     // Max       2.56 Meters
    private const float DISTANCE_M_METER_VALUE = DISTANCE_L_METER_VALUE * 256;              // Max     655.35 Meters
    private const float DISTANCE_H_METER_VALUE = DISTANCE_M_METER_VALUE * 256;              // Max 167,769.60 Meters

    private float ElapsedTimeL;
    private float ElapsedTimeM;
    private float ElapsedTimeH;

    // Measured as 0.01 seconds per least-significant bit
    private const float ELAPSED_TIME_L_SECOND_VALUE = 0.01f;                                // Max       2.55 Seconds
    private const float ELAPSED_TIME_M_SECOND_VALUE = ELAPSED_TIME_L_SECOND_VALUE * 256;    // Max     655.35 Seconds (11 Minutes)
    private const float ELAPSED_TIME_H_SECOND_VALUE = ELAPSED_TIME_M_SECOND_VALUE * 256;    // Max 167,769.60 Seconds (46 Hours)

    private float SpeedL;
    private float SpeedH;

    // Measured as 0.001 m/s per least-significant bit
    private const float SPEED_L_MPS_VALUE = 0.001f;                                         // Max       2.55 m/s
    private const float SPEED_H_MPS_VALUE = SPEED_L_MPS_VALUE * 256;                        // Max     655.35 m/s

    private int count = 0;
    private void RetrieveStats()
    {
        // Distance Covered (Meters)
        DistanceL = BluetoothManager.RowingStatusData[3];           // Distance Lo
        DistanceM = BluetoothManager.RowingStatusData[4];           // Distance Mid
        DistanceH = BluetoothManager.RowingStatusData[5];           // Distance Hi

        DistanceCovered = (DistanceH * DISTANCE_H_METER_VALUE)
                        + (DistanceM * DISTANCE_M_METER_VALUE)
                        + (DistanceL * DISTANCE_L_METER_VALUE) * 10;

        // Time Rowing (Seconds)
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

        StrokeState = BluetoothManager.RowingStatusData[10];        // Stroke State
        StrokePower = BluetoothManager.StrokeData1[3];              // Average Power L

        StrokesPerMin = BluetoothManager.RowingStatusData1[5];      // Stroke Rate
        DriveLength = BluetoothManager.StrokeData[6];               // Drive Length

#if !UNITY_EDITOR
        if (count > 50)
        {
            Debug.Log("DUNITY: Meters Rowed Low: " + DistanceL);
            Debug.Log("DUNITY: Meters Rowed Mid: " + DistanceM);
            Debug.Log("DUNITY: Meters Rowed High: " + DistanceH);
            Debug.Log("DUNITY: Meters Rowed: " + DistanceCovered);
            Debug.Log("DUNITY: Seconds Rowing Low: " + ElapsedTimeL);
            Debug.Log("DUNITY: Seconds Rowing Mid: " + ElapsedTimeM);
            Debug.Log("DUNITY: Seconds Rowing High: " + ElapsedTimeH);
            Debug.Log("DUNITY: Speed Low: " + SpeedL);
            Debug.Log("DUNITY: Speed High: " + SpeedH);
            Debug.Log("DUNITY: Speed: " + Speed);
            Debug.Log("DUNITY: Time Rowing: " + TimeRowing);
            Debug.Log("DUNITY: Stroke State: " + StrokeState);
            Debug.Log("DUNITY: Stroke Power: " + StrokePower);
            Debug.Log("DUNITY: SPM: " + StrokesPerMin);
            Debug.Log("DUNITY: Drive Length: " + DriveLength);
            count = 0;
        }
#endif

        count++;
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

    public int GetSpeed()
    {
        return (int) Speed;
    }
}
