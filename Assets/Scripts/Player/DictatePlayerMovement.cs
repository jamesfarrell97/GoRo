using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

//Code referenced: https://www.youtube.com/watch?v=22PZJlpDkPE
//
//
//
public class DictatePlayerMovement : MonoBehaviour
{
    [SerializeField] public Transform[] route;
    [SerializeField] Boat boat;
    [SerializeField] public float speed;
    [SerializeField] public bool startMovement;
    [SerializeField] public float rotationSpeedInRadians = 0.015f;
    [SerializeField] public int waypointIndex = 0;
    [SerializeField] public int currentLap = 1;
    [SerializeField] public int amountOfLaps = 0;

    [SerializeField] public float timeOfCompletion;
    [SerializeField] public Race currentRace;
    [SerializeField] public TimeTrial currentTimeTrial;
    private float distance;
    private bool partcipatingInRace = false;
    private bool partcipatingInTimeTrial = false;
    private TimeSpan eventDuration;
    void Start()
    {
        waypointIndex = 0;
        startMovement = false;
        speed = 0;
    }

    void Update()
    {
        //if (startMovement == true)
        //{
        //    LookAtTargetWaypoint();
        //    distance = Vector3.Distance(transform.position, route[waypointIndex].position);

        //    if (distance < 3f)
        //    {
        //        IncreaseIndex();
        //    }
        //    Patrol();
        //}
    }

    private void LookAtTargetWaypoint()
    {
        Vector3 vector = route[waypointIndex].position - transform.position;
        vector.y = transform.position.y;
        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, vector, rotationSpeedInRadians * Time.deltaTime, 0.0f));
    }

    public void StartARace()
    {
        if (partcipatingInTimeTrial == false && partcipatingInRace == false)
        {
            timeOfCompletion = 0; //Resetting this
            partcipatingInRace = true;
            GameObject.Find("Race Manager").GetComponent<RaceManager>().AddPlayerToRace(boat);
        }
    }

    public void StartATimeTrial()
    {
        if (partcipatingInRace == false && partcipatingInTimeTrial == false)
        {
            timeOfCompletion = 0; //Resetting this
            partcipatingInTimeTrial = true;
            GameObject.Find("Time Trial Manager").GetComponent<TimeTrialManager>().AddPlayerToTimeTrial(boat);
        }
    }

    //Temporary way of processing speed of boat -> for demo purposes due to lack of access to concept 2 machine
    public void UpdateSpeedOfBoat(Slider slidier)
    {
        if (startMovement == true)
        {
            speed = slidier.value;
        }
    }

    public void ResetPlayerEventData()
    {
        route = null;
        speed = 0;
        startMovement = false;
        waypointIndex = 0;
        currentLap = 1;
        amountOfLaps = 0;

        timeOfCompletion = 0;
        currentRace = null;
        currentTimeTrial = null;
        distance = 0;
        partcipatingInRace = false;
        partcipatingInTimeTrial = false;
}

    private void Patrol()
    {
        if (startMovement == true)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    private void IncreaseIndex()
    {
        waypointIndex++;
        if (waypointIndex >= route.Length)
        {
            waypointIndex = 0;

            if (currentLap < amountOfLaps)
            {
                currentLap++;
                //Update Lap in UI for this player
            }
            else
            {
                Debug.Log("Within else statement of IncreaseIndex");
                speed = 0;
                startMovement = false;
                CompleteEvent();
            }
        }
        if (startMovement == true)
        {
            LookAtTargetWaypoint();
        }
    }

    private void CompleteEvent()
    {
        if(partcipatingInRace == true)
        {
            //Time, position and race info(amount of laps, track(distance), amount of participants) to be all stored relating to the new highscore obtained for this particular setup->Add with Player data
            eventDuration = TimeSpan.FromSeconds(Time.timeSinceLevelLoad - currentRace.timeRaceStarted);
            currentRace.AddParticipantToCompletedRaceList(boat);           
        }
        else if(partcipatingInTimeTrial == true)
        {
            currentTimeTrial.timeTrialComplete = true;
        }
    }

    


}
