using UnityEngine;

using static PlayerController;

// Code adapted from the Unity Standard Assets WaypointProgressTracker class
namespace UnityStandardAssets.Utility
{
    public class RouteFollower : MonoBehaviour
    {
        [HideInInspector] public Route route;
        [HideInInspector] public Route[] routes;

        [SerializeField] private Transform target;

        [HideInInspector] public float progressAlongRoute;
        [HideInInspector] public int numberOfLaps;
        [HideInInspector] public int currentLap;
        
        [HideInInspector] public Route.RoutePoint targetPoint { get; private set; }
        [HideInInspector] public Route.RoutePoint speedPoint { get; private set; }
        [HideInInspector] public Route.RoutePoint progressPoint { get; private set; }

        private Vector3 offset;

        private PlayerController player;
        private GhostController ghost;
        private Vector3 previousPosition;

        private float translationalVelocity = 5f;
        private float translationVelocityFactor = .1f;

        private float rotationalVelocity = 10;
        private float rotationalVelocityFactor = .2f;
        private float minimumThreshold = 1f;

        private bool halfPointReached;
        private float speed;

        private void Awake()
        {
            routes = FindObjectsOfType<Route>();
            route = FindRoute("Cliffs");
        }
        
        private Route FindRoute(string name)
        {
            foreach (Route route in routes)
            {
                if (route.name.Equals(name)) return route;
            }

            return routes[0];
        }

        private void Start()
        {
            if (GetComponent<PlayerController>() != null)
            {
                player = GetComponent<PlayerController>();
            }

            if (GetComponent<GhostController>() != null)
            {
                ghost = GetComponent<GhostController>();
            }

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

            if (player != null && (!player.photonView.IsMine || player.Paused())) return;

            if (ghost != null && ghost.Paused()) return;

            if (route == null) return;

            if (route.GetRoutePoint(0).Equals(null)) return;

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
            startPosition = target.position;
            startRotation = target.rotation;

            targetPosition = startPosition;
            targetRotation = startRotation;

            progressAlongRoute = 0;
            currentLap = 1;
            speed = 0;

            SetPosition();
        }

        public float range = 0;
        public float totalDistance = 0;

        public int delta = 0;

        public void UpdateDistance(float distance)
        {
            // Subtract deleta form distance to calculate progress
            progressAlongRoute = distance;

            // Set start position
            startPosition = target.position;

            // Set start rotation
            startRotation = target.rotation;

            // Calculate target position
            targetPosition = route.GetRoutePoint(progressAlongRoute).position;

            // Calculate target rotation
            targetRotation = Quaternion.LookRotation(
                    route.GetRoutePoint(progressAlongRoute).direction
                );

            // Reset lerp time
            lerpTime = 0;
        }

        public void UpdateRoute(Route route, int numberOfLaps)
        {
            this.route = route;
            this.numberOfLaps = numberOfLaps;

            this.progressAlongRoute = 0;

            Reset();
        }

        public Vector3 startPosition;
        public Quaternion startRotation;
        public Vector3 targetPosition;
        public Quaternion targetRotation;

        public float lerpTime = 0;

        public void UpdatePosition()
        {

#if UNITY_EDITOR

            // Calculate lerp time based on sample rate
            lerpTime += Time.fixedDeltaTime / (1f / StatsManager.MOVE_SAMPLE_RATE);

#else

            // Calculate lerp time based on sample rate
            // This has been changed to only expect samples every one second
            lerpTime += Time.fixedDeltaTime / (1f / StatsManager.MOVE_SAMPLE_RATE);

#endif

            // Update position
            target.position = Vector3.Lerp(startPosition, targetPosition, lerpTime);

            // Update rotation
            target.rotation = Quaternion.Lerp(startRotation, targetRotation, lerpTime);
        }

        public void SetPosition()
        {
            // Set position
            target.position = route.GetRoutePoint(progressAlongRoute).position;

            // Set rotation
            target.rotation = Quaternion.LookRotation(
                    route.GetRoutePoint(progressAlongRoute).direction
                );
        }

        public bool CheckIfPointReached(Transform point)
        {
            if (player != null)
            {
                // Only check for players who are currently participating in a race or time trial
                if (player.state != PlayerState.ParticipatingInRace && player.state != PlayerState.ParticipatingInTrial) return false;

                // Return true if distance to point is less than the minimum threshold
                return (Vector3.Distance(transform.position, point.position) < minimumThreshold);
            }

            else if (ghost != null)
            {
                // Return true if distance to point is less than the minimum threshold
                return (Vector3.Distance(transform.position, point.position) < minimumThreshold);
            }

            return false;
        }

        private void CheckIfHalfPointReached()
        {
            if (player != null)
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

            else if (ghost != null)
            {
                // True if the ghost is near the halfway point
                halfPointReached = (
                    Vector3.Distance(
                        transform.position,
                        ghost.trial.route.waypointList.items[ghost.trial.route.waypointList.items.Length / 2].position
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

                if (player != null)
                {
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
                }

                if (ghost != null)
                {
                    // Calculate distance to finish line
                    // Assumes finish line is at the the start point
                    distance =
                        Vector3.Distance(
                            transform.position,
                            ghost.trial.route.waypointList.items[0].position
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
            if (player != null)
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

            else if (ghost != null)
            {
                ghost.Pause();
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
