using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Photon.Pun;

public class AchievementTracker : MonoBehaviour
{
    #region Distance Based Achievements
    [Header("Distance Based Achievements")]

    #region Achievement Names
    [SerializeField] GameObject distance1;
    [SerializeField] GameObject distance2;
    [SerializeField] GameObject distance3;
    #endregion

    #region Targets
    [SerializeField] [Tooltip("In meters")] int distance1Target = 100;
    [SerializeField] [Tooltip("In meters")] int distance2Target = 500;
    [SerializeField] [Tooltip("In meters")] int distance3Target = 1000;
    #endregion

    #region Flags
    private bool distance1_Flag = false;
    private bool distance2_Flag = false;
    private bool distance3_Flag = false;
    #endregion

    #region Tracked Values
    private float metersRowed = 0;
    #endregion

    #region Tracker Methods
    private void TrackDistance1(PhotonView photonView)
    {
        if (distance1_Flag) return;

        if (metersRowed > distance1Target)
        {
            ActivateAchievement(photonView, distance1.name);
            distance1_Flag = true;
        }
    }

    private void TrackDistance2(PhotonView photonView)
    {
        if (distance2_Flag) return;

        if (metersRowed > distance2Target)
        {
            ActivateAchievement(photonView, distance2.name);
            distance2_Flag = true;
        }
    }

    private void TrackDistance3(PhotonView photonView)
    {
        if (distance3_Flag) return;

        if (metersRowed > distance3Target)
        {
            ActivateAchievement(photonView, distance3.name);
            distance3_Flag = true;
        }
    }
    #endregion

    #endregion

    #region Time Based Achievements
    [Header("Time Based Achievements")]

    #region Achievement Names
    [SerializeField] GameObject time1;
    [SerializeField] GameObject time2;
    [SerializeField] GameObject time3;
    #endregion

    #region Targets
    [SerializeField] [Tooltip("In minutes")] [Range(0, 60)] int time1_Target = 1;
    [SerializeField] [Tooltip("In minutes")] [Range(0, 60)] int time2_Target = 5;
    [SerializeField] [Tooltip("In minutes")] [Range(0, 60)] int time3_Target = 10;
    #endregion

    #region Flags
    private bool time1_Flag = false;
    private bool time2_Flag = false;
    private bool time3_Flag = false;
    #endregion

    #region Tracked Values
    private float secondsRowing = 0;
    #endregion

    #region Tracker Methods
    private void TrackTime1(PhotonView photonView)
    {
        if (time1_Flag) return;

        if (secondsRowing / Stats.SECS_MINUTE > time1_Target)
        {
            ActivateAchievement(photonView, time1.name);
            time1_Flag = true;
        }
    }

    private void TrackTime2(PhotonView photonView)
    {
        if (time2_Flag) return;

        if (secondsRowing / Stats.SECS_MINUTE > time2_Target)
        {
            ActivateAchievement(photonView, time2.name);
            time2_Flag = true;
        }
    }

    private void TrackTime3(PhotonView photonView)
    {
        if (time3_Flag) return;

        if (secondsRowing / Stats.SECS_MINUTE > time3_Target)
        {
            ActivateAchievement(photonView, time3.name);
            time3_Flag = true;
        }
    }
    #endregion
    
    #endregion

    public void TrackAchievements(PhotonView photonView, Stats stats)
    {
        metersRowed = stats.GetMetersRowed();
        secondsRowing = stats.GetSecondsRowing();

        TrackDistance1(photonView);
        TrackDistance2(photonView);
        TrackDistance3(photonView);

        TrackTime1(photonView);
        TrackTime2(photonView);
        TrackTime3(photonView);
    }

    private void ActivateAchievement(PhotonView photonView, string achievementName)
    {
        GameObject achievement = 
            PhotonNetwork.Instantiate(
                Path.Combine("Photon Prefabs", "Achievements", achievementName), 
                transform.position, 
                transform.rotation
            );

        photonView.RPC(
            "PlaceAchievement", 
            RpcTarget.AllBuffered, 
            new object[] {
                photonView.ViewID,
                achievement.GetPhotonView().ViewID
            }
        );
    }

    [PunRPC]
    private void PlaceAchievement(int playerID, int achievermentID)
    {
        PhotonView PlayerView = PhotonView.Find(playerID);
        PhotonView AchievementView = PhotonView.Find(achievermentID);

        Boat PlayerBoat = PlayerView.gameObject.GetComponentInChildren<Boat>();

        // Is there a slot available?
        if (!PlayerBoat.IsSlotAvailable()) return;

        Transform achievementSlot = PlayerBoat.GetAchievementSlot();

        AchievementView.gameObject.transform.SetPositionAndRotation(achievementSlot.position, achievementSlot.rotation);
        AchievementView.gameObject.transform.SetParent(achievementSlot);
    }
}
