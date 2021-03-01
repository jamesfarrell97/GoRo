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
    public enum RaceState
    {
        Inactive,
        Forming,
        InProgress,
        Resolving
    }

    public RaceState state;

    [SerializeField] public int numberOfLaps;

    [SerializeField] [Range(0, 30)] int startTimeoutDuration;
    [SerializeField] [Range(0, 30)] int positionTimeoutDuration;
    [SerializeField] [Range(0, 30)] int resolveTimeoutDuration;

    [HideInInspector] public Transform[] route;

    private PhotonView photonView;
    private WaypointCircuit waypointCircuit;

    private List<PlayerController> players;
    private Dictionary<PlayerController, int> positions;

    private int positionIndex;

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
        waypointCircuit = GetComponent<WaypointCircuit>();
    }

    private void Reset()
    {
        state = RaceState.Inactive;

        route = GetComponentsInChildren<Transform>();

        players = new List<PlayerController>();
        positions = new Dictionary<PlayerController, int>();

        positionIndex = 0;

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
            // Only execute on our view
            if (player.photonView.IsMine) continue;

            // Update duration
            raceDuration = TimeSpan.FromSeconds(PhotonNetwork.Time - raceStartTime);

            // Display duration
            DisplayDataToParticipants(raceDuration.ToString(@"mm\:ss"));
        }
    }

    private void CheckIfComplete()
    {
        // If all players have finished the race
        if (positions.Count.Equals(players.Count))
        {
            // Resolve race
            SetState(RaceState.Resolving);

            // Return
            return;
        }

        // For each player in race
        foreach (PlayerController player in players)
        {
            // Retrieve player view
            PhotonView playerView = player.GetComponent<PhotonView>();

            // Check if player complete race
            photonView.RPC("RPC_CheckIfPlayerComplete", RpcTarget.AllBufferedViaServer, playerView.ViewID);
        }
    }

    [PunRPC]
    private void RPC_CheckIfPlayerComplete(int playerID)
    {
        // Retrieve player view
        PhotonView playerView = PhotonView.Find(playerID);

        // Retrieve player
        PlayerController player = playerView.GetComponent<PlayerController>();
        
        // Don't execute for finished players
        if (positions.ContainsKey(player)) return;
        
        // If player has reached the finish line
        if (player.state == PlayerState.AtRaceFinishLine)
        {
            // Assign player to position
            positions.Add(player, ++positionIndex);
            
            // Pause player movement
            player.Pause();

            // Begin position timeout - Will need to determine way of resetting this timeout for each player across the line
            StartCoroutine(StartPositionTimeout(positionTimeoutDuration));
        }
    }

    IEnumerator StartPositionTimeout(float timeout)
    {
        // For each player in race
        foreach (PlayerController player in players)
        {
            // Don't display for finished players
            if (!positions.ContainsKey(player))
            {
                // Display race ending message
                StartCoroutine(GameManager.Instance.DisplayCountdown("Race ending!", 3));
            }
        }

        // Wait for other players to finish race
        yield return new WaitForSeconds(timeout);

        // Resolve race
        SetState(RaceState.Resolving);
    }

    private void ResolveRace()
    {
        // Resolve race
        photonView.RPC("RPC_ResolveRace", RpcTarget.AllBufferedViaServer);

        // Set race to inactive
        SetState(RaceState.Inactive);
    }

    [PunRPC]
    private void RPC_ResolveRace()
    {
        // Display race stats to participants
        StartCoroutine(DisplayEndOfRaceStats());

        // Remove all players from race
        RemoveAllPlayersFromRace();
    }

    IEnumerator DisplayEndOfRaceStats()
    {
        // For each player
        foreach (PlayerController player in players)
        {
            // If not our view, continue
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
            WaypointProgressTracker routeFollower = player.GetComponent<WaypointProgressTracker>();

            // Update route
            routeFollower.UpdateRoute(waypointCircuit, numberOfLaps);
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
        // Retrieve player view
        PhotonView playerView = PhotonView.Find(playerID);

        // Retrieve player
        PlayerController player = playerView.GetComponent<PlayerController>();

        // Remove player from list
        players.Remove(player);

        // Resume player movement
        player.Resume();

        // Reset player race
        player.race = null;

        // Hide time and lap info
        GameManager.Instance.HideTimeAndLap();

        // Start just row
        GameManager.Instance.StartJustRow();

        // Update player state
        player.state = PlayerState.JustRowing;
    }

    public void RemoveAllPlayersFromRace()
    {
        // Remove all players from race
        photonView.RPC("RPC_RemoveAllPlayersFromRace", RpcTarget.AllBufferedViaServer);
    }

    [PunRPC]
    private void RPC_RemoveAllPlayersFromRace()
    {
        // For each player in the race
        foreach (PlayerController player in players)
        {
            // Only execute for our player
            if (!player.photonView.IsMine) continue;

            // Remove player from race
            RemovePlayerFromRace(player);
        }
    }

    public void FormRace(PlayerController player)
    {
        // Add player to race
        AddPlayerToRace(player);

        // If multiplayer
        if (!PhotonNetwork.OfflineMode)
        {
            // Form race
            photonView.RPC("RPC_FormRace", RpcTarget.AllBufferedViaServer);
        }
        else
        {
            // Start race countdown
            StartCoroutine(StartCountdown());
        }
        
        // Set state
        SetState(RaceState.Forming);
    }

    [PunRPC]
    private void RPC_FormRace()
    {
        // Send notification to players that a race is about to begin
        SendRaceNotification();

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
        photonView.RPC("RPC_SetRaceStartTime", RpcTarget.AllBufferedViaServer);

        // Update race state
        SetState(RaceState.InProgress);
    }

    [PunRPC]
    public void RPC_SetRaceStartTime()
    {
        raceStartTime = (float) PhotonNetwork.Time;
    }

    public void EndRace()
    {
        RemoveAllPlayersFromRace();
        Reset();
    }

    private void DisplayDataToParticipants(string time)
    {
        foreach (PlayerController player in players)
        {
            PhotonView playerView = player.GetComponent<PhotonView>();

            if (!playerView.IsMine) continue;

            int currentLap = player.GetComponent<WaypointProgressTracker>().currentLap;

            GameManager.Instance.DisplayTimeAndLap(time, $"Lap: {currentLap}/{numberOfLaps}");

            return;
        }
    }

    private void SendRaceNotification()
    {
        // Retrieve all currently active players
        PlayerController[] activePlayers = FindObjectsOfType<PlayerController>();

        // For each active player
        foreach (PlayerController player in activePlayers)
        {
            // Don't display for players currently in the race
            if (players.Contains(player)) continue;
            
            // Send race notification
            StartCoroutine(GameManager.Instance.DisplayCountdown("Race Starting", 3));
        }
    }

    [PunRPC]
    private void RPC_SendRaceNotification(int countdown)
    {
        // Don't display on our own screen
        if (photonView.IsMine) return;

        // Send race notification
        StartCoroutine(GameManager.Instance.DisplayCountdown("Race Starting", countdown));
    }

    private void SetState(RaceState state)
    {
        // Update race state
        photonView.RPC("RPC_SetState", RpcTarget.AllBufferedViaServer, new object[] { (int) state });
    }

    [PunRPC]
    private void RPC_SetState(int state)
    {
        switch(state)
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
