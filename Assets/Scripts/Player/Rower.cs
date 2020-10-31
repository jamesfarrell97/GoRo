using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Photon.Pun;

public class Rower : MonoBehaviour
{
    public Boat boat;

    private bool gettingStarted = false;
    private bool seasick = false;

    // Stats
    private static readonly float KILOMETERS_METER = 50;  // TEST VALUES
    private static readonly float SECS_MINUTE = 5;         // TEST VALUES

    private Vector3 startingPos;
    private float distanceMeters;
    private float timeSecs;

    private PhotonView photonView;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
        }

        startingPos = transform.position;
        distanceMeters = 0;
        timeSecs = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        UpdateDistance();
        UpdateTime();

        if (!gettingStarted && distanceMeters / KILOMETERS_METER > 1)
        {
            Debug.Log("Achieved! Getting Started!");

            Transform achievementSlot = boat.GetAchievementSlot();
            GameObject achievement = PhotonNetwork.Instantiate(Path.Combine("Photon Prefabs", "Achievements", "Getting Started"), achievementSlot.position, achievementSlot.rotation);
            
            achievement.transform.SetParent(achievementSlot);

            gettingStarted = true;
        }

        if (!seasick && timeSecs / SECS_MINUTE > 1)
        {
            Debug.Log("Achieved! Seasick!");

            Transform achievementSlot = boat.GetAchievementSlot();
            GameObject achievement = PhotonNetwork.Instantiate(Path.Combine("Photon Prefabs", "Achievements", "Seasick!"), achievementSlot.position, achievementSlot.rotation);

            achievement.transform.SetParent(achievementSlot);

            seasick = true;
        }
    }

    private void UpdateDistance()
    {
        distanceMeters += Vector3.Magnitude(transform.position - startingPos);
        startingPos = transform.position;
    }

    private void UpdateTime()
    {
        timeSecs = Time.timeSinceLevelLoad;
    }

}
