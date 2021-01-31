using System.Collections;
using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine;

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

    public void AddParticipantIntoTimeTrial(Boat player)
    {
        participant = player;
        participant.GetComponent<Boat>().GetComponent<DictatePlayerMovement>().route = route;
        participant.GetComponent<Boat>().GetComponent<DictatePlayerMovement>().amountOfLaps = numberOfLaps;
        participant.transform.position = route[0].position;
        //participant.transform.LookAt(route[1].position);
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

    private void StartTimeTrial()
    {
        participant.GetComponent<Boat>().GetComponent<DictatePlayerMovement>().startMovement = true;
        //participant.GetComponent<Boat>().GetComponent<DictatePlayerMovement>().speed = 2f;

        timeTrialInProgress = true;
        timeTheTimeTrialStarted = Time.timeSinceLevelLoad;
    }

    private void EndTimeTrial()
    {
        DisplayEndOfTimeTrialStats();
        DisposeSessionResources();
    }

    private void UpdateStopWatch()
    {
        timeTrialDuration = TimeSpan.FromSeconds(timeSecs - timeTheTimeTrialStarted);
        GameObject.Find("UINotificationText").GetComponent<Text>().text = $"{timeTrialDuration.ToString(@"mm\:ss")}";
    }

    //Display window showing the participant their overall progress within the time trial, how long they took and 
    //Any achievements/ leaderboard score they obtained through that race + stat increase
    private void DisplayEndOfTimeTrialStats()
    {
        //Show achievements for this time trial
        //GameObject.Find("UINotificationText").GetComponent<Text>().text = $"{countdown}";
        GameObject.Find("UINotificationText").GetComponent<Text>().text = $"You completed {numberOfLaps} lap(s) within {timeTrialDuration.ToString(@"mm\:ss")}";
    }

    //Reset all datatypes back to their initial state, after a race is finished
    private void DisposeSessionResources()
    {
        participant.GetComponent<DictatePlayerMovement>().ResetPlayerEventData();
        //participant.transform.position = new Vector3(-258, 0.55f, -1027);
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
    }

}
