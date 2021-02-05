using System.Collections;
using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine;
using UnityStandardAssets.Utility;
using TMPro;

public class TimeTrial : MonoBehaviour
{
    #region Public Variables   
    [SerializeField] public Transform[] route;
    [SerializeField] public PlayerController participant;

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

        route = FindObjectOfType<TimeTrial>().GetComponentsInChildren<Transform>();
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

    public void AddParticipantIntoTimeTrial(PlayerController player)
    {
        participant = player;
        player.GetComponent<WaypointProgressTracker>().amountOfLaps = numberOfLaps;
    }

    // Ref for basic countdown implementation: https://answers.unity.com/questions/369581/countdown-to-start-game.html
    private void StartCountdown()
    {
        float delta = Time.deltaTime;
        currentTimeInCountdown += delta;

        // Pause player movement
        participant.PauseMovement();

        // Retrieve notification container
        Transform notificationContainer = participant.transform.Find("HUD").Find("Notification Cont");

        // Activate component if not current active
        if (!notificationContainer.gameObject.activeSelf)
        {
            notificationContainer.gameObject.SetActive(true);
        }

        if (currentTimeInCountdown >= 1)
        {
            if (countdown - 1 <= -1)
            {
                StartTimeTrial();
            }
            else if (countdown - 1 <= 0)
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

    private void StartTimeTrial()
    {
        // Resume player movement
        participant.ResumeMovement();

        timeTrialInProgress = true;
        timeTheTimeTrialStarted = Time.timeSinceLevelLoad;
        participant.GetComponent<WaypointProgressTracker>().moveTarget = true;
    }

    private void UpdateStopWatch()
    {
        timeTrialDuration = TimeSpan.FromSeconds(timeSecs - timeTheTimeTrialStarted);

        // Retrieve notification container
        Transform notificationContainer = participant.transform.Find("HUD").Find("Notification Cont");

        // Update notification value
        notificationContainer.GetComponentInChildren<TMP_Text>().text = $"{timeTrialDuration.ToString(@"mm\:ss")}";
    }

    private void EndTimeTrial()
    {
        StartCoroutine(DisplayEndOfTimeTrialStats());
    }

    // Display window showing the participant their overall progress within the time trial, how long they took and 
    // Any achievements/ leaderboard score they obtained through that race + stat increase
    IEnumerator DisplayEndOfTimeTrialStats()
    {
        // Retrieve notification container
        Transform notificationContainer = participant.transform.Find("HUD").Find("Notification Cont");

        // Update notification value
        notificationContainer.GetComponentInChildren<TMP_Text>().text = $"Time: {timeTrialDuration.ToString(@"mm\:ss")}";

        //Wait for 4 seconds
        yield return new WaitForSeconds(3);

        // Update notification value
        notificationContainer.GetComponentInChildren<TMP_Text>().text = $"Leaving...";

        //Wait for 2 seconds
        yield return new WaitForSeconds(2);

        // Update notification value
        notificationContainer.GetComponentInChildren<TMP_Text>().text = $"";
        notificationContainer.gameObject.SetActive(false);

        DisposeSessionResources();
    }

    // Reset all datatypes back to their initial state, after a race is finished
    private void DisposeSessionResources()
    {
        participant.JustRow();
        participant.participatingInTimeTrial = false;
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