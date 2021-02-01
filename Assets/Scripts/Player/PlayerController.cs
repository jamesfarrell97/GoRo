using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityStandardAssets.Utility;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class PlayerController : MonoBehaviour
{
    [SerializeField] Boat boat;
    [SerializeField] public float boatSpeed = 0f;
    [SerializeField] float boatTurningSpeed = 1f;
    [SerializeField] public bool participatingInRace = false;
    [SerializeField] public bool participatingInTimeTrial = false;

    [SerializeField] public Transform[] route;

    private BoxCollider boxCollider;
    private Rigidbody rigidBody;

    private PhotonView photonView;
    private AchievementTracker achievementTracker;
    private Stats stats;

    private bool allowedMove = true;
    private bool moveForward = false;
    private bool moveBack = false;
    private bool moveLeft = false;
    private bool moveRight = false;
    private bool rotateLeft = false;
    private bool rotateRight = false;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        rigidBody = GetComponent<Rigidbody>();

        photonView = GetComponent<PhotonView>();
        achievementTracker = GetComponent<AchievementTracker>();
        stats = GetComponent<Stats>();
    }

    private void Start()
    {
        //GetComponent<WaypointProgressTracker>().Circuit = FindObjectOfType<WaypointCircuit>();

        if (photonView.IsMine) return;

        previousPosition = transform.position;

        Destroy(GetComponentInChildren<Camera>().gameObject);
        Destroy(rigidBody);
    }

    private void Update()
    {


    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        achievementTracker.TrackAchievements(photonView, stats);

        if (!allowedMove) return;

        CalculateVelocity();
        CalculateTurnAngle();
    }

    #region Race/Time Trial Event Methods
    public void StartARace()
    {
        if (participatingInTimeTrial == false && participatingInRace == false)
        {
            participatingInRace = true;
            GameObject.Find("Race Manager").GetComponent<RaceManager>().AddPlayerToRace(boat);
        }
    }

    public void StartATimeTrial()
    {
        //if (participatingInRace == false && participatingInTimeTrial == false)
        //{
        //    participatingInTimeTrial = true;
        //    GameObject.Find("Time Trial Manager").GetComponent<TimeTrialManager>().AddPlayerToTimeTrial(boat);
        //}
    }
    #endregion

    public void CalculateVelocity()
    {
        //currDist = BluetoothManager.RowingStatusData[3] * 10;   // Convert to meters
        //currTime = BluetoothManager.RowingStatusData[0] * 100;  // Convert to seconds

        //deltDist = currDist - prevDist;
        //deltTime = currTime - prevTime;

        //velocity = deltDist / deltTime;

        //rigidBody.AddForce(-transform.forward * boatSpeed * Mathf.Abs(velocity / 100));

        //prevDist = currDist;
        //prevTime = currTime;

        if (Input.GetKey(KeyCode.W) || moveForward)
        {
            speed = boatSpeed;
            transform.position += transform.forward * speed;
        }
        else if (Input.GetKey(KeyCode.S) || moveBack)
        {
            speed = -boatSpeed;
            transform.position += transform.forward * speed;
        }
    }

    int nodeIndex = 0;
    Transform currentNode;

    private float distance = 0;
    private float prevDist = 0;
    private float angle = 0;
    private float prevAngl = 0;
    private float currTime = 0;
    private float prevTime = 0;
    private float deltaDistance = 0;
    private float deltAngl = 0;
    private float deltTime = 0;
    private float time = 0;
    private float velocity = 0;
    private float speed = 0;

    private Vector3 previousPosition;
    private Vector3 cross;

    private const float MIN_DISTANCE = 1f;

    public void CalculateTurnAngle()
    {
        return;

        // Only update if moving
        if (speed == 0) return;


        // Extract target node
        //
        // Doesn't need to called every update - performance intensive
        // Remove in future iterations
        Transform targetNode = route[nodeIndex];

        // If the player is at the target node
        if (Vector3.Distance(transform.position, targetNode.position) < 1f)
        {
            if (nodeIndex < route.Length - 1)
            {
                nodeIndex++;
            }
            else
            {
                nodeIndex = 0;
            }

            //// Iterate node
            //nodeIndex = (nodeIndex < route.Length - 1) 
            //    ? nodeIndex++ 
            //    : 0;
            return;
        }

        // Distance to target
        distance = Vector3.Distance(transform.position, targetNode.position);

        // Distance moved since last frame
        deltaDistance = Vector3.Distance(transform.position, previousPosition);

        // Direction to target position from current position
        Vector3 direction = (targetNode.position - transform.position);

        // Angle between current direction and target direction
        angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);

        // Calculate velocity
        // 1 units per timeStep
        //velocity = deltaDistance / Time.fixedDeltaTime;

        // Predicted time to reach node based on current velocity
        // 20 / 2 
        time = distance / deltaDistance;

        // Required angle per time step to successfully face next node
        angle = angle / time;

        // Turn to face next node
        transform.Rotate(Vector3.up, angle);

        // Update values
        previousPosition = transform.position;
        prevDist = distance;

        // Update position
        transform.position = transform.position + (direction.normalized * speed);

        //prevAngl = currAngl;
        //prevTime = currTime;
    }

    public void MoveForward()
    {
        moveForward = true;
    }

    public void MoveBack()
    {
        moveBack = true;
    }

    public void StopMoveForward()
    {
        moveForward = false;
    }

    public void StopMoveBack()
    {
        moveBack = false;
    }

    public void PauseMovement()
    {
        this.allowedMove = false;
    }

    public void ResumeMovement()
    {
        this.allowedMove = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag.Equals("Obstacle"))
        {
            FindObjectOfType<AudioManager>().Play("Collision");
        }
    }
}
