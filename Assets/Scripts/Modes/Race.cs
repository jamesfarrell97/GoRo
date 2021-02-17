using System;
using System.Collections.Generic;

using UnityStandardAssets.Utility;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class Race : MonoBehaviour
{
    #region Public Variables   
    [HideInInspector] public Transform[] route;
    [HideInInspector] public List<PlayerController> players;
    [HideInInspector] public Dictionary<int, PlayerController> playersCompletedRace;

    [HideInInspector] public bool raceInitiated = false;
    [HideInInspector] public bool raceInProgress = false;
    [HideInInspector] public bool raceComplete = false;

    [HideInInspector] public float waitTimeForOtherPlayersToJoin = 5f;
    [HideInInspector] public float timeRaceInitiated;
    [HideInInspector] public float timeRaceStarted;

    [SerializeField] public TimeSpan raceDuration;
    [SerializeField] public int numberOfLaps;
    [SerializeField] public int raceCapacity;
    #endregion Public Variables

    #region Private Variables
    private PlayerController participant;
    private PhotonView photonView;
    private float timeSecs;

    private bool gamePaused = false;
    private float durationOfRaceWithoutPauses;

    private float countdown = 4f;
    private float currentTimeInCountdown = 0;
    private int racePositionIndex = 1;
    #endregion Private Variables
    
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        playersCompletedRace = new Dictionary<int, PlayerController>();

        route = FindObjectOfType<Race>().GetComponentsInChildren<Transform>();
    }

    void Update()
    {
        if (!gamePaused)
        {
            timeSecs += Time.deltaTime;

            if (raceInitiated)
            {
                if (raceComplete)
                {
                    EndRace();
                }
                else if (!raceInProgress)
                {
                    // If singleplayer
                    if (PhotonNetwork.OfflineMode)
                    {
                        // Start race immediately
                        StartCountdown();
                    }

                    // Otherwise, wait for other players to join
                    else if (players.Count == raceCapacity || (timeSecs) > waitTimeForOtherPlayersToJoin)
                    {
                        StartCountdown();
                        timeSecs = 0;
                    }
                }
                else
                {
                    UpdateStopWatch();
                }
            }
        }
    }

    // Code referenced: https://answers.unity.com/questions/369581/countdown-to-start-game.html
    private void StartCountdown()
    {
        foreach(PlayerController participant in players)
        {
            // Pause player movement
            participant.Pause();

            float delta = Time.deltaTime;
            currentTimeInCountdown += delta;

            if (currentTimeInCountdown >= 1)
            {
                if (countdown - 1 <= -1)
                {
                    StartRace();
                }
                else if (countdown - 1 <= 0)
                {
                    DisplayCountdownToParticipants("Start!");
                    countdown = 0;
                }
                else
                {
                    countdown -= 1;

                    DisplayCountdownToParticipants($"{countdown}");
                    currentTimeInCountdown = 0;
                }
            }
        }
    }

    public void InitiateRace(int numberOfLaps, int raceCapacity)
    {
        raceInitiated = true;
        timeRaceInitiated = Time.timeSinceLevelLoad;

        this.numberOfLaps = numberOfLaps;
        this.raceCapacity = raceCapacity;
    }

    private void UpdateStopWatch()
    {
        foreach (PlayerController participant in players)
        {
            raceDuration = TimeSpan.FromSeconds((timeSecs + durationOfRaceWithoutPauses) - timeRaceStarted);

            DisplayRaceDataToParticipants($"{raceDuration.ToString(@"mm\:ss")}");
        }
    }

    public void AddPlayerToRaceList(PlayerController player)
    {
        if (players.Count < raceCapacity)
        {
            // Retrieve player view
            PhotonView playerView = player.GetComponent<PhotonView>();

            // Add player to shared race list
            photonView.RPC("RPC_AddPlayerToRaceList", RpcTarget.AllBuffered, playerView.ViewID);

            // Mark player as participating in race
            player.participatingInRace = true;

            // Retrieve waypoint progress tracker
            WaypointProgressTracker wpt = player.GetComponent<WaypointProgressTracker>();

            // Update values
            wpt.Reset();
            wpt.UpdateLaps(numberOfLaps);
            wpt.UpdatePosition();
        }
    }

    [PunRPC]
    public void RPC_AddPlayerToRaceList(int playerID)
    {
        // Retrieve player view
        PhotonView playerView = PhotonView.Find(playerID);

        // Retrieve player controller
        PlayerController player = playerView.gameObject.GetComponent<PlayerController>();

        // Add player to players list
        players.Add(player);
    }

    public void AddPlayerToCompletedRaceList(PlayerController player)
    {
        // Retrieve player view
        PhotonView playerView = player.GetComponent<PhotonView>();

        // Add player to shared completed race list
        photonView.RPC("RPC_AddPlayerToCompletedRaceList", RpcTarget.AllBuffered, playerView.ViewID);

        // Mark player as participating in race
        player.participatingInRace = false;

        CheckIfRaceComplete();
    }

    [PunRPC]
    public void RPC_AddPlayerToCompletedRaceList(int playerID)
    {
        // Retrieve player view
        PhotonView playerView = PhotonView.Find(playerID);

        // Retrieve player controller
        PlayerController player = playerView.gameObject.GetComponent<PlayerController>();

        // Add player to completed race list
        playersCompletedRace.Add(racePositionIndex, player);

        // Update race position
        racePositionIndex++;
    }

    private void StartRace()
    {
        // Allow participants to move [May need to figure out a better way to allow movement, where they'll all start at the EXACT same time]
        foreach (PlayerController player in players)
        {
            // Resume player movement
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

        timeRaceStarted = Time.timeSinceLevelLoad;
        raceInProgress = true;
    }

    [PunRPC]
    private void CheckIfRaceComplete()
    {
        if (playersCompletedRace.Count == players.Count)
        {
            raceComplete = true;
        }
    }

    private void EndRace()
    {
        DisplayEndOfRaceStats();
        DisposeSessionResources();
    }

    // Pause singleplayer race if pause menu is opened
    public void PauseSingleplayerRace()
    {
        gamePaused = true;
        durationOfRaceWithoutPauses = durationOfRaceWithoutPauses + (timeSecs - timeRaceStarted);

        foreach (PlayerController player in players)
        {
            player.Pause();
        }
    }

    // Resume singleplayer race if pause menu is closed
    public void ResumeSingleplayerRace()
    {
        gamePaused = false;
        timeRaceStarted = Time.timeSinceLevelLoad;
        foreach (PlayerController player in players)
        {
            player.Resume();
        }
    }

    private void DisplayRaceDataToParticipants(string time)
    {
        foreach (PlayerController player in players)
        {
            PhotonView photonView = player.GetComponent<PhotonView>();

            if (!photonView.IsMine) continue;

            int currentLap = player.GetComponent<WaypointProgressTracker>().currentLap;

            GameManager.Instance.DisplayTimeAndLap(time, $"Lap: {currentLap}/{numberOfLaps}");

            return;
        }
    }

    private void DisplayCountdownToParticipants(string count)
    {
        foreach (PlayerController player in players)
        {
            PhotonView photonView = player.GetComponent<PhotonView>();

            if (!photonView.IsMine) continue;

            StartCoroutine(GameManager.Instance.DisplayCountdown(count, 3));

            return;
        }
    }

    private void DisplayTextToParticipants(string text, int time = 0)
    {
        foreach (PlayerController player in players)
        {
            PhotonView photonView = player.GetComponent<PhotonView>();

            if (!photonView.IsMine) continue;

            StartCoroutine(GameManager.Instance.DisplayQuickNotificationText(text, time));

            return;
        }
    }

    private void DisplayEndOfRaceStats()
    {
        foreach (KeyValuePair<int, PlayerController> player in playersCompletedRace)
        {
            string text = $"Your position within the race: {player.Key}";
            StartCoroutine(GameManager.Instance.DisplayQuickNotificationText(text, 6));
        }
    }

    // Reset all datatypes back to their initial state, after a race is finished
    private void DisposeSessionResources()
    {
        Debug.Log($"DisposeSessionResources entered for race");
        ResetRaceStatsForParticipants();

        foreach (PlayerController player in players)
        {
            GameManager.Instance.StartJustRow();
        }

        players.Clear();
        playersCompletedRace.Clear();
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
    }

    private void ResetRaceStatsForParticipants()
    {
        foreach(PlayerController participant in players)
        {
            participant.participatingInRace = false;
        }
    }
}
