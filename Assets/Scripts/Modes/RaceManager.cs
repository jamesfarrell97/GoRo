using Photon.Pun;
using UnityEngine;
using UnityStandardAssets.Utility;

public class RaceManager : MonoBehaviour
{
    PhotonView photonView;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void TakePartInARace(PlayerController player)
    {
        //Will initiate Menu Screen to appear where player will be prompted to select race track they wish to take part in
        //From this interaction the following methods starting with "AddPlayerToXXX" represent the button pressed actions relevant to race selected
    }

    #region Race Initiation Button Responses
    public void AddPlayerToHeroBeachRace()
    {
        // Changing this just to get it working for the release
        // Will change back to previous implementation later
        Race heroBeachRace = FindObjectOfType<Race>();
        if(heroBeachRace.raceInitiated == false)
        {
            //BringUpRaceSetupMenu
            //SetAllRaceParametersBased on the feedback given by player
            heroBeachRace.raceInitiated = true;
            //heroBeachRace.numberOfLaps = ?;
            //heroBeachRace.raceCapacity = ?;
            //Add player to participants list
        }
        else
        {
            if(heroBeachRace.players.Count < heroBeachRace.raceCapacity)
            {
                //Add player to the partcipants list
            }
        }
    }
    #endregion Race Initiation Button Responses

    //HardCoded Method to reach simple solution for Base Version of the 
    //waypoint/ race/ time trial mechanics until the menu and more race routes are implemented 
    //IMPORTANT: Everything being set in this method will need to be set for every player joining game, it is essential 
    public void AddPlayerToRace(PlayerController player)
    {
        // Retrieve race
        Race race = FindObjectOfType<Race>();

        // Retrieve waypoint progress tracker
        RouteFollower wpt = player.GetComponent<RouteFollower>();

        // Setup wpt values
        wpt.SetCircuit(race.gameObject.GetComponent<Route>());
        wpt.UpdateLastNodeIndex(race.route.Length - 1);
        wpt.SetRace(race);

        PhotonView playerView = player.GetComponent<PhotonView>();
        photonView.RPC("RPC_AddPlayerToRace", RpcTarget.All, playerView.ViewID);
    }

    [PunRPC]
    void RPC_AddPlayerToRace(int playerID)
    {
        // Retrieve race
        Race race = FindObjectOfType<Race>();

        // Retrieve player view
        PhotonView playerView = PhotonView.Find(playerID);

        // Retrieve player controller
        PlayerController player = playerView.gameObject.GetComponent<PlayerController>();
        
        // Setup race values
        race.InitiateRace(1, 2);
        race.AddParticipantIntoRace(player);
    }
}
