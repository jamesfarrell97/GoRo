using UnityEngine;
using static Race;

public class RaceManager : MonoBehaviour
{
    [SerializeField] private Race[] races;

    public void JoinRace(PlayerController player, string route)
    {
        // For each race in the races array
        foreach (Race race in races)
        {
            // Skip loop if current race is not equal to the select route
            if (!race.name.Equals(route)) continue;

            // Switch based on race state
            switch (race.state)
            {
                // Race currently inactive
                case RaceState.Inactive:

                    // Form new race
                    race.FormRace(player);
                    break;

                // Race currently forming
                case RaceState.Forming:

                    // Add player to the race
                    race.AddPlayerToRace(player);
                    break;
            }

            // No need to check any more in the loop
            return;
        }
    }
}