using System.Collections.Generic;
using System.Collections;
using System;

using UnityStandardAssets.Utility;
using UnityEngine;
using Photon.Pun;
using TMPro;

using static EventNotification;
using System.Linq;

public class Race : MonoBehaviour
{
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

    [SerializeField] GameObject opponentSlider;
    
    public PhotonView photonView { get; private set; }
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
        photonView = GetComponent<PhotonView>();
        route = GetComponent<Route>();
    }

    private void Reset()
    {
        state = RaceState.Inactive;

        players = new List<PlayerController>();
        positions = new Dictionary<PlayerController, int>();

        position = 0;

        pauseDuration = 0;
        raceDuration = TimeSpan.Zero;
        raceStartTime = 0;

        ResetDistanceSlider();
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
        // Don't execute if paused
        if (GameManager.State.Equals(GameManager.GameState.Paused))
        {
            if (PhotonNetwork.OfflineMode)
            {
                pauseDuration += Time.fixedDeltaTime;
            }

            return;
        }

        UpdateEventPanel();
    }

    private void UpdatePosition()
    {
        // Add player to completed race list
        photonView.RPC("RPC_UpdatePosition", RpcTarget.AllBufferedViaServer);
    }

    Dictionary<PlayerController, float> currentDistances = new Dictionary<PlayerController, float>();

    private float pauseDuration = 0;

    private void UpdateEventPanel()
    {
        // Clear distances dictionary
        currentDistances.Clear();

        // Reset local player progress
        float localPlayerProgress = 0;

        // For each player
        foreach (PlayerController player in players)
        {
            // Store progress
            float progress = player.GetRouteDistance();

            // Add progress to dictionary
            currentDistances.Add(player, progress);
            
            // Store local player progress
            if (player.photonView.IsMine) localPlayerProgress = progress;

            // Update progress bar
            GameManager.Instance.UpdatePlayerProgress(route, player, numberOfLaps);
        }

        // Convert dictionary to sorted list
        List<float> position = currentDistances.Values.ToList();
        position.Sort((a, b) => b.CompareTo(a));

        // For each player
        foreach (PlayerController player in players)
        {
            // Skip networked players
            if (!player.photonView.IsMine) continue;



            // Update duration
            raceDuration = TimeSpan.FromSeconds(PhotonNetwork.Time - (raceStartTime + pauseDuration));

            // Update lap
            int currentLap = player.GetCurrentLap();

            // Update position
            int playerPosition = position.IndexOf(localPlayerProgress) + 1; // 0 indexed
            int playerCount = players.Count();

            // Display event panel
            GameManager.Instance.DisplayEventPanel(
                raceDuration.ToString(@"mm\:ss"), 
                $"{currentLap}/{numberOfLaps}", 
                playerPosition.ToString() + "/" + playerCount.ToString()
            );

            // Local player found, stop checking
            break;
        }
    }

    private void ResetDistanceSlider()
    {
        // Don't execute if game paused
        if (GameManager.State.Equals(GameManager.GameState.Paused)) return;

        // For each player in race
        foreach (PlayerController player in players)
        {
            // Only execute on our view
            if (!player.photonView.IsMine) continue;

            // Update distance slider
            GameManager.Instance.ResetProgressBar();

            break;
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

        // Pause game
        GameManager.Instance.PauseGame();

        // If all players have finished the race
        if (positions.Count.Equals(players.Count))
        {
            // Resolve race
            SetState(RaceState.Resolving);
        }

        // Else if first player to finish the race
        else if (positions.Count.Equals(1))
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
            StartCoroutine(GameManager.Instance.DisplayQuickNotificationText("Race ending!", 3));
        }

        // Wait for other players to finish race
        yield return new WaitForSeconds(timeout);

        // Don't resolve if already over
        if (state.Equals(RaceState.Resolving) || state.Equals(RaceState.Inactive)) yield return new WaitForSeconds(0);

        // Resolve race
        SetState(RaceState.Resolving);
    }

    private void ResolveRace()
    {
        // Display race stats to participants
        StartCoroutine(DisplayEndOfRaceStats());

        // Set race to inactive
        SetState(RaceState.Inactive);
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
                StartCoroutine(GameManager.Instance.DisplayQuickNotificationText("Your Position: " + positions[player] + "!", 3));
            }

            // Otherwise
            else
            {
                // Display race ended message
                StartCoroutine(GameManager.Instance.DisplayQuickNotificationText("Race Ended", 3));
            }

            break;
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
            // Retrieve player view
            PhotonView playerView = player.GetComponent<PhotonView>();

            // Add player to shared race list
            photonView.RPC("RPC_AddPlayerToRace", RpcTarget.AllBufferedViaServer, playerView.ViewID);

            // Retrieve waypoint progress tracker
            RouteFollower routeFollower = player.GetComponent<RouteFollower>();

            // Update route
            routeFollower.UpdateRoute(route, numberOfLaps);

            // Reset progress
            player.ResetProgress();

            // Update player race
            player.UpdateRace(this);

            // Pause player movement
            player.Pause();
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

        // Instantiate tracker for networked payers
        if (!player.photonView.IsMine) GameManager.Instance.InstantiatePlayerTracker(player);

        // Display event information panel
        if (player.photonView.IsMine) GameManager.Instance.DisplayEventPanel("00:00", $"{0}/{numberOfLaps}", players.Count.ToString());
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

        // Remove player from race
        players.Remove(player);

        // Destroy tracker for networked payers
        if (!player.photonView.IsMine) GameManager.Instance.DestroyPlayerTracker(player);

        // Start just row
        GameManager.Instance.StartJustRow();

        // Hide all panels
        GameManager.Instance.HideAllPanels();

        // Reset event panel
        GameManager.Instance.ResetEventPanel();

        // Reset if the race is now empty
        if (players.Count <= 0) Reset();
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

            break;
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
            StartCoroutine(GameManager.Instance.SendEventNotification(EventCategory.Race, "Join Race", name, numberOfLaps.ToString(), players.Count.ToString(), startTimeoutDuration));

            break;
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

            // Start race countdown
            StartCoroutine(StartCountdown());

            break;
        }
    }

    IEnumerator StartCountdown()
    {
        // End Just Row
        //
        if (players.Count > 0) BluetoothManager.Instance.EndJustRow();

        // Reset stats
        //
        StatsManager.Instance.ResetStats();

        yield return new WaitForSeconds(2);

        // Display countdown 3
        //
        if (players.Count > 0) StartCoroutine(GameManager.Instance.DisplayCountdown("3", 1));

        yield return new WaitForSeconds(1);

        // Display countdown 2
        //
        if (players.Count > 0) StartCoroutine(GameManager.Instance.DisplayCountdown("2", 1));

        // Start Just Row
        //
        if (players.Count > 0) BluetoothManager.Instance.StartJustRow();

        yield return new WaitForSeconds(1);

        // Display countdown 1
        //
        if (players.Count > 0) StartCoroutine(GameManager.Instance.DisplayCountdown("1", 1));

        yield return new WaitForSeconds(1);

        // Display start!
        //
        if (players.Count > 0) StartCoroutine(GameManager.Instance.DisplayCountdown("Start!", 1));

        // Start race
        //
        if (players.Count > 0) StartRace();
    }

    public void StartRace()
    {
        // For each player in the race
        foreach (PlayerController player in players)
        {
            // Only execute on our view
            if (!player.photonView.IsMine) continue;

            // Retrieve toast panel
            Transform toastPanel = GameManager.Instance.transform.Find("HUD/Toast Panel");

            // Activate component if not current active
            if (!toastPanel.gameObject.activeSelf)
            {
                toastPanel.gameObject.SetActive(true);
                toastPanel.GetComponentInChildren<TMP_Text>().text = "";
            }
            
            // Resume player movement
            player.Resume();

            break;
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
        ResetDistanceSlider();
        RemoveAllPlayersFromRace();
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