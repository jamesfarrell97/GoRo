using UnityEngine;
using static PlayerController;

// Code adapted from the Unity Standard Assets WaypointProgressTracker class
namespace UnityStandardAssets.Utility
{
    public class WaypointProgressTracker : MonoBehaviour
    {
        [SerializeField] public WaypointCircuit route;
        [SerializeField] public WaypointCircuit[] routes;

        [SerializeField] private float translationalVelocity = 5f;
        [SerializeField] private float translationVelocityFactor = .1f;

        [SerializeField] private float rotationalVelocity = 10;
        [SerializeField] private float rotationalVelocityFactor = .2f;
        [SerializeField] private float minimumThreshold = 1f;

        [SerializeField] private Transform target;

        [HideInInspector] public float progressAlongRoute;
        [HideInInspector] public int numberOfLaps;
        [HideInInspector] public int currentLap;
        
        [HideInInspector] public WaypointCircuit.RoutePoint targetPoint { get; private set; }
        [HideInInspector] public WaypointCircuit.RoutePoint speedPoint { get; private set; }
        [HideInInspector] public WaypointCircuit.RoutePoint progressPoint { get; private set; }

        private PlayerController player;
        private Vector3 previousPosition;

        private bool halfPointReached;
        private float speed;

        private void Awake()
        {
            routes = FindObjectsOfType<WaypointCircuit>();
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

        public void UpdateRoute(WaypointCircuit route, int numberOfLaps)
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
            if (player.state != PlayerState.ParticipatingInRace && player.state != PlayerState.ParticipatingInTimeTrial) return false;

            // Return true if distance to point is less than the minimum threshold
            return (Vector3.Distance(transform.position, point.position) < minimumThreshold);
        }

        private void CheckIfHalfPointReached()
        {
            // If player participating in race
            if (player.state.Equals(PlayerState.ParticipatingInRace))
            {
                // Returns true is the player is near the race route half point
                halfPointReached = (Vector3.Distance(transform.position, player.race.route[player.race.route.Length / 2].position) < minimumThreshold);
            }

            // Else if player participating in time trial
            else if (player.state.Equals(PlayerState.ParticipatingInTimeTrial))
            {
                // Returns true is the player is near the trial route half point
                halfPointReached = (Vector3.Distance(transform.position, player.trial.route[player.trial.route.Length / 2].position) < minimumThreshold);
            }
        }

        private void CheckIfLapComplete()
        {
            // If half point reached
            if (halfPointReached)
            {
                float distance = 0;
                if (player.state.Equals(PlayerState.ParticipatingInRace))
                {
                    distance = Vector3.Distance(transform.position, player.race.route[player.race.route.Length - 1].position);
                }
                else if (player.state.Equals(PlayerState.ParticipatingInTimeTrial))
                {
                    distance = Vector3.Distance(transform.position, player.trial.route[player.trial.route.Length - 1].position);
                }

                if (distance < minimumThreshold)
                {
                    halfPointReached = false;
                    UpdateEventLapCount();
                }
            }
        }

        private void UpdateEventLapCount()
        {
            if (currentLap < numberOfLaps)
            {
                currentLap++;
            }
            else
            {
                CompleteEvent();
                Reset();
            }
        }

        private void CompleteEvent()
        {
            if (player.state.Equals(PlayerState.ParticipatingInRace))
            {
                player.state = PlayerState.AtRaceFinishLine;
            }
            else if (player.state.Equals(PlayerState.ParticipatingInTimeTrial))
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
