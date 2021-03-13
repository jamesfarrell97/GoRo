using UnityEngine;
using UnityStandardAssets.Utility;

public class TimeTrialManager : MonoBehaviour
{
    public static TimeTrialManager Instance;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public void CreateTrialAndAddPlayer(PlayerController player, Route chosenRoute, int numberOfLaps)
    {
        player.participatingInTimeTrial = true;

        TimeTrial trial = chosenRoute.GetComponent<TimeTrial>();

        // Retrieve waypoint progress tracker
        WaypointProgressTracker wpt = player.GetComponent<WaypointProgressTracker>();

        // Setup wpt values
        wpt.SetCircuit(trial.gameObject.GetComponent<WaypointCircuit>());
        wpt.UpdateLastNodeIndex(trial.track.Length - 1);
        wpt.currentTimeTrial = trial;
        wpt.routeType = chosenRoute.routeType;

        trial.timeTrialInitiated = true;
        trial.timeTheTimeTrialInitiated = Time.timeSinceLevelLoad;
        trial.numberOfLaps = numberOfLaps;
        trial.AddParticipantIntoTimeTrial(player);
    }

    //public void AddPlayerToHeroBeachTimeTrial()
    //{
    //    // Changing this just to get it working for the release
    //    // Will change back to previous implementation later
    //    TimeTrial heroBeachTimeTrial = FindObjectOfType<TimeTrial>();
    //    if (heroBeachTimeTrial.timeTrialInitiated == false)
    //    {
    //        //BringUpRaceSetupMenu
    //        //SetAllRaceParametersBased on the feedback given by player
    //        heroBeachTimeTrial.timeTrialInitiated = true;
    //        //heroBeachRace.numberOfLaps = ?;
    //        //heroBeachRace.raceCapacity = ?;
    //        //Add player to participants list
    //    }
    //}

    //// Hard coded method to reach simple solution for Base Version of the 
    //// waypoint/ race/ time trial mechanics until the menu and more race routes are implemented 
    //public void AddPlayerToTimeTrial(PlayerController player)
    //{
    //    player.participatingInTimeTrial = true;

    //    // Changing this just to get it working for the release
    //    // Will change back to previous implementation later
    //    TimeTrial heroBeachTimeTrial = FindObjectOfType<TimeTrial>();

    //    // Changing this just to get it working for the release
    //    // Will change back to previous implementation later
    //    player.GetComponent<WaypointProgressTracker>().Circuit = heroBeachTimeTrial.gameObject.GetComponent<WaypointCircuit>();
    //    player.GetComponent<WaypointProgressTracker>().currentTimeTrial = heroBeachTimeTrial;
    //    player.GetComponent<WaypointProgressTracker>().lastIndex = heroBeachTimeTrial.track.Length - 1;
    //    heroBeachTimeTrial.timeTrialInitiated = true;
    //    heroBeachTimeTrial.timeTheTimeTrialInitiated = Time.timeSinceLevelLoad;
    //    heroBeachTimeTrial.numberOfLaps = 3;
    //    heroBeachTimeTrial.AddParticipantIntoTimeTrial(player);
    //}
}
