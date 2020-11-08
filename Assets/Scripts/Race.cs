using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Race : MonoBehaviour
{

    public Rower participant;

    //Amount of players allowed to participate per session
    public int maxAmountParticipants = 5;

    //Time permitted to get into initiated racing session before race begins
    public float timeToPrepare = 10f;

    //Time permitted for other racers to reach finish line (before ending the race session), once a first place for this race session is assigned
    public float timeToFinishRace = 20f; 


    private bool raceInitiated = false;

    private bool raceInProgress = false;

    private List<Rower> participants;


    private float timeRaceInitiated;
    private float timeRaceStarted;
    private float timeSecs;

    void OnTriggerEnter(Collider other)
    {
        

        //if (other.gameObject == participant)
        if (other.CompareTag("Player"))
        {

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

    // Start is called before the first frame update
    void Start()
    {
        participants = new List<Rower>();
    }

    // Update is called once per frame
    void Update()
    {
        timeSecs = Time.time;

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
        timeRaceStarted = Time.time;

        Debug.Log($"In Game Time: {timeSecs}");
        Debug.Log($"Time Race Initiated {timeRaceInitiated}");
        Debug.Log($"Time Race Started {timeRaceStarted}");
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
