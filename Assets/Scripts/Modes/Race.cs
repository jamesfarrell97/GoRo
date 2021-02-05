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
    private PhotonView photonView;
    private float timeSecs;

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

    void Update()
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

    // Ref for basic countdown implementation: https://answers.unity.com/questions/369581/countdown-to-start-game.html
    private void StartCountdown()
    {
        foreach(PlayerController participant in participants)
        {
            // Pause player movement
            participant.PauseMovement();

            float delta = Time.deltaTime;
            currentTimeInCountdown += delta;
            
            // Retrieve notification container
            Transform notificationContainer = participant.transform.Find("HUD").Find("Notification Cont");

            // Activate component if not current active
            if (!notificationContainer.gameObject.activeSelf)
            {
                notificationContainer.gameObject.SetActive(true);
            }

            if(currentTimeInCountdown >= 1)
            {
                if (countdown - 1 <= -1)
                {
                    StartRace();
                }
                else if(countdown - 1 <= 0)
                {
                    notificationContainer.GetComponentInChildren<TMP_Text>().text = "Start!";
                    countdown = 0;
                }           
                else
                {
                    countdown -= 1;

                    notificationContainer.GetComponentInChildren<TMP_Text>().text = $"{countdown}";
                    currentTimeInCountdown = 0;
                }
            }
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
        StartCoroutine(DisplayRaceStats());
    }

    // Display window showing the participant their overall progress within the time trial, how long they took and 
    // Any achievements/ leaderboard score they obtained through that race + stat increase
    IEnumerator DisplayRaceStats()
    {
        // Show leaderboard (of partcipants) for this race
        // GameObject.Find("UINotificationText").GetComponent<Text>().text = $"{countdown}";
        foreach (KeyValuePair<int, PlayerController> participant in participantsCompletedRace)
        {
            // Retrieve notification container
            Transform notificationContainer = participant.Value.transform.Find("HUD").Find("Notification Cont");

            // Update notification value
            notificationContainer.GetComponentInChildren<TMP_Text>().text = $"Position: {participant.Key}";
        }

        //Wait for 4 seconds
        yield return new WaitForSeconds(3);

        // Show leaderboard (of partcipants) for this race
        // GameObject.Find("UINotificationText").GetComponent<Text>().text = $"{countdown}";
        foreach (KeyValuePair<int, PlayerController> participant in participantsCompletedRace)
        {
            // Retrieve notification container
            Transform notificationContainer = participant.Value.transform.Find("HUD").Find("Notification Cont");

            // Update notification value
            notificationContainer.GetComponentInChildren<TMP_Text>().text = $"Leaving...";
        }

        //Wait for 2 seconds
        yield return new WaitForSeconds(2);

        // Show leaderboard (of partcipants) for this race
        // GameObject.Find("UINotificationText").GetComponent<Text>().text = $"{countdown}";
        foreach (KeyValuePair<int, PlayerController> participant in participantsCompletedRace)
        {
            // Retrieve notification container
            Transform notificationContainer = participant.Value.transform.Find("HUD").Find("Notification Cont");

            // Update notification value
            notificationContainer.gameObject.SetActive(false);
            notificationContainer.GetComponentInChildren<TMP_Text>().text = $"";
        }

        DisposeSessionResources();
    }

    private void UpdateStopWatch()
    {
        foreach(PlayerController participant in participants)
        {
            raceDuration = TimeSpan.FromSeconds(timeSecs - timeRaceStarted);

            // Retrieve notification container
            Transform notificationContainer = participant.transform.Find("HUD").Find("Notification Cont");

            // Update notification value
            notificationContainer.GetComponentInChildren<TMP_Text>().text = $"{raceDuration.ToString(@"mm\:ss")}";
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
    }

    private void ResetRaceStatsForParticipants()
    {
        foreach(PlayerController participant in participants)
        {
            participant.participatingInRace = false;
        }
    }
}
