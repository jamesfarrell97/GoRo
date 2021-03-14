using System.Collections.Generic;
using System.Collections;
using System;

using UnityStandardAssets.Utility;
using UnityEngine;
using Photon.Pun;
using TMPro;

using static PlayerController;
public class Race : MonoBehaviour
{
    #region Public Variables   
    [HideInInspector] public Transform[] track;
    //[HideInInspector] public List<PlayerController> players;
    [HideInInspector] public Dictionary<int, PlayerController> participantsCompletedRace;

    [HideInInspector] public bool raceInitiated = false;
    [HideInInspector] public bool raceInProgress = false;
    [HideInInspector] public bool raceComplete = false;

    [HideInInspector] public float waitTimeForOtherPlayersToJoin = 5f;
    [HideInInspector] public float timeRaceInitiated;
    [HideInInspector] public float timeRaceStarted;

    //[SerializeField] public TimeSpan raceDuration;
    [SerializeField] public int raceCapacity;
    #endregion Public Variables

    #region Private Variables
    private PlayerController participant;
    private float timeSecs;

    private bool gamePaused = false;
    private float durationOfRaceWithoutPauses;

    private float countdown = 4f;
    private float currentTimeInCountdown = 0;
    private int racePositionIndex = 1;
    #endregion Private Variables
    public enum RaceState
    {
        Inactive,
        Forming,
        InProgress,
        Resolving
    }

    public RaceState state;

    [SerializeField] [Range(0, 10)] public int numberOfLaps;
    [SerializeField] [Range(0, 30)] int startTimeoutDuration;
    [SerializeField] [Range(0, 30)] int positionTimeoutDuration;
    [SerializeField] [Range(0, 30)] int resolveTimeoutDuration;
    
    public PhotonView photonView { get; private set; }
    //public Route route { get; private set; }
    public Route route { get; private set; }

    private List<PlayerController> players;
    private Dictionary<PlayerController, int> positions;

    private int position;

    private TimeSpan raceDuration;
    private float raceStartTime;

    private void Start()
    {
        Setup();
        Reset();
    }

    private void Setup()
    {
        //track = FindObjectOfType<Race>().GetComponentsInChildren<Transform>();
        photonView = GetComponent<PhotonView>();
        participantsCompletedRace = new Dictionary<int, PlayerController>();
        route = GetComponent<Route>();
        track = route.Waypoints;
    }

    private void Reset()
    {
        state = RaceState.Inactive;

        players = new List<PlayerController>();
        positions = new Dictionary<PlayerController, int>();

        position = 0;

        raceDuration = TimeSpan.Zero;
        raceStartTime = 0;
    }

    private void FixedUpdate()
    {
        // Switch based on race state
        switch (state)
        {
            // Race inactive
            case RaceState.Inactive:

                // Do nothing
                return;

            // Race forming
            case RaceState.Forming:

                // Do nothing?
                return;

            // Race in progress
            case RaceState.InProgress:

                MonitorRace();
                break;

            // Race resolving
            case RaceState.Resolving:

                ResolveRace();
                break;
        }
    }

    private void MonitorRace()
    {
        UpdateStopWatch();
        CheckIfComplete();
    }

    private void UpdateStopWatch()
    {
        // Don't execute if game paused
        if (GameManager.State.Equals(GameManager.GameState.Paused)) return;

        // For each player in race
        foreach (PlayerController player in players)
        {
            route.routeStatus = Route.RouteStatus.Busy;

            // Pause player movement
            //participant.Pause();
            // Only execute on our view
            if (!player.photonView.IsMine) continue;

            // Update duration
            raceDuration = TimeSpan.FromSeconds(PhotonNetwork.Time - raceStartTime);

            // Display duration
            DisplayDataToParticipants(raceDuration.ToString(@"mm\:ss"));
        }
    }

    public void InitiateRace(float secondsForWait, int chosenNumberOfLaps, int chosenRaceCapacity)
    {
        route.routeStatus = Route.RouteStatus.InitiatedRace;
        raceInitiated = true;
        timeRaceInitiated = Time.timeSinceLevelLoad;

        waitTimeForOtherPlayersToJoin = secondsForWait;
        numberOfLaps = chosenNumberOfLaps;
        raceCapacity = chosenRaceCapacity;
    }

    private void CheckIfComplete()
    {
        // Check if complete
        photonView.RPC("RPC_CheckIfComplete", RpcTarget.AllBufferedViaServer);
    }

    [PunRPC]
    private void RPC_CheckIfComplete()
    {
        // If all players have finished the race
        if (positions.Count.Equals(players.Count))
        {
            // Resolve race
            SetState(RaceState.Resolving);
        }
    }

    public void PlayerCompletedRace(PlayerController player)
    {
        // Retrieve player view
        PhotonView playerView = player.GetComponent<PhotonView>();

        // Add player to completed race list
        photonView.RPC("RPC_PlayerCompletedRace", RpcTarget.AllBufferedViaServer, playerView.ViewID);
    }

    [PunRPC]
    public void RPC_PlayerCompletedRace(int playerID)
    {
        // Retrieve player view
        PhotonView playerView = PhotonView.Find(playerID);

        // Retrieve player
        PlayerController player = playerView.GetComponent<PlayerController>();

        // Update position
        position++;

        // Assign player to position
        positions.Add(player, position);

        // Pause player movement
        player.Pause();
        
        // If first player to finish the race
        if (positions.Count.Equals(1))
        {
            // Begin position timeout
            // Will need to determine way of resetting this timeout later for each player across the line
            StartCoroutine(StartPositionTimeout(positionTimeoutDuration));
        }
    }

    IEnumerator StartPositionTimeout(float timeout)
    {
        // For each player in race
        foreach (PlayerController player in players)
        {
            // Only display on our view
            if (!player.photonView.IsMine) continue;

            // Don't display for finished players
            if (positions.ContainsKey(player)) continue;
            
            // Display race ending message
            StartCoroutine(GameManager.Instance.DisplayCountdown("Race ending!", 3));
        }

        // Wait for other players to finish race
        yield return new WaitForSeconds(timeout);

        // Resolve race
        SetState(RaceState.Resolving);
    }

    private void ResolveRace()
    {
        // Display race stats to participants
        StartCoroutine(DisplayEndOfRaceStats());

        // Set race to inactive
        SetState(RaceState.Inactive);
        route.routeStatus = Route.RouteStatus.Available;
    }

    IEnumerator DisplayEndOfRaceStats()
    {
        // For each player
        foreach (PlayerController player in players)
        {
            // Only display on our view
            if (!player.photonView.IsMine) continue;

            // If player finished the race
            if (positions.ContainsKey(player))
            {
                // Display player position
                StartCoroutine(GameManager.Instance.DisplayCountdown("Position: " + positions[player], 3));
            }

            // Otherwise
            else
            {
                // Display race ended message
                StartCoroutine(GameManager.Instance.DisplayCountdown("Race Ended", 3));
            }
        }

        // Display stats for resolve timeout seconds
        yield return new WaitForSeconds(resolveTimeoutDuration);

        // End race
        EndRace();
    }

    public void AddPlayerToRace(PlayerController player)
    {
        // If player not already in race
        if (!players.Contains(player))
        {
            // Pause player movement
            player.Pause();

            // Update player race
            player.race = this;

            // Retrieve player view
            PhotonView playerView = player.GetComponent<PhotonView>();

            // Add player to shared race list
            photonView.RPC("RPC_AddPlayerToRace", RpcTarget.AllBufferedViaServer, playerView.ViewID);

            // Retrieve waypoint progress tracker
            RouteFollower routeFollower = player.GetComponent<RouteFollower>();

            // Update route
            routeFollower.UpdateRoute(route, numberOfLaps);
        }
    }

    [PunRPC]
    private void RPC_AddPlayerToRace(int playerID)
    {
        // Retrieve player view
        PhotonView playerView = PhotonView.Find(playerID);

        // Retrieve player
        PlayerController player = playerView.GetComponent<PlayerController>();

        // Add player to list
        players.Add(player);
    }

    public void RemovePlayerFromRace(PlayerController player)
    {
        // If player in race
        if (players.Contains(player))
        {
            // Retrieve player view
            PhotonView playerView = player.GetComponent<PhotonView>();

            // Remove player from shared race list
            photonView.RPC("RPC_RemovePlayerFromRace", RpcTarget.AllBufferedViaServer, playerView.ViewID);
        }
    }

    [PunRPC]
    private void RPC_RemovePlayerFromRace(int playerID)
    {
        // Assumes that this is only called from the RemoveAllPlayersFromRace function
        // A check may need to be added later when we want to remove a single player from a race
        // For example, for a player who wishes to leave mid-race

        // Retrieve player view
        PhotonView playerView = PhotonView.Find(playerID);

        // Retrieve player
        PlayerController player = playerView.GetComponent<PlayerController>();

        // Resume player movement
        player.Resume();

        // Start just row
        GameManager.Instance.StartJustRow();
    }

    public void RemoveAllPlayersFromRace()
    {
        // For each player in the race
        foreach (PlayerController player in players)
        {
            // Only execute for our player
            if (!player.photonView.IsMine) continue;
            
            // Remove player from race
            RemovePlayerFromRace(player);
        }

        // Reset
        Reset();
    }

    public void FormRace(PlayerController player)
    {
        // Add player to race
        AddPlayerToRace(player);

        // If multiplayer
        if (!PhotonNetwork.OfflineMode)
        {
            // Send notification to players that a race is about to begin
            SendRaceNotification();

            // Start race timeout
            StartRaceTimeout();
        }
        else
        {
            // Start race countdown
            StartCoroutine(StartCountdown());
        }

        // Set state
        SetState(RaceState.Forming);
    }

    public void SendRaceNotification()
    {
        // Send notification to players that a race is about to begin
        photonView.RPC("RPC_SendRaceNotification", RpcTarget.AllBufferedViaServer);
    }

    [PunRPC]
    private void RPC_SendRaceNotification()
    {
        // Retrieve all currently active players
        PlayerController[] activePlayers = FindObjectsOfType<PlayerController>();

        // For each active player
        foreach (PlayerController player in activePlayers)
        {
            // Only display on our view
            if (!player.photonView.IsMine) continue;

            // Don't display for players currently in the race
            if (players.Contains(player)) continue;

            // Send race notification
            StartCoroutine(GameManager.Instance.DisplayCountdown("Race Starting", 3));
        }
    }

    public void StartRaceTimeout()
    {
        // Start race timeout
        photonView.RPC("RPC_StartRaceTimeout", RpcTarget.AllBufferedViaServer);
    }

    [PunRPC]
    public void RPC_StartRaceTimeout()
    {
        // Start race timeout
        StartCoroutine(StartRaceTimeout(startTimeoutDuration));
    }

    IEnumerator StartRaceTimeout(float timeout)
    {
        // Wait for other players to join race
        yield return new WaitForSeconds(timeout);

        // For each player in race
        foreach (PlayerController player in players)
        {
            // Only display on our view
            if (!player.photonView.IsMine) continue;

            route.routeStatus = Route.RouteStatus.Busy;
            // Start race countdown
            StartCoroutine(StartCountdown());
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

        // Start race
        StartRace();
    }

    public void StartRace()
    {
        // For each player in the race
        foreach (PlayerController player in players)
        {
            // Resume movement
            player.Resume();

            // Retrieve notification container
            Transform notificationContainer = GameManager.Instance.transform.Find("HUD/Notification Cont");

            // Activate component if not current active
            if (!notificationContainer.gameObject.activeSelf)
            {
                notificationContainer.gameObject.SetActive(true);
                notificationContainer.GetComponentInChildren<TMP_Text>().text = "";
            }
        }

        // Set race start time
        SetRaceStartTime();

        // Update race state
        SetState(RaceState.InProgress);
    }

    public void SetRaceStartTime()
    {
        // Set race start time
        photonView.RPC("RPC_SetRaceStartTime", RpcTarget.AllBufferedViaServer);
    }

    [PunRPC]
    public void RPC_SetRaceStartTime()
    {
        raceStartTime = (float) PhotonNetwork.Time;
    }

    public void EndRace()
    {
        RemoveAllPlayersFromRace();
    }

    private void DisplayDataToParticipants(string time)
    {
        foreach (PlayerController player in players)
        {
            PhotonView playerView = player.GetComponent<PhotonView>();

            if (!playerView.IsMine) continue;

            int currentLap = player.GetComponent<RouteFollower>().currentLap;

            GameManager.Instance.DisplayTimeAndLap(time, $"Lap: {currentLap}/{numberOfLaps}");

            return;
        }
    }

    public void DiposeResources()
    {
        route.routeStatus = Route.RouteStatus.Available;
        players.Clear();
        participantsCompletedRace.Clear();
        raceInitiated = false;
        raceInProgress = false;
        raceComplete = false;
        timeRaceStarted = 0;
        timeSecs = 0;
        numberOfLaps = 0;
        countdown = 3f;
        currentTimeInCountdown = 0;
        timeRaceInitiated = 0;
        racePositionIndex = 1;
        durationOfRaceWithoutPauses = 0;
        SetState(RaceState.Inactive);
    }

    private void SetState(RaceState state)
    {
        // Update race state
        photonView.RPC("RPC_SetState", RpcTarget.AllBufferedViaServer, (int) state );
    }

    [PunRPC]
    private void RPC_SetState(int state)
    {
        switch (state)
        {
            case (int) RaceState.Inactive:
                this.state = RaceState.Inactive;
                break;

            case (int) RaceState.Forming:
                this.state = RaceState.Forming;
                break;

            case (int) RaceState.InProgress:
                this.state = RaceState.InProgress;
                break;

            case (int) RaceState.Resolving:
                this.state = RaceState.Resolving;
                break;
        }
    }
}