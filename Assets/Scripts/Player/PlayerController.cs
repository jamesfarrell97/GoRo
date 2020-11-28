using UnityEngine;
using Photon.Pun;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class PlayerController : MonoBehaviour
{
    [SerializeField] Boat boat;
    [SerializeField] float boatSpeed = 30f;
    [SerializeField] float boatTurningSpeed = 1f;

    [SerializeField] GameObject leftTurningPoint;
    [SerializeField] GameObject rightTurningPoint;

    private BoxCollider boxCollider;
    private Rigidbody rigidBody;

    private PhotonView photonView;
    private AchievementTracker achievementTracker;
    private Stats stats;

    private bool allowedMove = true;

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
        if (photonView.IsMine) return;

        Destroy(GetComponentInChildren<Camera>().gameObject);
        Destroy(rigidBody);
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        achievementTracker.TrackAchievements(photonView, stats);

        if (!allowedMove) return;

        if (Input.GetKey(KeyCode.W))
        {
            rigidBody.AddForce(transform.forward * boatSpeed);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            rigidBody.AddForce(-transform.forward * boatSpeed);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            rigidBody.AddTorque(transform.up * boatTurningSpeed);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rigidBody.AddTorque(-transform.up * boatTurningSpeed);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            rigidBody.AddTorque(transform.up * boatTurningSpeed);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            rigidBody.AddTorque(-transform.up * boatTurningSpeed);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag.Equals("Obstacle"))
        {
            FindObjectOfType<AudioManager>().Play("Collision");
        }    
    }

    public void PauseMovement()
    {
        this.allowedMove = false;
    }

    public void ResumeMovement()
    {
        this.allowedMove = true;
    }
}
