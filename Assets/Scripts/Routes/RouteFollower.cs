using UnityEngine;
using static PlayerController;

// Code adapted from the Unity Standard Assets WaypointProgressTracker class
namespace UnityStandardAssets.Utility
{
    public class RouteFollower : MonoBehaviour
    {
        [SerializeField] public Route route;
        [SerializeField] public Route[] routes;

        [SerializeField] private float translationalVelocity = 5f;
        [SerializeField] private float translationVelocityFactor = .1f;

        [SerializeField] private float rotationalVelocity = 10;
        [SerializeField] private float rotationalVelocityFactor = .2f;
        [SerializeField] private float minimumThreshold = 1f;

        [SerializeField] private Transform target;

        [HideInInspector] public float progressAlongRoute;
        [HideInInspector] public int numberOfLaps;
        [HideInInspector] public int currentLap;
        
        [HideInInspector] public Route.RoutePoint targetPoint { get; private set; }
        [HideInInspector] public Route.RoutePoint speedPoint { get; private set; }
        [HideInInspector] public Route.RoutePoint progressPoint { get; private set; }

        private PlayerController player;
        private Vector3 previousPosition;

        private bool halfPointReached;
        private float speed;

        private void Awake()
        {
            routes = FindObjectsOfType<Route>();
            route = routes[0];
        }
        
        private void Start()
        {
            player = GetComponent<PlayerController>();

            previousPosition = transform.position;

            // we use a transform to represent the point to aim for, and the point which
            // is considered for upcoming changes-of-speed. This allows this component
            // to communicate this information to the AI without requiring further dependencies.

            // you can manually create a transform and assign it to this component *and* the AI,
            // then this component will update it, and the AI can read it.
            if (target == null)
            {
                target = new GameObject(name + " Waypoint Target").transform;
            }

            Reset();
        }

        private void FixedUpdate()
        {
            CheckIfLapComplete();

            if (player.Paused()) return;

            if (route == null) return;
            
            if (route.GetRoutePoint(0).Equals(null)) return;

            translationalVelocity = player.GetVelocity();
             
            // determine the position we should currently be aiming for
            // (this is different to the current progress position, it is a a certain amount ahead along the route)
            // we use lerp as a simple way of smoothing out the speed over time.
            if (Time.fixedDeltaTime > 0)
            {
                UpdatePosition();

                if (!halfPointReached)
                {
                    // Check if half point reached
                    CheckIfHalfPointReached();
                }
            }
        }

        public void Reset()
        {
            progressAlongRoute = 0;
            currentLap = 1;
            speed = 0;
        }

        public void UpdateRoute(Route route, int numberOfLaps)
        {
            this.route = route;
            this.numberOfLaps = numberOfLaps;
 
            Reset();
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            speed = Mathf.Lerp(speed, (previousPosition - transform.position).magnitude / Time.fixedDeltaTime, Time.fixedDeltaTime);

            target.position =
                route.GetRoutePoint(progressAlongRoute + translationalVelocity + translationVelocityFactor * speed).position;

            target.rotation =
                Quaternion.LookRotation(
                    route.GetRoutePoint(progressAlongRoute + rotationalVelocity + rotationalVelocityFactor * speed).direction
                );
            
            // Calculate progress along route
            progressPoint = route.GetRoutePoint(progressAlongRoute);
            Vector3 progressDelta = progressPoint.position - transform.position;
            if (Vector3.Dot(progressDelta, progressPoint.direction) < 0)
            {
                progressAlongRoute += progressDelta.magnitude * 0.5f;
            }

            previousPosition = transform.position;
        }

        public bool CheckIfPointReached(Transform point)
        {
            // Only check for players who are currently participating in a race or time trial
            if (player.state != PlayerState.ParticipatingInRace && player.state != PlayerState.ParticipatingInTrial) return false;

            // Return true if distance to point is less than the minimum threshold
            return (Vector3.Distance(transform.position, point.position) < minimumThreshold);
        }

        private void CheckIfHalfPointReached()
        {
            // If player participating in race
            if (player.state.Equals(PlayerState.ParticipatingInRace))
            {
                // True if the player is near the halfway point
                halfPointReached = (
                    Vector3.Distance(
                        transform.position, 
                        player.race.route.waypointList.items[player.race.route.waypointList.items.Length / 2].position
                    ) < minimumThreshold
                );
            }

            // Else if player participating in trial
            else if (player.state.Equals(PlayerState.ParticipatingInTrial))
            {
                // True if the player is near the halfway point
                halfPointReached = (
                    Vector3.Distance(
                        transform.position,
                        player.trial.route.waypointList.items[player.trial.route.waypointList.items.Length / 2].position
                    ) < minimumThreshold
                );
            }
        }

        private void CheckIfLapComplete()
        {
            // If half point reached
            if (halfPointReached)
            {
                float distance = 0;
                
                // If player participating in race
                if (player.state.Equals(PlayerState.ParticipatingInRace))
                {
                    // Calculate distance to finish line
                    // Assumes finish line is at the the start point
                    distance =
                        Vector3.Distance(
                            transform.position,
                            player.race.route.waypointList.items[0].position
                        );
                }

                // Else if player participating in trial
                else if (player.state.Equals(PlayerState.ParticipatingInTrial))
                {
                    // Calculate distance to finish line
                    // Assumes finish line is at the the start point
                    distance =
                        Vector3.Distance(
                            transform.position,
                            player.trial.route.waypointList.items[0].position
                        );
                }

                // If within range of finish line
                if (distance < minimumThreshold)
                {
                    // Update lap count
                    UpdateLapCount();
                }
            }
        }

        private void UpdateLapCount()
        {
            // Reset variable
            halfPointReached = false;

            // If laps remaining
            if (currentLap < numberOfLaps)
            {
                // Update lap count
                currentLap++;
            }

            // Otherwise
            else
            {
                // Complete event
                CompleteEvent();
                Reset();
            }
        }

        private void CompleteEvent()
        {
            if (player.state.Equals(PlayerState.ParticipatingInRace))
            {
                player.state = PlayerState.AtRaceFinishLine;
                player.race.PlayerCompletedRace(player);
            }
            else if (player.state.Equals(PlayerState.ParticipatingInTrial))
            {
                player.state = PlayerState.CompletedTimeTrial;
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target.position);
                Gizmos.DrawWireSphere(route.GetRoutePosition(progressAlongRoute), 3);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(target.position, target.position + target.forward);
            }
        }
    }
}
