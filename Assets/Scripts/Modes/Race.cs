using System.Collections;
using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine;
using UnityStandardAssets.Utility;
using TMPro;

public class Race : MonoBehaviour
{
    #region Public Variables   
    [SerializeField] public Transform[] route;
    [SerializeField] public List<PlayerController> participants;
    [SerializeField] public Dictionary<int, PlayerController> participantsCompletedRace;

    [SerializeField] public int numberOfLaps;
    [SerializeField] public int raceCapacity;

    [SerializeField] public bool raceInitiated = false;
    [SerializeField] public bool raceInProgress = false;
    [SerializeField] public bool raceComplete = false;

    [SerializeField] public float waitTimeForOtherPlayersToJoin = 5f;
    [SerializeField] public float timeRaceInitiated;
    [SerializeField] public float timeRaceStarted;
    [SerializeField] public TimeSpan raceDuration;
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
        participantsCompletedRace = new Dictionary<int, PlayerController>();

        route = FindObjectOfType<Race>().GetComponentsInChildren<Transform>();
    }

    public void AddParticipantIntoRace(PlayerController player)
    {
        if(participants.Count < raceCapacity)
        {
            participants.Add(player);

            player.GetComponent<WaypointProgressTracker>().amountOfLaps = numberOfLaps;
        }
    }

    public void AddParticipantToCompletedRaceList(PlayerController player)
    {
        participantsCompletedRace.Add(racePositionIndex, player);
        racePositionIndex++;

        CheckIfRaceComplete();
    }

    //If Menu is brought up in SINGLEPLAYER, pause race
    public void PauseSingleplayerRace()
    {
        gamePaused = true;
        durationOfRaceWithoutPauses = durationOfRaceWithoutPauses + (timeSecs - timeRaceStarted);  
        foreach(PlayerController player in participants)
        {
            player.GetComponent<WaypointProgressTracker>().moveTarget = false;
        }
    }

    //Unpause race in SINGLEPLAYER if game menu is closed 
    public void UnpauseSingleplayerRace()
    {
        gamePaused = false;
        timeRaceStarted = Time.timeSinceLevelLoad;
        foreach (PlayerController player in participants)
        {
            player.GetComponent<WaypointProgressTracker>().moveTarget = true;
        }
    }

    void Update()
    {
        if (gamePaused == false)
        {
            timeSecs = Time.timeSinceLevelLoad;

            if (raceInitiated == true)
            {
                if (raceComplete == true)
                {
                    EndRace();
                }
                else if (raceInProgress == false)
                {
                    if (participants.Count == raceCapacity || (timeRaceInitiated + timeSecs) > waitTimeForOtherPlayersToJoin)
                    {
                        StartCountdown();
                    }
                }
                else
                {
                    UpdateStopWatch();
                }
            }
        }
    }

    // Ref for basic countdown implementation: https://answers.unity.com/questions/369581/countdown-to-start-game.html
    private void StartCountdown()
    {
        foreach(PlayerController participant in participants)
        {
            // Pause player movement
            participant.PauseMovement();

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

    private void DisplayTextToParticipants(string text, int time=0)
    {
        foreach (PlayerController player in participants)
        {
            StartCoroutine(player.DisplayQuickNotificationText(text, time));
        }
    }

    private void DisplayRaceDataToParticipants(string time)
    {
        foreach (PlayerController player in participants)
        {
            Debug.Log($"Laps Completed:{player.GetComponent<WaypointProgressTracker>().currentLap}");
            player.DisplayTimeAndLap(time, $"Lap: {player.GetComponent<WaypointProgressTracker>().currentLap}/{numberOfLaps}");
        }
    }

    private void DisplayCountdownToParticipants(string count)
    {
        foreach (PlayerController player in participants)
        {
            StartCoroutine(player.DisplayCountdown(count, 3));
        }
    }

    private void StartRace()
    {
        //Allow participants to move [May need to figure out a better way to allow movement, where they'll all start at the EXACT same time]
        foreach (PlayerController participant in participants)
        {
            // Resume player movement
            participant.ResumeMovement();

            participant.GetComponent<WaypointProgressTracker>().moveTarget = true;

            // Retrieve notification container
            Transform notificationContainer = participant.transform.Find("HUD").Find("Notification Cont");

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

    private void CheckIfRaceComplete()
    {
        if(participantsCompletedRace.Count == raceCapacity)
        {
            raceComplete = true;
        }
    }

    private void EndRace()
    {
        DisplayEndOfRaceStats();
        DisposeSessionResources();
    }

    private void DisplayEndOfRaceStats()
    {
        foreach (KeyValuePair<int, PlayerController> player in participantsCompletedRace)
        {
            player.Value.timePanel.SetActive(false);
            player.Value.lapPanel.SetActive(false);
            StartCoroutine(player.Value.DisplayQuickNotificationText($"Your position within the race: {player.Key}", 6));
        }
    }

    private void UpdateStopWatch()
    {
        foreach(PlayerController participant in participants)
        {
            raceDuration = TimeSpan.FromSeconds((timeSecs + durationOfRaceWithoutPauses) - timeRaceStarted);

            DisplayRaceDataToParticipants($"{raceDuration.ToString(@"mm\:ss")}");
        }
    }

    // Reset all datatypes back to their initial state, after a race is finished
    private void DisposeSessionResources()
    {
        foreach(PlayerController participant in participants)
        {
            participant.JustRow();
        }

        ResetRaceStatsForParticipants();
        participants.Clear();
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
    }

    private void ResetRaceStatsForParticipants()
    {
        foreach(PlayerController participant in participants)
        {
            participant.GetComponent<PlayerController>().participatingInRace = false;
        }
    }
}
