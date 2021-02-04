using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class TimeTrialManager : MonoBehaviour
{
    public void TakePartInATimeTrial(Boat player)
    {
        //Will initiate Menu Screen to appear where player will be prompted to select time trial track they wish to take part in
        //From this interaction the following methods starting with "AddPlayerToXXX" represent the button pressed actions relevant to time trial selected
    }

    #region Time Trial Initiation Button Responses
    public void AddPlayerToHeroBeachTimeTrial()
    {
        TimeTrial heroBeachTimeTrial = GameObject.Find("Hero Beach Island Time Trial").GetComponent<TimeTrial>();
        if (heroBeachTimeTrial.timeTrialInitiated == false)
        {
            //BringUpRaceSetupMenu
            //SetAllRaceParametersBased on the feedback given by player
            heroBeachTimeTrial.timeTrialInitiated = true;
            //heroBeachRace.numberOfLaps = ?;
            //heroBeachRace.raceCapacity = ?;
            //Add player to participants list
        }
    }
    #endregion Time Trial Initiation Button Responses

    //HardCoded Method to reach simple solution for Base Version of the 
    //waypoint/ race/ time trial mechanics until the menu and more race routes are implemented 
    public void AddPlayerToTimeTrial(Boat player)
    {
        TimeTrial heroBeachTimeTrial = GameObject.Find("Hero Beach Island Time Trial").GetComponent<TimeTrial>();
        player.GetComponent<WaypointProgressTracker>().Circuit = GameObject.Find("Hero Beach Island Time Trial").GetComponent<WaypointCircuit>();
        player.GetComponent<WaypointProgressTracker>().currentTimeTrial = heroBeachTimeTrial;
        player.GetComponent<WaypointProgressTracker>().lastIndex = heroBeachTimeTrial.route.Length - 1;
        heroBeachTimeTrial.timeTrialInitiated = true;
        heroBeachTimeTrial.timeTheTimeTrialInitiated = Time.timeSinceLevelLoad;
        heroBeachTimeTrial.numberOfLaps = 3;
        heroBeachTimeTrial.AddParticipantIntoTimeTrial(player);
    }


}
