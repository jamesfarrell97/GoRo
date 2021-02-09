using System;

using UnityStandardAssets.Utility;
using UnityEngine;
using Photon.Pun;

public class TimeTrial : MonoBehaviour
{
    #region Public Variables   
    [HideInInspector] public Transform[] route;
    [HideInInspector] public PlayerController player;

    [HideInInspector] public bool timeTrialInitiated = false;
    [HideInInspector] public bool timeTrialInProgress = false;
    [HideInInspector] public bool timeTrialComplete = false;

    [HideInInspector] public float timeTheTimeTrialInitiated;
    [HideInInspector] public float timeTheTimeTrialStarted;
    [HideInInspector] public float pauseBeforeTimeTrialBegins = 5f;

    [SerializeField] public TimeSpan timeTrialDuration;
    [SerializeField] public int numberOfLaps;
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

        route = FindObjectOfType<TimeTrial>().GetComponentsInChildren<Transform>();
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

    // Code referenced: https://answers.unity.com/questions/369581/countdown-to-start-game.html
    private void StartCountdown()
    {
        // Pause player movement
        player.PauseMovement();

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
                StartCoroutine(GameManager.Instance.DisplayCountdown($"Start!", 3));
                countdown = 0;
            }
            else
            {
                countdown -= 1;

                StartCoroutine(GameManager.Instance.DisplayCountdown($"{countdown}", 3));
                currentTimeInCountdown = 0;
            }
        }
    }

    private void UpdateStopWatch()
    {
        timeTrialDuration = TimeSpan.FromSeconds((timeSecs + durationOfTimeTrialWithoutPauses) - timeTheTimeTrialStarted);

        DisplayTimeTrialDataToParticipants($"{timeTrialDuration.ToString(@"mm\:ss")}");
    }

    public void AddParticipantIntoTimeTrial(PlayerController player)
    {
        this.player = player;
        player.GetComponent<WaypointProgressTracker>().amountOfLaps = numberOfLaps;
    }

    private void StartTimeTrial()
    {
        player.ResumeMovement();

        timeTrialInProgress = true;
        timeTheTimeTrialStarted = Time.timeSinceLevelLoad;
    }

    // Pause singleplayer time trial if pause menu is opened
    public void PauseSingleplayerTimeTrial()
    {
        gamePaused = true;
        durationOfTimeTrialWithoutPauses = durationOfTimeTrialWithoutPauses + (timeSecs - timeTheTimeTrialStarted);

        player.PauseMovement();
    }

    // Resume singleplayer time trial if pause menu is closed
    public void ResumeSingleplayerTimeTrial()
    {
        gamePaused = false;
        timeTheTimeTrialStarted = Time.timeSinceLevelLoad;

        player.ResumeMovement();
    }

    private void DisplayTimeTrialDataToParticipants(string time)
    {
        int currentLap = player.GetComponent<WaypointProgressTracker>().currentLap;

        GameManager.Instance.DisplayTimeAndLap(time, $"Lap: {currentLap}/{numberOfLaps}");
    }

    private void DisplayEndOfTimeTrialStats()
    {
        string text = $"You completed {numberOfLaps} lap(s) within {timeTrialDuration.ToString(@"mm\:ss")}";
        StartCoroutine(GameManager.Instance.DisplayQuickNotificationText(text, 6));
    }

    private void EndTimeTrial()
    {
        DisplayEndOfTimeTrialStats();
        DisposeSessionResources();
    }

    // Reset all datatypes back to their initial state, after a race is finished
    private void DisposeSessionResources()
    {
        player.GetComponent<PlayerController>().participatingInTimeTrial = false;

        GameManager.Instance.StartJustRow();
        player.participatingInTimeTrial = false;

        player = null;
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