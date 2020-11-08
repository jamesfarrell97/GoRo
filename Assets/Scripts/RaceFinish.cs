using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceFinish : MonoBehaviour
{
    public Rower participant;

    public bool finishLineReached = false;
    //Time when the first participant has reached the finish line
    private float timeFirstRowerPassed;


    private Achievement[] achievements;

    private Dictionary<Rower, float> participantsAtFinish;

    private float timeSecs;


    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(participantsAtFinish.Count == 0)
            {
                timeFirstRowerPassed = Time.timeSinceLevelLoad;
                finishLineReached = true;
            }



            participantsAtFinish.Add(participant, Time.timeSinceLevelLoad); // !!!Have proper way to define the particluar rower that has triggered the box collider
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        participantsAtFinish = new Dictionary<Rower, float>();
        finishLineReached = false;
    }

    // Update is called once per frame
    void Update()
    {
        timeSecs = Time.timeSinceLevelLoad;
    }

    void DetermineRaceRanks()
    {
        foreach(KeyValuePair<Rower, int> winner in participantsAtFinish)
        {
            //sort OR create simple class to store race passer which 
            //would store the Rower, their place and their time passed
        }
    }
}
