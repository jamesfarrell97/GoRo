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
    private static readonly float KILOMETERS_METER = 50;   // TEST VALUES
    private static readonly float SECS_MINUTE = 1;         // TEST VALUES

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
            
            GameObject achievement = PhotonNetwork.Instantiate(Path.Combine("Photon Prefabs", "Achievements", "Getting Started"), transform.position, transform.rotation);

            photonView.RPC("GitParent", RpcTarget.AllBuffered, new object[] { photonView.ViewID, achievement.GetPhotonView().ViewID });

            gettingStarted = true;
        }

        if (!seasick && timeSecs / SECS_MINUTE > 1)
        {
            Debug.Log("Achieved! Seasick!");
            
            GameObject achievement = PhotonNetwork.Instantiate(Path.Combine("Photon Prefabs", "Achievements", "Seasick!"), transform.position, transform.rotation);

            photonView.RPC("GitParent", RpcTarget.AllBuffered, new object[] { photonView.ViewID, achievement.GetPhotonView().ViewID});

            seasick = true;
        }
    }

    [PunRPC]
    void GitParent(int playerID, int achievermentID)
    {
        PhotonView playerView = PhotonView.Find(playerID); 
        PhotonView achievementView = PhotonView.Find(achievermentID);

        Boat boat = playerView.gameObject.GetComponentInChildren<Boat>();
        Transform achievementSlot = boat.GetAchievementSlot();

        achievementView.gameObject.transform.SetParent(achievementSlot);
        achievementView.gameObject.transform.SetPositionAndRotation(achievementSlot.position, achievementSlot.rotation);
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
