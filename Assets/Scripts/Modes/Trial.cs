using System.Collections;
using System;

using UnityStandardAssets.Utility;
using UnityEngine;
using Photon.Pun;

using static PlayerController;
using System.Collections.Generic;

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


    private float sampleTime = 0;
    private float sampleRate = 0.5f;
    private List<float> samples;

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
        sampleTime = 0;
        samples = new List<float>();

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
        SamplePlayerVelocity();
        CheckIfComplete();
    }

    private void UpdateStopWatch()
    {
        if (GameManager.State != GameManager.GameState.Playing) return;

        trialDuration = TimeSpan.FromSeconds(PhotonNetwork.Time - trialStartTime);
        DisplayDataToParticipants(trialDuration.ToString(@"mm\:ss"));
    }

    private void SamplePlayerVelocity()
    {
        // Update sample time
        sampleTime += Time.fixedDeltaTime;

        // If time since last sample exceeds sample rate
        if (sampleTime > sampleRate)
        {
            // Sample player velocity
            samples.Add(player.GetVelocity());

            // Reset sample time
            sampleTime = 0;
        }
    }

    private void CheckIfComplete()
    {
        // If player has completed time trial
        if (player.state == PlayerState.CompletedTimeTrial)
        {
            // Pause player movement
            player.Pause();

            // Display trial stats to participants
            StartCoroutine(DisplayEndOfTrialStats());

            // Resolve trial
            SetState(TrialState.Inactive);
        }
    }

    private void DisplayDataToParticipants(string time)
    {
        int currentLap = player.GetComponent<RouteFollower>().currentLap;
        GameManager.Instance.DisplayTimeAndLap(time, $"Lap: {currentLap}/{numberOfLaps}");
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

        // Pause player movement
        this.player.Pause();

        // Update player trial
        this.player.trial = this;

        // Retrieve route follower
        RouteFollower routeFollower = this.player.GetComponent<RouteFollower>();

        // Update route
        routeFollower.UpdateRoute(route, numberOfLaps);
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
                float[] speedSamples = new float[sampleSize];

                // Loop through samples
                for (int i = 0; i < sampleSize; i++)
                {
                    // Construct samples array
                    speedSamples[i] = float.Parse(samples[i]);
                }

                // Load ghost
                LoadGhost(speedSamples);
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
    }

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
                    float storedTime = float.Parse(data[1]);

                    // If player time is less than the stored time
                    if (time < storedTime)
                    {
                        // Update data array
                        data[i] = name + "|" + time + "|" + samples.Count + "|" + string.Join(",", samples) + "\n";

                        // Overwrite file
                        HelperFunctions.WriteArrayToFile(data, TRIAL_GHOST_FILEPATH);
                    }

                    // No need to check any more! Return
                    return;
                }
            }
        }

        // File must not be created yet
        else
        {
            // Create data array
            string[] data = new string[] { name + "|" + time + "|" + samples.Count + "|" + string.Join(",", samples) + "\n" };

            // Write data array to file
            HelperFunctions.WriteArrayToFile(data, TRIAL_GHOST_FILEPATH);
        }
    }

    IEnumerator StartCountdown()
    {
        // Display countdown 3
        //
        StartCoroutine(GameManager.Instance.DisplayCountdown("3", 1));

        yield return new WaitForSeconds(1);

        // Display countdown 2
        //
        StartCoroutine(GameManager.Instance.DisplayCountdown("2", 1));

        yield return new WaitForSeconds(1);

        // Display countdown 1
        //
        StartCoroutine(GameManager.Instance.DisplayCountdown("1", 1));

        yield return new WaitForSeconds(1);

        // Display start!
        //
        StartCoroutine(GameManager.Instance.DisplayCountdown("Start!", 1));

        // Start trial
        StartTrial();
    }

    private void StartTrial()
    {
        // Resume player movement
        player.Resume();

        // Resume ghost movement
        if (ghost != null) ghost.Resume();

        // Set start time
        trialStartTime = (float) PhotonNetwork.Time;

        // Update state
        SetState(TrialState.InProgress);
    }

    public void EndTrial()
    {
        RemovePlayerFromTrial();
        RecordTrialInformation(name, trialDuration.TotalSeconds);
        RemoveGhostFromTrial();
        Reset();
    }

    public void RemovePlayerFromTrial()
    {
        // Resume player movement
        player.Resume();

        // Start just row
        GameManager.Instance.StartJustRow();

        // Hide all panels
        GameManager.Instance.HideAllPanels();
    }

    private void RemoveGhostFromTrial()
    {
        Destroy(ghost);
        Destroy(spawnedGhost);
    }

    private void DisplayTimeTrialDataToParticipants(string time)
    {
        int currentLap = player.GetComponent<RouteFollower>().currentLap;

        GameManager.Instance.DisplayTimeAndLap(time, $"Lap: {currentLap}/{numberOfLaps}");
    }

    IEnumerator DisplayEndOfTrialStats()
    {
        // Display player position
        StartCoroutine(GameManager.Instance.DisplayQuickNotificationText("Time: " + trialDuration.ToString(@"mm\:ss"), 3));

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