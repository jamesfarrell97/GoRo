using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Race : MonoBehaviour
{

    public Rower participant;
    private RaceFinishLine finshLine;
    private RaceFinish finishLineDetails;

    //Amount of players allowed to participate per session
    public int maxAmountParticipants = 5;

    //Time permitted to get into initiated racing session before race begins
    public float timeToPrepare = 10f;

    //Time permitted for other racers to reach finish line (before ending the race session), once a first place for this race session is assigned
    public float timeToFinishRace = 20f; 


    private bool raceInitiated = false;

    private bool raceInProgress = false;

    private bool raceComplete = false;

    private List<Rower> participants;


    private float timeRaceInitiated;
    private float timeRaceStarted;
    private float timeSecs;

    void OnTriggerEnter(Collider other)
    {
        

        if (other.CompareTag("Player"))
        {

            if (participants.Count() == 0)
            {
                raceInitiated = true;
                timeRaceInitiated = Time.time;
            }

            if(participants.Count() < maxAmountParticipants)
            {
                participants.Add(participant); // !!!Have proper way to define the particluar rower that has triggered the box collider
            }

        }
    }

    void Awake()
    {
        finishLine = GameObject.FindWithTag("FinishRace");
    }

    void Start()
    {
        participants = new List<Rower>();
        //finishLineDetails = finishLine.GetComponent<RaceFinish>();
    }

    // Update is called once per frame
    void Update()
    {
        timeSecs = Time.timeSinceLevelLoad;

        Debug.Log($"In Game Time: {timeSecs}");
        Debug.Log($"Time Race Initiated {timeRaceInitiated}");
        Debug.Log($"Time Race Started {timeRaceStarted}");

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
        if(finshLine.finishLineReached == true)
        {          
            if((timeSecs - finishLine.timeFirstRowerPassed) >= timeToFinishRace)
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

        Debug.Log($"In Game Time: {timeSecs}");
        Debug.Log($"Time Race Initiated {timeRaceInitiated}");
        Debug.Log($"Time Race Started {timeRaceStarted}");
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



        }
    }

    //Reset all datatypes back to their initial state, after a race is finished
    private void DisposeSessionResources()
    {
        raceInProgress = false;
        raceInitiated = false;
        participants.Clear();
        timeRaceInitiated = 0;
        timeRaceStarted = 0;
        timeSecs = 0;
    }

}
