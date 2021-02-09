using UnityEngine;
using UnityStandardAssets.Utility;

public class TimeTrialManager : MonoBehaviour
{
    public void TakePartInATimeTrial(Boat player)
    {
        // Will initiate Menu Screen to appear where player will be prompted to select time trial track they wish to take part in
        // From this interaction the following methods starting with "AddPlayerToXXX" represent the button pressed actions relevant to time trial selected
    }

    #region Time Trial Initiation Button Responses
    public void AddPlayerToHeroBeachTimeTrial()
    {
        // Changing this just to get it working for the release
        // Will change back to previous implementation later
        TimeTrial heroBeachTimeTrial = FindObjectOfType<TimeTrial>();
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

    // Hard coded method to reach simple solution for Base Version of the 
    // waypoint/ race/ time trial mechanics until the menu and more race routes are implemented 
    public void AddPlayerToTimeTrial(PlayerController player)
    {
        player.participatingInTimeTrial = true;

        // Changing this just to get it working for the release
        // Will change back to previous implementation later
        TimeTrial heroBeachTimeTrial = FindObjectOfType<TimeTrial>();

        // Changing this just to get it working for the release
        // Will change back to previous implementation later
        player.GetComponent<WaypointProgressTracker>().Circuit = heroBeachTimeTrial.gameObject.GetComponent<WaypointCircuit>();
        player.GetComponent<WaypointProgressTracker>().currentTimeTrial = heroBeachTimeTrial;
        player.GetComponent<WaypointProgressTracker>().lastIndex = heroBeachTimeTrial.route.Length - 1;
        heroBeachTimeTrial.timeTrialInitiated = true;
        heroBeachTimeTrial.timeTheTimeTrialInitiated = Time.timeSinceLevelLoad;
        heroBeachTimeTrial.numberOfLaps = 3;
        heroBeachTimeTrial.AddParticipantIntoTimeTrial(player);
    }
}
