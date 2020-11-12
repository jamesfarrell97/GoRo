using System;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class Race : MonoBehaviour
{
    #region Public Variables
    //Amount of players allowed to participate per session
    [SerializeField] int maxAmountParticipants = 5;
    //Time permitted to get into initiated racing session before race begins
    [SerializeField] float timeToPrepare = 2f;
    //Time permitted for other racers to reach finish line (before ending the race session), once a first place for this race session is assigned
    [SerializeField] float timeToFinishRace = 3f;
    [SerializeField] TMP_Text startText;
    [SerializeField] TMP_Text endText;
    [SerializeField] Canvas canvas;
    #endregion Public Variables

    #region Private Variables
    private List<PlayerController> participants;
    private PlayerController participant;

    private bool raceInitiated = false;
    private bool raceInProgress = false;
    private bool raceComplete = false;

    private float timeRaceInitiated;
    private float timeRaceStarted;
    private float timeSecs;
    #endregion Private Variables

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boat Tip"))
        {
            participant = other.GetComponentInParent<PlayerController>();

            if (participants.Count() == 0 && !raceInitiated)
            {
                raceInitiated = true;
                timeRaceInitiated = Time.time;
                StartCoroutine(sendNotification("Get Ready!", 2));
            }

            if(participants.Count() < maxAmountParticipants && !participants.Contains(participant))
            {
                participants.Add(participant); 
                participant.PauseMovement();
            }
        }
    }

    void Awake()
    {
        canvas.worldCamera = FindObjectOfType<Camera>();
    }

    void Start()
    {
        participants = new List<PlayerController>();
    }

    void Update()
    {
        timeSecs = Time.timeSinceLevelLoad;

        if (raceInitiated == true)
        {
            if (raceInProgress == false)
            {
                if (participants.Count() == maxAmountParticipants || (timeSecs - timeRaceInitiated) >= timeToPrepare)
                {                   
                    StartRace();                 
                }
            }
            else
            {
                HasFinishLineBeenCrossed();
                EndRace();
            }
        }
    }

    private void HasFinishLineBeenCrossed()
    {
        bool finishLineReached = GameObject.Find("Race Finish Line").GetComponent<RaceFinish>().finishLineReached;
        
        if (finishLineReached == true)
        {
            float timeFirstRowerPassed = GameObject.Find("Race Finish Line").GetComponent<RaceFinish>().timeFirstRowerPassed;
            int amtParticipantsFinished = GameObject.Find("Race Finish Line").GetComponent<RaceFinish>().participantsAtFinishCount;

            //Finish this race session either if all participants have reached the finish line, 
            //Or once extra time has run out after  first participant passed the finish line.
            if (amtParticipantsFinished == participants.Count || (timeSecs - timeFirstRowerPassed) >= timeToFinishRace)
            {
                raceComplete = true;
            }
        }
    }

    private void StartRace()
    {
        StartCoroutine(sendNotification("Start!", 2));

        //Allow participants to move
        foreach (var participant in participants)
        {
            participant.ContinueMovement();
        }

        //Need to make sure that the floaters are up the whole way before race begins 
        //OR atleast have the invisible barrier block appear straight away to avoid others entering race area
        raceInProgress = true;
        timeRaceStarted = Time.timeSinceLevelLoad;
    }

    private void EndRace()
    {
        if(raceComplete == true)
        {
            StartCoroutine(sendNotification("Race Complete", 2));
            
            //Bring buoys down
            //Remove invisible barrier block
            //Reset Race class to await for new session prompt
            //      Have another boxCollider covering entire race area,
            //      Only reset everything once players are no longer in race area
            //      To avoid re-activating the race trigger
            // OR have a cooldown period before the race can be activated again 
            //      (but still need to make sure no one is located within the race area)
            GameObject.Find("Race Finish Line").GetComponent<RaceFinish>().resetFinishLine = true;
            DisposeSessionResources();
        }
    }

    //Reset all datatypes back to their initial state, after a race is finished
    private void DisposeSessionResources()
    {
        participants.Clear();
        raceInitiated = false;
        raceInProgress = false;
        raceComplete = false;
        timeRaceInitiated = 0;
        timeRaceStarted = 0;
        timeSecs = 0;
    }

    //UI Output Code Reference: https://www.youtube.com/watch?v=9MsPWhqQRxo
    IEnumerator sendNotification(string text, int time)
    {
        startText.text = text;
        endText.text = text;
        yield return new WaitForSeconds(time);
        startText.text = String.Empty;
        endText.text = String.Empty;
    }

}
