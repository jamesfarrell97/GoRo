using UnityEngine;

using static Trial;
public class TrialManager : MonoBehaviour
{
    private Trial[] trials;

    void Start()
    {
        trials = FindObjectsOfType<Trial>();
    }

    public bool JoinTrial(PlayerController player, string route)
    {
        // For each trial in the trials array
        foreach (Trial trial in trials)
        {
            // Skip loop if current trial is not equal to the select route
            if (!trial.name.Equals(route)) continue;

            // Switch based on trial state
            switch (trial.state)
            {
                // trial currently inactive
                case TrialState.Inactive:

                    // Form trial
                    trial.FormTrial(player, route);
                    return true;

                case TrialState.InProgress:

                    // Do nothing
                    return false;
            }

            // No need to check any more in the loop
            return false;
        }

        return false;
    }
}