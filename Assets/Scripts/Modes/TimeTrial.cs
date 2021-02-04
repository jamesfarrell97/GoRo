using System.Collections;
using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine;
using UnityStandardAssets.Utility;

public class TimeTrial : MonoBehaviour
{
    #region Public Variables   
    [SerializeField] public Transform[] route;
    [SerializeField] public Boat participant;

    [SerializeField] public int numberOfLaps;

    [SerializeField] public bool timeTrialInitiated = false;
    [SerializeField] public bool timeTrialInProgress = false;
    [SerializeField] public bool timeTrialComplete = false;

    [SerializeField] public float timeTheTimeTrialInitiated;
    [SerializeField] public float timeTheTimeTrialStarted;
    [SerializeField] public float pauseBeforeTimeTrialBegins = 5f;
    [SerializeField] public TimeSpan timeTrialDuration;
    #endregion Public Variables

    #region Private Variables
    private PhotonView photonView;
    private float timeSecs;

    private bool gamePaused = false;
    private float durationOfTimeTrialWithoutPauses;
    private float countdown = 4f;
    private float currentTimeInCountdown = 0;
    private int timeTrialPositionIndex = 1;
    #endregion Private Variables

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (gamePaused == false)
        {
            timeSecs = Time.timeSinceLevelLoad;

            if (timeTrialInitiated == true)
            {
                if (timeTrialComplete == true)
                {
                    EndTimeTrial();
                }
                else if (timeTrialInProgress == false)
                {
                    if ((timeTheTimeTrialInitiated + timeSecs) > pauseBeforeTimeTrialBegins)
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

    public void AddParticipantIntoTimeTrial(Boat player)
    {
        participant = player;
        player.GetComponent<WaypointProgressTracker>().amountOfLaps = numberOfLaps;
    }

    //REf for basic countdown implementation: https://answers.unity.com/questions/369581/countdown-to-start-game.html
    private void StartCountdown()
    {
        float delta = Time.deltaTime;
        currentTimeInCountdown += delta;

        if (currentTimeInCountdown >= 1)
        {
            if (countdown - 1 <= -1)
            {
                StartTimeTrial();
            }
            else if (countdown - 1 <= 0)
            {
                //GameObject.Find("UINotificationText").GetComponent<Text>().text = $"Start!";
                StartCoroutine(participant.GetComponent<PlayerController>().DisplayCountdown($"Start!", 3));
                countdown = 0;
            }
            else
            {
                countdown -= 1;
                //GameObject.Find("UINotificationText").GetComponent<Text>().text = $"{countdown}";
                StartCoroutine(participant.GetComponent<PlayerController>().DisplayCountdown($"{countdown}", 3));
                currentTimeInCountdown = 0;
            }
        }
    }

    private void StartTimeTrial()
    {
        timeTrialInProgress = true;
        timeTheTimeTrialStarted = Time.timeSinceLevelLoad;
        participant.GetComponent<WaypointProgressTracker>().moveTarget = true;
    }

    private void EndTimeTrial()
    {
        DisplayEndOfTimeTrialStats();
        DisposeSessionResources();
    }

    //If Menu is brought up in SINGLEPLAYER, pause time trial
    public void PauseSingleplayerTimeTrial()
    {
        gamePaused = true;
        durationOfTimeTrialWithoutPauses = durationOfTimeTrialWithoutPauses + (timeSecs - timeTheTimeTrialStarted);

        participant.GetComponent<WaypointProgressTracker>().moveTarget = false;
    }

    //Unpause time trial in SINGLEPLAYER if game menu is closed 
    public void UnpauseSingleplayerTimeTrial()
    {
        gamePaused = false;
        timeTheTimeTrialStarted = Time.timeSinceLevelLoad;

        participant.GetComponent<WaypointProgressTracker>().moveTarget = true;
    }

    private void UpdateStopWatch()
    {
        timeTrialDuration = TimeSpan.FromSeconds((timeSecs + durationOfTimeTrialWithoutPauses) - timeTheTimeTrialStarted);
        //GameObject.Find("UINotificationText").GetComponent<Text>().text = $"{timeTrialDuration.ToString(@"mm\:ss")}";
        string time = $"{timeTrialDuration.ToString(@"mm\:ss")}";
        participant.GetComponent<PlayerController>().DisplayTimeAndLap(time, $"Lap: {participant.GetComponent<WaypointProgressTracker>().currentLap}/{numberOfLaps}");
    }

    //Display window showing the participant their overall progress within the time trial, how long they took and 
    //Any achievements/ leaderboard score they obtained through that race + stat increase
    private void DisplayEndOfTimeTrialStats()
    {
        participant.GetComponent<PlayerController>().timePanel.SetActive(false);
        participant.GetComponent<PlayerController>().lapPanel.SetActive(false);
        //Show achievements for this time trial
        //GameObject.Find("UINotificationText").GetComponent<Text>().text = $"You completed {numberOfLaps} lap(s) within {timeTrialDuration.ToString(@"mm\:ss")}";
        string text = $"You completed {numberOfLaps} lap(s) within {timeTrialDuration.ToString(@"mm\:ss")}";
        StartCoroutine(participant.GetComponent<PlayerController>().DisplayQuickNotificationText(text, 6));
    }

    //Reset all datatypes back to their initial state, after a race is finished
    private void DisposeSessionResources()
    {
        participant.GetComponent<PlayerController>().participatingInTimeTrial = false;
        participant.GetComponent<PlayerController>().speedSlider.SetActive(false);
        participant.GetComponent<PlayerController>().speedSlider.GetComponent<Slider>().value = 0;
        participant = null;
        timeTrialInitiated = false;
        timeTrialInProgress = false;
        timeTrialComplete = false;
        timeTheTimeTrialStarted = 0;
        timeSecs = 0;
        numberOfLaps = 0;
        countdown = 3f;
        currentTimeInCountdown = 0;
        timeTheTimeTrialInitiated = 0;
        durationOfTimeTrialWithoutPauses = 0;
    }

}
