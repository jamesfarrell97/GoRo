using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public enum RaceState
    {
        Inactive,
        Forming,
        InProgress,
        Resolving
    }

    public RaceState state;

    [SerializeField] Transform start;
    [SerializeField] Transform finish;

    [SerializeField] [Range(0, 30)] int startTimeoutDuration;
    [SerializeField] [Range(0, 30)] int positionTimeoutDuration;
    [SerializeField] [Range(0, 30)] int resolveTimeoutDuration;

    private PhotonView photonView;

    private List<TestController> players;
    private Dictionary<TestController, int> positions;

    private int positionIndex;

    private float minDistanceToFinish = 1f;

    private void Start()
    {
        Setup();
        Reset();
    }

    private void Setup()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Reset()
    {
        state = RaceState.Inactive;

        players = new List<TestController>();
        positions = new Dictionary<TestController, int>();

        positionIndex = 0;
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

                CheckIfComplete();
                break;

            // Race resolving
            case RaceState.Resolving:

                ResolveRace();
                break;
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

        // For each player
        foreach (TestController player in players)
        {
            // Player already finished race
            if (positions.ContainsKey(player)) return;

            // Retrieve current players distance to finish line
            float distanceToFinish = Vector3.Distance(player.transform.position, finish.position);

            // If player has reached the finish line
            if (distanceToFinish < minDistanceToFinish)
            {
                // Assign player to position
                positions.Add(player, positionIndex++);

                // Place player at finish line
                player.transform.SetPositionAndRotation(finish.position, finish.rotation);

                // Pause player movement
                player.Pause();

                // Begin position timeout - Will need to determine way of resetting this timeout for each player across the line
                StartCoroutine(StartPositionTimeout(positionTimeoutDuration));
            }
        }
    }

    IEnumerator StartPositionTimeout(float timeout)
    {
        // For each player in race
        foreach (TestController player in players)
        {
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
    }

    IEnumerator DisplayEndOfRaceStats()
    {
        // For each player who finished the race
        foreach (TestController player in positions.Keys)
        {
            // Display end of race stats
            StartCoroutine(GameManager.Instance.DisplayCountdown("Your position: " + positions[player], 3));
        }

        // Display stats for resolve timeout seconds
        yield return new WaitForSeconds(resolveTimeoutDuration);

        // End race
        EndRace();

        // Set race to inactive
        SetState(RaceState.Inactive);
    }

    public void AddPlayerToRace(TestController player)
    {
        if (!players.Contains(player))
        {
            // Pause player movement
            player.Pause();

            // Retrieve player view
            PhotonView playerView = player.GetComponent<PhotonView>();

            // Add player to shared race list
            photonView.RPC("RPC_AddPlayerToRace", RpcTarget.AllBufferedViaServer, playerView.ViewID);

            // Place player on start line
            player.transform.SetPositionAndRotation(start.position, start.rotation);
        }
    }

    [PunRPC]
    private void RPC_AddPlayerToRace(int playerID)
    {
        // Retrieve player view
        PhotonView playerView = PhotonView.Find(playerID);

        // Retrieve player
        TestController player = playerView.GetComponent<TestController>();

        // Add player to list
        players.Add(player);
    }

    public void RemovePlayerFromRace(TestController player)
    {
        if (!players.Contains(player))
        {
            // Retrieve player view
            PhotonView playerView = player.GetComponent<PhotonView>();

            // Add player to shared race list
            photonView.RPC("RPC_RemovePlayerFromRace", RpcTarget.AllBufferedViaServer, playerView.ViewID);
        }
    }

    [PunRPC]
    private void RPC_RemovePlayerFromRace(int playerID)
    {
        // Retrieve player view
        PhotonView playerView = PhotonView.Find(playerID);

        // Retrieve player
        TestController player = playerView.GetComponent<TestController>();

        // Add player to list
        players.Remove(player);
    }

    public void RemoveAllPlayersFromRace()
    {
        // Remove all players from race
        photonView.RPC("RPC_RemoveAllPlayersFromRace", RpcTarget.AllBufferedViaServer);
        
        // For each player
        foreach (TestController player in players)
        {
            // Resume movement
            player.Resume();
        }
    }

    [PunRPC]
    private void RPC_RemoveAllPlayersFromRace()
    {
        // Clear player list
        players.Clear();
    }

    public void FormRace(TestController player)
    {
        // Add player to race
        AddPlayerToRace(player);

        // If multiplayer
        if (!PhotonNetwork.OfflineMode)
        {
            // Send notification to players that a race is about to begin
            SendRaceNotification();

            // Start race timeout
            StartCoroutine(StartRaceTimeout(startTimeoutDuration));
        }
        else
        {
            // Start race countdown
            StartCoroutine(StartRaceCountdown());
        }
    }

    IEnumerator StartRaceTimeout(float timeout)
    {
        // Wait for other players to join race
        yield return new WaitForSeconds(timeout);

        // Start race countdown
        StartCoroutine(StartRaceCountdown());
    }

    IEnumerator StartRaceCountdown()
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
        foreach (TestController player in players)
        {
            // Resume player movement
            player.Resume();
        }

        SetState(RaceState.InProgress);
    }

    public void EndRace()
    {
        RemoveAllPlayersFromRace();
        Reset();
    }

    private void SendRaceNotification()
    {
        // Retrieve all currently active players
        TestController[] activePlayers = FindObjectsOfType<TestController>();

        // For each active player
        foreach (TestController player in activePlayers)
        {
            // Send race notification
            StartCoroutine(GameManager.Instance.DisplayCountdown("Race Starting", 10));
        }
    }

    private void SetState(RaceState state)
    {
        // Update race state
        photonView.RPC("RPC_SetState", RpcTarget.AllBufferedViaServer, (int) state);
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
