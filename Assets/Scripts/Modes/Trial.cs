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
    
    public PhotonView photonView { get; private set; }
    public Route route { get; private set; }

    private PlayerController player;

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

    public void FormTrial(PlayerController player)
    {
        // Add player to race
        AddPlayerToTrial(player);

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

        // Retrieve waypoint progress tracker
        RouteFollower routeFollower = this.player.GetComponent<RouteFollower>();

        // Update route
        routeFollower.UpdateRoute(route, numberOfLaps);
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
        // Resume movement
        player.Resume();

        // Set start time
        trialStartTime = (float)PhotonNetwork.Time;

        // Update state
        SetState(TrialState.InProgress);
    }

    private void EndTrial()
    {
        RemovePlayerFromTrial();
        Reset();
    }

    private void RemovePlayerFromTrial()
    {
        // Resume player movement
        player.Resume();

        // Start just row
        GameManager.Instance.StartJustRow();
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