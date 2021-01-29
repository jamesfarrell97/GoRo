using UnityEngine;
using TMPro;

public class Stats : MonoBehaviour
{
    [SerializeField] TMP_Text SplitTimeDisplay;
    [SerializeField] TMP_Text SplitDistDisplay;
    [SerializeField] TMP_Text TimeDisplay;
    [SerializeField] TMP_Text DistanceDisplay;

    [SerializeField] TMP_Text StrokesPerMinDisplay;
    [SerializeField] TMP_Text StrokePowerDisplay;
    [SerializeField] TMP_Text DriveLengthDisplay;

    // TODO: EXTRACT INTO APPDATA SETTINGS FILE
    public static readonly int[] SplitDistances = { 100, 500, 1000, 1500, 5000 };
    public static readonly float KILOMETERS_METER = 1000;
    public static readonly float SECS_MINUTE = 5;

    private int SplitIterator = 0;

    private int PreviousDistance = 0;
    private float PreviousTime = 0;

    private int MetersRowed;
    private float SecondsRowing;

    private int SplitTime;
    private int SplitDist;
    private int StrokesPerMin;
    private int StrokePower;
    private float DriveLength;

    private void Awake()
    {
        ResetStats();
        ResetDisplay();
    }

    private void Update()
    {
        UpdateTime();
        UpdateSplit();
        UpdateStats();
        UpdateDisplay();
    }

    private void UpdateTime()
    {
        SecondsRowing += Time.deltaTime;
    }

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

        int deltaDist = MetersRowed - PreviousDistance;
        int deltaTime = (int) (SecondsRowing - PreviousTime);

        if (deltaDist == 0) return;

        float deltaFactor = int.Parse(SplitDistDisplay.text) / deltaDist;

        int seconds = (int) (deltaFactor * deltaTime);

        PreviousDistance = MetersRowed;
        PreviousTime = SecondsRowing;
    }

    private void UpdateStats()
    {
        // TODO: PULL VALUES FROM CONCEPT2
        //
        // MetersRowed = PMCommunication.RowingData[1];
        // StrokePower = PMCommunication.RowingData[2];
        // StrokesPerMin = PMCommunication.RowingData[3];
        // DriveLength = PMCommunication.RowingData[4];
    }

    private void ResetStats()
    {
        SplitTime = 0;
        MetersRowed = 0;
        SecondsRowing = 0;
        StrokePower = 0;
        StrokesPerMin = 0;
        DriveLength = 0;
    }

    private void ResetDisplay()
    {
        SetSplitDistDisplay(SplitDistances[SplitIterator]);
        SetSplitTimeDisplay(0);
        SetTimeDisplay(0);
        SetDistanceDiplay(0);
        SetStrokesPerMinDisplay(0);
        SetPowerDisplay(0);
        SetDriveLengthDisplay(0);
    }

    private void UpdateDisplay()
    {
        SetSplitTimeDisplay(SplitTime);
        SetTimeDisplay((int) SecondsRowing);
        SetDistanceDiplay(MetersRowed);
        SetStrokesPerMinDisplay(StrokesPerMin);
        SetPowerDisplay(StrokePower);
        SetDriveLengthDisplay(DriveLength);
    }

    private void SetSplitDistDisplay(int distance)
    {
        if (!SplitDistDisplay.enabled) return;

        SplitDistDisplay.text = distance.ToString();
    }

    private void SetSplitTimeDisplay(int seconds)
    {
        if (!SplitTimeDisplay.enabled) return;

        int[] hms = HelperFunctions.SecondsToHMS(seconds);

        SplitTimeDisplay.text = hms[1] + ":" + hms[2].ToString("D2");
    }

    private void SetTimeDisplay(int seconds)
    {
        if (!TimeDisplay.enabled) return;

        int[] hms = HelperFunctions.SecondsToHMS(seconds);

        TimeDisplay.text = hms[0].ToString("D2") + ":" + hms[1].ToString("D2") + ":" + hms[2].ToString("D2");
    }

    private void SetDistanceDiplay(int distance)
    {
        if (!DistanceDisplay.enabled) return;

        DistanceDisplay.text = distance + "m";
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

        DriveLengthDisplay.text = driveLength + "m";
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
        return MetersRowed;
    }

    public int GetSecondsRowing()
    {
        return (int) SecondsRowing;
    }
}
