using UnityEngine;

using static Test;
public class TestManager : MonoBehaviour
{
    private Test[] races;

    // Start is called before the first frame update
    void Start()
    {
        races = FindObjectsOfType<Test>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        TestController player = other.GetComponent<TestController>();

        JoinRace(player, "Race");
    }

    public void JoinRace(TestController player, string route)
    {
        // For each race in the races array
        foreach (Test race in races)
        {
            // Skip loop if current race is not equal to the select route
            if (!race.name.Equals(route)) continue;

            // Switch based on race state
            switch(race.state)
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
