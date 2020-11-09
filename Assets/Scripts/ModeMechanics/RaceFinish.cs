using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class RaceFinish : MonoBehaviour
{
    #region Public Variables
    [HideInInspector] public int participantsAtFinishCount = 0;
    [HideInInspector] public bool finishLineReached = false;
    [HideInInspector] public float timeFirstRowerPassed;
    [HideInInspector] public bool resetFinishLine;
    [SerializeField] TMP_Text startText;
    [SerializeField] TMP_Text endText;
    #endregion Public Variables

    #region Private Variables
    private Dictionary<PlayerController, float> participantsAtFinish;
    private PlayerController participant;
    private float timeSecs;
    #endregion Private Variables

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Boat Tip"))
        {
            participant = other.GetComponentInParent<PlayerController>();

            if (participantsAtFinish.Count == 0)
            {
                timeFirstRowerPassed = Time.timeSinceLevelLoad;
                finishLineReached = true;
            }

            if (!participantsAtFinish.ContainsKey(participant))
            {
                participantsAtFinish.Add(participant, Time.timeSinceLevelLoad); // !!!Have proper way to define the particluar rower that has triggered the box collider
                participantsAtFinishCount = participantsAtFinish.Count;
            }

            DeclareWinner(participant);
        }
    }

    void Start()
    {
        participantsAtFinish = new Dictionary<PlayerController, float>();
    }

    void Update()
    {
        timeSecs = Time.timeSinceLevelLoad;

        if(resetFinishLine == true)
        {
            DisposeSessionResources();
        }
    }

    private void DeclareWinner(PlayerController participant)
    {
        string playerName = participant.GetComponent<PhotonView>().name;
        StartCoroutine(sendNotification($"Race won by: {playerName}", 5));
        //Send user their achievement to store in stats
    }

    private void DisposeSessionResources()
    {
        participantsAtFinish.Clear();
        finishLineReached = false;
        resetFinishLine = false;
        timeFirstRowerPassed = 0;
        participantsAtFinishCount = 0;   
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
