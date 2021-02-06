using UnityEngine;
using UnityStandardAssets.Utility;

public class RaceManager : MonoBehaviour
{
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
            if(heroBeachRace.participants.Count < heroBeachRace.raceCapacity)
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
        // Changing this just to get it working for the release
        // Will change back to previous implementation later
        Race heroBeachRace = FindObjectOfType<Race>();

        // Changing this just to get it working for the release
        // Will change back to previous implementation later
        player.GetComponent<WaypointProgressTracker>().Circuit = heroBeachRace.gameObject.GetComponent<WaypointCircuit>();
        player.GetComponent<WaypointProgressTracker>().currentRace = heroBeachRace;
        player.GetComponent<WaypointProgressTracker>().lastIndex = heroBeachRace.route.Length-1;
        heroBeachRace.raceInitiated = true;
        heroBeachRace.timeRaceInitiated = Time.timeSinceLevelLoad;
        heroBeachRace.numberOfLaps = 1;
        heroBeachRace.raceCapacity = 1;
        heroBeachRace.AddParticipantIntoRace(player);
    }
}
