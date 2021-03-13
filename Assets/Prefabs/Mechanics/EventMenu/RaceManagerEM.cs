using Photon.Pun;
using UnityEngine;
using UnityStandardAssets.Utility;

public class RaceManager : MonoBehaviour
{
    PhotonView photonView;
    public static RaceManager Instance;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();

        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    //Create a race and add the player who created it, into the race
    public void CreateRace(PlayerController player, Route chosenRoute, float secondsForWait, int numberOfLaps, int raceCapacity)
    {
        chosenRoute.GetComponent<Race>().InitiateRace(secondsForWait, numberOfLaps, raceCapacity);
        AddPlayerToRace(player, chosenRoute);
    }

    //Add a player into an existing race which has not began yet
    public void AddPlayerToRace(PlayerController player, Route chosenRoute)
    {
        Race race = chosenRoute.GetComponent<Race>();

        // Retrieve waypoint progress tracker
        WaypointProgressTracker wpt = player.GetComponent<WaypointProgressTracker>();

        // Setup wpt values
        wpt.SetCircuit(race.gameObject.GetComponent<WaypointCircuit>());
        wpt.UpdateLastNodeIndex(race.track.Length - 1);
        wpt.SetRace(race);
        wpt.routeType = chosenRoute.routeType;

        chosenRoute.GetComponent<Race>().AddParticipantIntoRace(player);
    }

    //public void AddPlayerToRace(PlayerController player)
    //{
    //    // Retrieve race
    //    Race race = FindObjectOfType<Race>();

    //    // Retrieve waypoint progress tracker
    //    WaypointProgressTracker wpt = player.GetComponent<WaypointProgressTracker>();

    //    // Setup wpt values
    //    wpt.SetCircuit(race.gameObject.GetComponent<WaypointCircuit>());
    //    wpt.UpdateLastNodeIndex(race.track.Length - 1);
    //    wpt.SetRace(race);

    //    PhotonView playerView = player.GetComponent<PhotonView>();
    //    photonView.RPC("RPC_AddPlayerToRace", RpcTarget.All, playerView.ViewID);
    //}

    //[PunRPC]
    //void RPC_AddPlayerToRace(int playerID)
    //{
    //    // Retrieve race
    //    Race race = FindObjectOfType<Race>();

    //    // Retrieve player view
    //    PhotonView playerView = PhotonView.Find(playerID);

    //    // Retrieve player controller
    //    PlayerController player = playerView.gameObject.GetComponent<PlayerController>();

    //    // Setup race values
    //    race.InitiateRace(1, 2);
    //    race.AddParticipantIntoRace(player);
    //}
}
