using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Race : MonoBehaviour
{

    public Rower participant;

    //Amount of players are able to participate in a race session
    public int maxAmountParticipants = 5;



    //Flag stating whether player(s) are in starting positions
    private bool playersReady = false;

    //Flag to state whether race is occurinng or is available to be started
    private bool raceInProgress = false;

    //Store race participants
    private Dictionary<Rower, DateTime> participants;
    //private List<GameObject> participants = new List<GameObject>();


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == participant)
        {
            DateTime timeEntered = System.DateTime.Now;
            participants.Add(participant, timeEntered);

            //Date stamp will exist to control the scenario where more than the max amount of rowers attempt to enter a race session
            //The first five will be given access
            //NEED TO: research into how viable this idea is, if rowers enter this trigger at the same time
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        participants = new Dictionary<Rower, DateTime>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Test");

        if (IsRaceInitiated() == true)
        {
            InitiateCountDown();
        }

    }

    private bool IsRaceInitiated()
    {
        if (participants.Count > 0)
        {
            return true;
        }

        return false;
    }

    //Initiate Race once player(s) in start position
    private void InitiateCountDown()
    {
        Debug.Log("Race Initiated");
    }


    //Reset all datatypes back to their initial state, after a race is finished
    private void DisposeSessionResources()
    {
        playersReady = false;
        raceInProgress = false;
        participants.Clear();

    }

}
