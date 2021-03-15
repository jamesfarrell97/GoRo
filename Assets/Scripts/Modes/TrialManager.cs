using UnityEngine;
using UnityStandardAssets.Utility;

public class TrialManager : MonoBehaviour
{
    public static TrialManager Instance;

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

    public void JoinTrial(PlayerController player, string route)
    {
        //Trial trial = route.
        //// Skip loop if current trial is not equal to the select route
        //if (!trial.name.Equals(route)) continue;

        //// Switch based on trial state
        //switch (trial.state)
        //{
        //    // trial currently inactive
        //    case TrialState.Inactive:

        //        // Form trial
        //        trial.FormTrial(player, route);
        //        break;

        //    case TrialState.InProgress:

        //        // Do nothing
        //        break;
        //}

        //// No need to check any more in the loop
        //return;

    }

    public void CreateTrialAndAddPlayer(PlayerController player, Route chosenRoute, int numberOfLaps)
    {
        player.GetComponent<PlayerController>().state = PlayerController.PlayerState.ParticipatingInTrial;

        Trial trial = chosenRoute.GetComponent<Trial>();

        RouteFollower routeFollower = player.GetComponent<RouteFollower>();

        routeFollower.UpdateRoute(chosenRoute, numberOfLaps);
        routeFollower.UpdateLastNodeIndex(trial.track.Length - 1);
        routeFollower.routeType = chosenRoute.routeType;
        trial.FormTrial(player, chosenRoute.name);

        //trial.timeTrialInitiated = true;
        //trial.timeTheTimeTrialInitiated = Time.timeSinceLevelLoad;
        //trial.numberOfLaps = numberOfLaps;
        //trial.AddParticipantIntoTimeTrial(player);
    }

}

//using static Trial;
//public class TrialManager : MonoBehaviour
//{
//    private Trial[] trials;

//    void Start()
//    {
//        trials = FindObjectsOfType<Trial>();
//    }

//    public void JoinTrial(PlayerController player, string route)
//    {
//        // For each trial in the trials array
//        foreach (Trial trial in trials)
//        {
//            // Skip loop if current trial is not equal to the select route
//            if (!trial.name.Equals(route)) continue;

//            // Switch based on trial state
//            switch (trial.state)
//            {
//                // trial currently inactive
//                case TrialState.Inactive:

//                    // Form trial
//                    trial.FormTrial(player, route);
//                    break;

//                case TrialState.InProgress:

//                    // Do nothing
//                    break;
//            }

//            // No need to check any more in the loop
//            return;
//        }
//    }
//}

