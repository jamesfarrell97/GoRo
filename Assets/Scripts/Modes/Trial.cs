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
        CheckIfComplete();
    }

    private void UpdateStopWatch()
    {
        if (GameManager.State != GameManager.GameState.Playing) return;

        trialDuration = TimeSpan.FromSeconds(PhotonNetwork.Time - trialStartTime);
        DisplayDataToParticipants(trialDuration.ToString(@"mm\:ss"));
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

        foreach (string line in lines)
        {
            string[] data = line.Split(',');

            string name = data[0];

            if (name.Equals(route))
            {
                float speed = float.Parse(data[1]);

                LoadGhost(speed);
                
                return;
            }
        }
    }

    GameObject spawnedGhost;
    private void LoadGhost(float speed)
    {
        // Instantiate prefab
        spawnedGhost = Instantiate(trialGhost) as GameObject;
        
        // Instantiate values
        spawnedGhost.SendMessage("InstantiateGhostTrial", this);
        spawnedGhost.SendMessage("InstantiateGhostSpeed", speed);
        
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
    private void RecordTrialInformation(string route, float distance, double time)
    {
        // Retrieve trial file
        string file = HelperFunctions.ReadStringFromFile(TRIAL_GHOST_FILEPATH);

        if (file != null)
        {
            // Split file into lines
            string[] lines = file.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                string[] data = line.Split(',');

                string name = data[0];

                if (name.Equals(route))
                {
                    float routeSpeed = float.Parse(data[1]);
                    float playerSpeed = distance / (float) time;

                    if (playerSpeed > routeSpeed)
                    {
                        data[i] = name + "," + playerSpeed;
                        HelperFunctions.WriteArrayToFile(data, TRIAL_GHOST_FILEPATH);
                    }

                    return;
                }
            }
        }
        else
        {
            float playerSpeed = distance / (float) time;
            string[] data = new string[] { name + "," + playerSpeed };

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

    private void EndTrial()
    {
        RecordTrialInformation(name, route.routeDistance, trialDuration.TotalSeconds);
        RemovePlayerFromTrial();
        RemoveGhostFromTrial();
        Reset();
    }

    private void RemovePlayerFromTrial()
    {
        // Resume player movement
        player.Resume();

        // Start just row
        GameManager.Instance.StartJustRow();
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
        StartCoroutine(GameManager.Instance.DisplayCountdown("Time: " + trialDuration.ToString(@"mm\:ss"), 3));

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