using UnityEngine;
using Photon.Pun;
using UnityStandardAssets.Utility;
using UnityEngine.Rendering.Universal;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class PlayerController : MonoBehaviour
{
    [SerializeField] public float boatSpeed = 1f;
    [SerializeField] [Range(0, 1f)] float speedIncreaseFactor = 0.1f;
    [SerializeField] [Range(0, 1f)] float speedDecayFactor = 0.1f;

    [SerializeField] private Animator[] rowingAnimators;
    [SerializeField] private Material otherPlayerMaterial;
    [SerializeField] private Oar leftOar;
    [SerializeField] private Oar rightOar;

    [HideInInspector] public Transform[] route;

    [HideInInspector] public bool participatingInRace = false;
    [HideInInspector] public bool participatingInTimeTrial = false;

    private AchievementTracker achievementTracker;
    private BoxCollider boxCollider;
    private Rigidbody rigidbody;
    private PhotonView photonView;
    private StatsManager stats;

    private bool allowedMove = true;
    private bool move = false;

    private void Awake()
    {
        stats = GameManager.Instance.GetComponent<StatsManager>();

        achievementTracker = GetComponent<AchievementTracker>();
        boxCollider = GetComponent<BoxCollider>();
        photonView = GetComponent<PhotonView>();

        leftOar.rowing = false;
        rightOar.rowing = false;
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            AssignMenuCamera();

            rigidbody = GameObject.Find("Rigidbody").GetComponent<Rigidbody>();
        }
        else
        {
            DestroyComponents();
            UpdateAppearance();
        }
    }

    private void AssignMenuCamera()
    {
        // Retrieve player camera
        Camera playerCamera = transform.Find("Cameras/Main Camera").GetComponent<Camera>();

        // Update menu camera
        MenuManager.Instance.GetComponentInParent<Canvas>().worldCamera = playerCamera;

        // Display HUD
        MenuManager.Instance.OpenMenu("HUD");
    }

    private void DestroyComponents()
    {
        // Destroy cameras
        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach (Camera c in cameras)
        {
            Destroy(c.GetComponent<UniversalAdditionalCameraData>());
            Destroy(c);
        }

        // Destroy waypoint tracker
        Destroy(GetComponent<WaypointProgressTracker>());

        // Destroy animation
        Destroy(GetComponent<Animation>());
    }

    private void DestroyProgressTracker()
    {
    }

    private void UpdateAppearance()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;

            for (int i = 0; i < renderer.materials.Length; i++)
            {
                // Change minimap icon color
                if (renderer.gameObject.name == "Boat Front" || renderer.gameObject.name == "Boat Rear")
                {
                    materials[i].color = Color.red;
                }

                // Destroy water mask
                else if (renderer.gameObject.name.Contains("Mask"))
                {
                    Destroy(renderer.transform.parent.gameObject.GetComponent<SetRenderQueue>());
                    Destroy(renderer.gameObject);
                }

                // Update material
                else if (!materials[i].color.Equals(null))
                {
                    materials[i] = otherPlayerMaterial;
                }
            }

            renderer.materials = materials;
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        achievementTracker.TrackAchievements(photonView);

        if (!allowedMove)
        {
            velocity = 0;
        }
        else
        {
            UpdateSpeed();
            Animate();
        }
    }

    private int strokeState = 0;
    private float velocity = 0;

    private float prevSpeed = 0;
    private float deltSpeed = 0;
    private float speed = 0;

    private enum StrokeStates
    {
        WaitingForWheelToReachMinSpeed,
        WaitingForWheelToAccelerate,
        Driving,
        DwellingAfterDrive,
        RecoveryState
    }

    private float currTime = 0;
    private void UpdateSpeed()
    {
        //#if UNITY_EDITOR
        //        if (move)
        //        {
        //            speed = (Random.Range(1.5f, 3f));
        //        }

        //        // Calculate delta speed
        //        deltSpeed = Mathf.Abs(speed - prevSpeed);

        //        // If the speed hasn't changed, don't add a force
        //        if (stats)
        //        {
        //            rigidbody.AddForce(transform.forward * speed * Time.fixedDeltaTime);
        //        }

        //        velocity = rigidbody.velocity.magnitude * boatSpeed;

        //        prevSpeed = speed;

        //        return;
        //#endif

        // Update speed - !!bring back down to end of function!!
        //prevSpeed = speed;



//#if UNITY_EDITOR

        // Increase time
        currTime += Time.deltaTime;

        // Simlating concpet2 update speed
        if (currTime > 0.5f)
        {
            speed = (Random.Range(1.75f, 2.25f));
            currTime = 0;
        }

        if (move)
        {
            // Driving
            strokeState = 2;
        }
        else
        {
            // Resting
            strokeState = 0;
        }

//#else

//        // Get speed from ERG
//        speed = stats.GetSpeed();

//        // Get stroke state from ERG
//        strokeState = stats.GetStrokeState();

//#endif

        // If driving
        if (strokeState == (int) StrokeStates.Driving)
        {
            // Apply force
            rigidbody.AddForce(transform.forward * speed * Time.deltaTime);
        }

        // Update velocity
        velocity = rigidbody.velocity.magnitude * boatSpeed;

    }

    private void Animate()
    {
        foreach (Animator animator in rowingAnimators)
        {
            if (strokeState == (int) StrokeStates.Driving)
            {
                leftOar.rowing = true;
                rightOar.rowing = true;

                animator.SetBool("Play", true);
            }
            else
            {
                leftOar.rowing = false;
                rightOar.rowing = false;

                animator.SetBool("Play", false);
            }
        }
    }

    public void Go()
    {
        move = true;
    }

    public void Stop()
    {
        move = false;
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
}
