using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class RaceFinish : MonoBehaviour
{
    #region Public Variables
    [HideInInspector] public int amtParticipantsFinished = 0;
    [HideInInspector] public bool finishLineReached = false;
    [HideInInspector] public float timeFirstRowerPassed;
    [HideInInspector] public bool resetFinishLine;
    #endregion Public Variables

    #region Private Variables
    private Dictionary<GameObject, float> participantsAtFinish;
    private GameObject participant;
    private int rankCounter = 1;
    private float timeSecs;
    #endregion Private Variables


    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            participant = other.gameObject;

            if (participantsAtFinish.Count == 0)
            {
                timeFirstRowerPassed = Time.timeSinceLevelLoad;
                finishLineReached = true;              
            }


            participantsAtFinish.Add(participant, Time.timeSinceLevelLoad); // !!!Have proper way to define the particluar rower that has triggered the box collider
            amtParticipantsFinished = participantsAtFinish.Count;
            DeclareWinner(participant, rankCounter);

            rankCounter++;
        }
    }

    void Start()
    {
        participantsAtFinish = new Dictionary<GameObject, float>();
    }

    void Update()
    {
        timeSecs = Time.timeSinceLevelLoad;

        if(resetFinishLine == true)
        {
            DisposeSessionResources();
        }
    }

    private void DeclareWinner(GameObject participant, int rank)
    {
        //Print winner out into UI
        //Send user their achievement to store in stats
    }

    private void DisposeSessionResources()
    {
        participantsAtFinish.Clear();
        finishLineReached = false;
        resetFinishLine = false;
        timeFirstRowerPassed = 0;
        amtParticipantsFinished = 0;   
        timeSecs = 0;
    }
}
