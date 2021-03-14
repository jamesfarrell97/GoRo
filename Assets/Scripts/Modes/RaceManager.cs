using UnityEngine;
using static Race;

public class RaceManager : MonoBehaviour
{
    //PhotonView photonView;
    public static RaceManager Instance;

    private void Awake()
    {
        //photonView = GetComponent<PhotonView>();

        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    //Create a race and add the player who created it, into the race
    public void CreateRace(PlayerController player, UnityStandardAssets.Utility.Route chosenRoute, float secondsForWait, int numberOfLaps, int raceCapacity)
    {
        chosenRoute.GetComponent<Race>().InitiateRace(secondsForWait, numberOfLaps, raceCapacity);
        AddPlayerToRace(player, chosenRoute);
    }

    //Add a player into an existing race which has not began yet
    public void AddPlayerToRace(PlayerController player, UnityStandardAssets.Utility.Route chosenRoute)
    {
        Race race = chosenRoute.GetComponent<Race>();

        // Retrieve waypoint progress tracker
        UnityStandardAssets.Utility.RouteFollower routeFollower = player.GetComponent<UnityStandardAssets.Utility.RouteFollower>();

        // Setup wpt values
        //////////wpt.SetCircuit(race.gameObject.GetComponent<WaypointCircuit>());
        //////////wpt.UpdateLastNodeIndex(race.track.Length - 1);
        //////////wpt.SetRace(race);
        //////////wpt.routeType = chosenRoute.routeType;
        routeFollower.UpdateRoute(chosenRoute, race.numberOfLaps);
        routeFollower.UpdateLastNodeIndex(race.track.Length - 1);
        routeFollower.routeType = chosenRoute.routeType;

        chosenRoute.GetComponent<Race>().AddPlayerToRace(player);
    }

    //private Race[] races;

    //void Start()
    //{
    //    races = FindObjectsOfType<Race>();
    //}

    //public void JoinRace(PlayerController player, string route)
    //{
    //    // For each race in the races array
    //    foreach (Race race in races)
    //    {
    //        // Skip loop if current race is not equal to the select route
    //        if (!race.name.Equals(route)) continue;

    //        // Switch based on race state
    //        switch (race.state)
    //        {
    //            // Race currently inactive
    //            case RaceState.Inactive:

    //                // Form new race
    //                race.FormRace(player);
    //                break;

    //            // Race currently forming
    //            case RaceState.Forming:

    //                // Add player to the race
    //                race.AddPlayerToRace(player);
    //                break;
    //        }

    //        // No need to check any more in the loop
    //        return;
    //    }
    //}
}

