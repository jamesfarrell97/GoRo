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
            //player.transform.position = route[0].position;
            //player.transform.LookAt(route[1].position);

            //Vector3 vector = route[1].position - player.transform.position;
            //player.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, vector, 0f * Time.deltaTime, 0.0f));
        }
    }

    public void AddParticipantToCompletedRaceList(Boat player)
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
                GameObject.Find("UINotificationText").GetComponent<Text>().text = $"Start!";
                countdown = 0;
            }           
            else
            {
                countdown -= 1;
                GameObject.Find("UINotificationText").GetComponent<Text>().text = $"{countdown}";
                currentTimeInCountdown = 0;
            }
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
        //GameObject.Find("UINotificationText").GetComponent<Text>().text = $"{(int)(timeSecs-timeRaceStarted)}";

        raceDuration = TimeSpan.FromSeconds(timeSecs - timeRaceStarted);
        GameObject.Find("UINotificationText").GetComponent<Text>().text = $"{raceDuration.ToString(@"mm\:ss")}";
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
            GameObject.Find("UINotificationText").GetComponent<Text>().text = $"Your position within the race: {player.Key}";
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
    }

    private void ResetRaceStatsForParticipants()
    {
        foreach(Boat player in participants)
        {
            player.GetComponent<PlayerController>().participatingInRace = false;
            //player.GetComponent<DictatePlayerMovement>().ResetPlayerEventData();
            //Transform player back to start point, could later make it(random), 
            //to perhaps overlook oher races whilst in neutral mood
            //player.transform.position = new Vector3(-258, 0.55f, -1027); 
        }
    }
}
