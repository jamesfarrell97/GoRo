using System.Collections.Generic;
using System.Collections;
using System;

using UnityStandardAssets.Utility;
using UnityEngine;
using Photon.Pun;

using static PlayerController;
public class Trial : MonoBehaviour
{
    public enum TrialState
    {
        Inactive,
        Forming,
        InProgress
    }

    public TrialState state;

    [SerializeField] [Range(0, 10)] public int numberOfLaps;
    [SerializeField] [Range(0, 30)] int resolveTimeoutDuration;

    [SerializeField] GameObject trialGhost;
    
    public PhotonView photonView { get; private set; }
    public Route route { get; private set; }

    private PlayerController player;
    private GhostController ghost;

    private TimeSpan trialDuration;
    private float trialStartTime;

    private void Start()
    {
        Setup();
        Reset();
    }

    private void Setup()
    {
        photonView = GetComponent<PhotonView>();
        route = GetComponent<Route>();
    }

    private void Reset()
    {
        state = TrialState.Inactive;
        player = null;

        trialDuration = TimeSpan.Zero;
        trialStartTime = 0;
    }

    private void FixedUpdate()
    {
        // Switch based on trial state
        switch (state)
        {
            // Trial inactive
            case TrialState.Inactive:

                // Do nothing
                return;

            // Trial forming
            case TrialState.Forming:

                // Do nothing
                break;

            // Trial in progress
            case TrialState.InProgress:

                MonitorTrial();
                break;
        }
    }

    private void MonitorTrial()
    {
        UpdateStopWatch();
        UpdateDistanceSlider();
        CheckIfComplete();
    }

    private void UpdateStopWatch()
    {
        trialDuration = TimeSpan.FromSeconds(PhotonNetwork.Time - trialStartTime);
        DisplayDataToParticipants(trialDuration.ToString(@"mm\:ss"));
    }

    private void UpdateDistanceSlider()
    {
        // Update distance slider
        GameManager.Instance.UpdatePlayerProgress(route, player, numberOfLaps);

        // Update ghost tracker
        if (ghost != null) GameManager.Instance.UpdateGhostTracker(route, ghost, numberOfLaps);
    }

    private void ResetDistanceSlider()
    {
        // Reset slide distance
        GameManager.Instance.ResetProgressBar();
    }

    private void CheckIfComplete()
    {
        // If player has completed time trial
        if (player.state == PlayerState.CompletedTimeTrial)
        {
            // Record trial information
            RecordTrialInformation(name, trialDuration.TotalSeconds);
            
            // Display trial stats to participants
            StartCoroutine(DisplayEndOfTrialStats());

            // Resolve trial
            SetState(TrialState.Inactive);
        }
    }

    private void DisplayDataToParticipants(string time)
    {
        string distance = StatsManager.Instance.GetMetersRowed().ToString();
        string speed = string.Format("{0:0.00}", StatsManager.Instance.GetSpeed());
        string strokeRate = StatsManager.Instance.GetStrokeRate().ToString();

        GameManager.Instance.DisplayEventPanel(time, distance, speed, strokeRate);
    }

    public void FormTrial(PlayerController player, string route)
    {
        // Add player to trial
        AddPlayerToTrial(player);

        // Retrieve trial information
        RetrieveTrialInformation(route);

        // Start countdown
        StartCoroutine(StartCountdown());

        // Update state
        SetState(TrialState.Forming);
    }

    public void AddPlayerToTrial(PlayerController player)
    {
        // Assign player
        this.player = player;

        // Retrieve route follower
        RouteFollower routeFollower = this.player.GetComponent<RouteFollower>();

        // Update route
        routeFollower.UpdateRoute(route, numberOfLaps);

        // Reset progress
        this.player.ResetProgress();

        // Reset samples
        this.player.ResetSamples();

        // Update player trial
        this.player.UpdateTrial(this);

        // Pause player movement
        this.player.Pause();

        // Display event information panel
        GameManager.Instance.DisplayEventPanel("00:00", "0", "0", "0");
    }

    private const string TRIAL_GHOST_FILEPATH = "trial-ghost-data";

    private void RetrieveTrialInformation(string route)
    {
        // Retrieve trial file
        string file = HelperFunctions.ReadStringFromFile(TRIAL_GHOST_FILEPATH);
        
        // If no file present, return
        if (file == null) return;

        // Split file into lines
        string[] lines = file.Split('\n');

        // For each line in the file
        foreach (string line in lines)
        {
            // Split each line into data cells
            string[] data = line.Split('|');

            // Extract route name
            string name = data[0];

            // If the route's name is equal to the current route
            if (name.Equals(route))
            {
                // Retrieve sample size
                int sampleSize = int.Parse(data[2]);

                // Retrieve samples
                string[] samples = data[3].Split(',');

                // Create samples array
                float[] distanceSamples = new float[sampleSize];

                // Loop through samples
                for (int i = 0; i < sampleSize; i++)
                {
                    // Construct samples array
                    distanceSamples[i] = float.Parse(samples[i]);
                }

                // Load ghost
                LoadGhost(distanceSamples);
                return;
            }
        }
    }

    GameObject spawnedGhost;

    private void LoadGhost(float[] speedSamples)
    {
        // Instantiate prefab
        spawnedGhost = Instantiate(trialGhost) as GameObject;
        
        // Instantiate values
        spawnedGhost.SendMessage("InstantiateGhostTrial", this);
        spawnedGhost.SendMessage("InstantiateGhostSamples", speedSamples);
        
        // Assign ghost
        ghost = spawnedGhost.GetComponent<GhostController>();

        // Pause movement
        ghost.Pause();

        // Assign trial
        ghost.trial = this;

        // Retrieve route follower
        RouteFollower routeFollower = ghost.GetComponent<RouteFollower>();

        // Update route
        routeFollower.UpdateRoute(route, numberOfLaps);

        // Reset progress
        ghost.ResetDistance();

        // Instantiate tracker
        GameManager.Instance.InstantiateGhostTracker(ghost);
    }

    private float storedTime;

    // Record Best time
    private void RecordTrialInformation(string route, double time)
    {
        // Retrieve trial file
        string file = HelperFunctions.ReadStringFromFile(TRIAL_GHOST_FILEPATH);

        // IF file present
        if (file != null)
        {
            // Split file into lines
            string[] lines = file.Split('\n');

            // For each line in the file
            for (int i = 0; i < lines.Length; i++)
            {
                // Retrieve each line
                string line = lines[i];

                // Split each line into data cells
                string[] data = line.Split('|');

                // Extract route name
                string name = data[0];

                // If the route's name is equal to the current route
                if (name.Equals(route))
                {
                    // Extract the stored time
                    storedTime = float.Parse(data[1]);

                    // If player time is less than the stored time
                    if (time < storedTime)
                    {
                        // Update file
                        lines[i] = route + "|" + time + "|" + player.DistanceSample.Count + "|" + string.Join(",", player.DistanceSample) + "\n";

                        // Overwrite file
                        HelperFunctions.WriteArrayToFile(lines, TRIAL_GHOST_FILEPATH);
                    }

                    // No need to check any more! Break
                    break;
                }
                else
                {
                    // Record trial data
                    string trialData = route + "|" + time + "|" + player.DistanceSample.Count + "|" + string.Join(",", player.DistanceSample) + "\n";

                    // Overwrite file
                    HelperFunctions.WriteStringToFile(trialData, TRIAL_GHOST_FILEPATH);

                    // No need to check any more! Break
                    break;
                }
            }
        }

        // File must not be created yet
        else
        {
            // Create data array
            string[] data = new string[] { name + "|" + time + "|" + player.DistanceSample.Count + "|" + string.Join(",", player.DistanceSample) + "\n" };

            // Write data array to file
            HelperFunctions.WriteArrayToFile(data, TRIAL_GHOST_FILEPATH);
        }
    }

    IEnumerator StartCountdown()
    {
        yield return new WaitForSeconds(2);

        if (player == null)
        {
            EndTrial();
            yield break;
        }

        // Display countdown 3
        //
        StartCoroutine(GameManager.Instance.DisplayCountdown("3", 1));

        yield return new WaitForSeconds(1);

        if (player == null)
        {
            EndTrial();
            yield break;
        }
        
        // Display countdown 2
        //
        StartCoroutine(GameManager.Instance.DisplayCountdown("2", 1));

        yield return new WaitForSeconds(1);

        if (player == null)
        {
            EndTrial();
            yield break;
        }

        // Display countdown 1
        //
        StartCoroutine(GameManager.Instance.DisplayCountdown("1", 1));

        yield return new WaitForSeconds(1);

        if (player == null)
        {
            EndTrial();
            yield break;
        }

        // Display start!
        //
        StartCoroutine(GameManager.Instance.DisplayCountdown("Start!", 1));

        // Display false start
        //
        if (StatsManager.Instance.GetDistance() > 0)
            StartCoroutine(GameManager.Instance.DisplayQuickNotificationText("False Start!", 2));

        // Reset PM
        //
        PerformanceMonitorManager.Instance.ResetPM();

        // Start trial
        //
        StartTrial();
    }

    private void StartTrial()
    {
        // Set start time
        trialStartTime = (float) PhotonNetwork.Time;

        // Unpause player
        if (player != null) player.Resume();

        // Unpause ghost
        if (ghost != null) ghost.Resume();

        // Update state
        SetState(TrialState.InProgress);
    }

    public void EndTrial()
    {
        RemovePlayerFromTrial();
        RemoveGhostFromTrial();
        ResetDistanceSlider();
        Reset();
    }

    public void RemovePlayerFromTrial()
    {
        // Start just row
        GameManager.Instance.StartJustRow();

        // Hide all panels
        GameManager.Instance.HideAllPanels();

        // Reset event panel
        GameManager.Instance.ResetEventPanel();
    }

    private void RemoveGhostFromTrial()
    {
        // Remove ghost tracker
        GameManager.Instance.DestroyGhostTracker(ghost);

        Destroy(ghost);
        Destroy(spawnedGhost);
    }

    IEnumerator DisplayEndOfTrialStats()
    {
        PerformanceMonitorManager.Instance.ResetPM();

        if (player != null) player.Pause();

        if (ghost != null) ghost.Pause();

        // Display information
        StartCoroutine(GameManager.Instance.DisplayQuickNotificationText("Time: " + trialDuration.ToString(@"mm\:ss"), 2));

        // If stored time present
        if (storedTime > 0)
        {
            yield return new WaitForSeconds(2);

            // Cacluate delta
            int[] delta = HelperFunctions.SecondsToHMS((int) (storedTime - (float) trialDuration.TotalSeconds));

            // Display delta
            StartCoroutine(GameManager.Instance.DisplayQuickNotificationText(
                (delta[2] > 0) 
                    ? "-" + Mathf.Abs(delta[1]).ToString("D2") + ":" + Mathf.Abs(delta[2]).ToString("D2")
                    : "+" + Mathf.Abs(delta[1]).ToString("D2") + ":" + Mathf.Abs(delta[2]).ToString("D2")
            , 2));
        }

        // Display stats for resolve timeout seconds
        yield return new WaitForSeconds(resolveTimeoutDuration);

        // End trial
        EndTrial();
    }

    private void SetState(TrialState state)
    {
        switch (state)
        {
            case TrialState.Inactive:
                this.state = TrialState.Inactive;
                break;

            case TrialState.InProgress:
                this.state = TrialState.InProgress;
                break;
        }
    }
}