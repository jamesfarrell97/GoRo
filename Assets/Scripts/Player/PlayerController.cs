using UnityStandardAssets.Utility;
using UnityEngine;
using Photon.Pun;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class PlayerController : MonoBehaviour
{
    [SerializeField] Boat boat;

    [SerializeField] float boatSpeed = 0f;

    [SerializeField] Animator[] rowingAnimators;
    [SerializeField] Transform[] route;

    public bool participatingInRace = false;
    public bool participatingInTimeTrial = false;

    private AchievementTracker achievementTracker;
    private BoxCollider boxCollider;
    private PhotonView photonView;
    private Stats stats;

    private bool allowedMove = true;
    private bool moveForward = false;
    private bool moveBack = false;

    private float speed;

    private void Awake()
    {
        achievementTracker = GetComponent<AchievementTracker>();
        boxCollider = GetComponent<BoxCollider>();
        photonView = GetComponent<PhotonView>();
        stats = GetComponent<Stats>();
    }

    private void Start()
    {
        if (photonView.IsMine) return;

        Destroy(GetComponentInChildren<Camera>().gameObject); // Main Camera
        Destroy(GetComponentInChildren<Camera>().gameObject); // Minimap Camera
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        achievementTracker.TrackAchievements(photonView, stats);

        if (!allowedMove)
        {
            velocity = 0;
            return;
        }

        if (Input.GetKey(KeyCode.W))
        {
            moveForward = true;
        }
        else
        {
            moveForward = false;
        }

        UpdateSpeed();
        CalculateVelocity();
        Animate();
    }

    public float speedIncreaseFactor = 0.1f;
    public float speedDecayFactor = 0.1f;

    private void UpdateSpeed()
    {
        if (moveForward)
        {
            speed = 5;
        }
        else
        {
            speed = 0;
        }

        //speed = stats.GetSpeed() * 1000;
    }

    private float velocity;
    private void CalculateVelocity()
    {
        // speed is measured in meters per second and this function is ran 
        // fixedDeltaTime's per second - so, we can figure out how far to 
        // move this update, if we evenly spread the speed out across the 
        // second by multiplying it by fixedDeltaTime

        velocity = speed * Time.deltaTime;
    }

    private void Animate()
    {
        foreach (Animator animator in rowingAnimators)
        {
            if (speed > 0)
            {
                animator.SetBool("Play", true);
            }
            else
            {
                animator.SetBool("Play", false);
            }
        }
    }

    #region Race/Time Trial Event Methods
    public void StartARace()
    {
        if (participatingInTimeTrial == false && participatingInRace == false)
        {
            participatingInRace = true;

            // Reset track distance
            GetComponent<WaypointProgressTracker>().progressDistance = 0;

            GameObject.Find("Race Manager").GetComponent<RaceManager>().AddPlayerToRace(this);
        }
    }

    public void StartATimeTrial()
    {
        if (participatingInRace == false && participatingInTimeTrial == false)
        {
            participatingInTimeTrial = true;

            // Reset track distance
            GetComponent<WaypointProgressTracker>().progressDistance = 0;

            GameObject.Find("Time Trial Manager").GetComponent<TimeTrialManager>().AddPlayerToTimeTrial(this);
        }
    }

    public void JustRow()
    {
        WaypointProgressTracker waypointProgressTracker = GetComponent<WaypointProgressTracker>();

        // Reset track distance
        waypointProgressTracker.progressDistance = 0;

        // Reset track
        waypointProgressTracker.Circuit = waypointProgressTracker.Routes[0];
    }
    #endregion

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

    public float GetVelocity()
    {
        return velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag.Equals("Obstacle"))
        {
            FindObjectOfType<AudioManager>().Play("Collision");
        }
    }
}
