using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityStandardAssets.Utility;

public class Race : MonoBehaviour
{
    #region Public Variables   
    [SerializeField] public Transform[] route;
    [SerializeField] public List<Boat> participants;
    [SerializeField] public Dictionary<int, Boat> participantsCompletedRace;

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
        participantsCompletedRace = new Dictionary<int, Boat>();
    }

    public void AddParticipantIntoRace(Boat player)
    {
        if(participants.Count < raceCapacity)
        {
            participants.Add(player);
            //player.GetComponent<WaypointProgressTracker>().Circuit = FindObjectOfType<WaypointCircuit>();
            //GetComponent<WaypointProgressTracker>().Circuit = FindObjectOfType<WaypointCircuit>();
            player.GetComponent<WaypointProgressTracker>().amountOfLaps = numberOfLaps;
            
        }
    }

    public void AddParticipantToCompletedRaceList(Boat player)
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
        foreach(Boat player in participants)
        {
            player.GetComponent<WaypointProgressTracker>().moveTarget = false;
        }
    }

    //Unpause race in SINGLEPLAYER if game menu is closed 
    public void UnpauseSingleplayerRace()
    {
        gamePaused = false;
        timeRaceStarted = Time.timeSinceLevelLoad;
        foreach (Boat player in participants)
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

    //REf for basic countdown implementation: https://answers.unity.com/questions/369581/countdown-to-start-game.html
    private void StartCountdown()
    {
        float delta = Time.deltaTime;
        currentTimeInCountdown += delta;

        if(currentTimeInCountdown >= 1)
        {
            if (countdown - 1 <= -1)
            {
                StartRace();
            }
            else if(countdown - 1 <= 0)
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

    private void DisplayTextToParticipants(string text, int time=0)
    {
        foreach (Boat player in participants)
        {
            StartCoroutine(player.GetComponent<PlayerController>().DisplayQuickNotificationText(text, time));
        }
    }

    private void DisplayRaceDataToParticipants(string time)
    {
        foreach (Boat player in participants)
        {
            Debug.Log($"Laps Completed:{player.GetComponent<WaypointProgressTracker>().currentLap}");
            player.GetComponent<PlayerController>().DisplayTimeAndLap(time, $"Lap: {player.GetComponent<WaypointProgressTracker>().currentLap}/{numberOfLaps}");
        }
    }

    private void DisplayCountdownToParticipants(string count)
    {
        foreach (Boat player in participants)
        {
            StartCoroutine(player.GetComponent<PlayerController>().DisplayCountdown(count, 3));
        }
    }

    private void StartRace()
    {
        //Allow participants to move [May need to figure out a better way to allow movement, where they'll all start at the EXACT same time]
        foreach (Boat participant in participants)
        {
            //if (!photonView.IsMine)
            //{
            //    //Make participant transparent
            //}            

            participant.GetComponent<WaypointProgressTracker>().moveTarget = true;
        }

        raceInProgress = true;
        timeRaceStarted = Time.timeSinceLevelLoad;
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

    private void UpdateStopWatch()
    {
        raceDuration = TimeSpan.FromSeconds((timeSecs + durationOfRaceWithoutPauses) - timeRaceStarted);
        //GameObject.Find("UINotificationText").GetComponent<Text>().text = $"{raceDuration.ToString(@"mm\:ss")}";
        //DisplayTextToParticipants(false, $"{raceDuration.ToString(@"mm\:ss")}");
        DisplayRaceDataToParticipants($"{raceDuration.ToString(@"mm\:ss")}");
    }

    //Display window showing participants their place in the race, how long they took and 
    //Any achievements/ leaderboard score they obtained through that race + stat increase
    private void DisplayEndOfRaceStats()
    {
        //Show leaderboard (of partcipants) for this race
        //GameObject.Find("UINotificationText").GetComponent<Text>().text = $"{countdown}";
        foreach(KeyValuePair<int, Boat> player in participantsCompletedRace)
        {
            //Modify to display for player only(photon.mine)
            //GameObject.Find("UINotificationText").GetComponent<Text>().text = $"Your position within the race: {player.Key}";
            player.Value.GetComponent<PlayerController>().timePanel.SetActive(false);
            player.Value.GetComponent<PlayerController>().lapPanel.SetActive(false);
            StartCoroutine(player.Value.GetComponent<PlayerController>().DisplayQuickNotificationText($"Your position within the race: {player.Key}", 6));
        }

    }

    //Reset all datatypes back to their initial state, after a race is finished
    private void DisposeSessionResources()
    {
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
        foreach(Boat player in participants)
        {
            player.GetComponent<PlayerController>().participatingInRace = false;
            player.GetComponent<PlayerController>().speedSlider.SetActive(false);
            player.GetComponent<PlayerController>().speedSlider.GetComponent<Slider>().value = 0;
        }
    }
}
