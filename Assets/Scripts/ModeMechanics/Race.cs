using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class Race : MonoBehaviour
{
    #region Public Variables
    //Amount of players allowed to participate per session
    [SerializeField] int maxAmountParticipants = 5;
    //Time permitted to get into initiated racing session before race begins
    [SerializeField] float timeToPrepare = 10f;
    //Time permitted for other racers to reach finish line (before ending the race session), once a first place for this race session is assigned
    [SerializeField] float timeToFinishRace = 20f;
    #endregion Public Variables

    #region Private Variables
    private List<GameObject> participants;
    private GameObject participant;
    private bool raceInitiated = false;
    private bool raceInProgress = false;
    private bool raceComplete = false;
    private float timeRaceInitiated;
    private float timeRaceStarted;
    private float timeSecs;
    #endregion Private Variables


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            participant = other.gameObject;

            if (participants.Count() == 0)
            {
                raceInitiated = true;
                timeRaceInitiated = Time.time;
            }

            if(participants.Count() < maxAmountParticipants)
            {
                participants.Add(participant); 
            }
        }
    }

    void Start()
    {
        participants = new List<GameObject>();
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
        bool finishLineReached = GameObject.Find("RaceFinishLine").GetComponent<RaceFinish>().finishLineReached;
        
        if (finishLineReached == true)
        {
            float timeFirstRowerPassed = GameObject.Find("RaceFinishLine").GetComponent<RaceFinish>().timeFirstRowerPassed;
            int amtParticipantsFinished = GameObject.Find("RaceFinishLine").GetComponent<RaceFinish>().amtParticipantsFinished;

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
        //Bring up barrier

        //Allow participants to move
        
        //Need to make sure that the floaters are up the whole way before race begins 
        //OR atleast have the invisible barrier block appear straight away to avoid others entering race area
        raceInProgress = true;
        timeRaceStarted = Time.timeSinceLevelLoad;
    }

    private void EndRace()
    {
        if(raceComplete == true)
        {
            //Bring buoys down
            //Remove invisible barrier block
            //Reset Race class to await for new session prompt
            //      Have another boxCollider covering entire race area,
            //      Only reset everything once players are no longer in race area
            //      To avoid re-activating the race trigger
            // OR have a cooldown period before the race can be activated again 
            //      (but still need to make sure no one is located within the race area)
            GameObject.Find("RaceFinishLine").GetComponent<RaceFinish>().resetFinishLine = true;
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

}
