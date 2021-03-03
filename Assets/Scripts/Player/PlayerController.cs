using UnityStandardAssets.Utility;
using UnityEngine;
using Photon.Pun;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        JustRowing,
        ParticipatingInTrial,
        CompletedTimeTrial,
        ParticipatingInRace,
        AtRaceStartLine,
        AtRaceFinishLine,
        AtBoathouse
    }

    [SerializeField] [Range(0, 3)] public float boatSpeed = 1f;

    [SerializeField] private Animator[] rowingAnimators;
    [SerializeField] private Material otherPlayerMaterial;
    [SerializeField] private Oar leftOar;
    [SerializeField] private Oar rightOar;

    [SerializeField] private Camera[] cameras;

    [HideInInspector] public Transform[] route;
    [HideInInspector] public Trial trial;
    [HideInInspector] public Race race;

    [HideInInspector] public PhotonView photonView { get; private set; }
    [HideInInspector] public PlayerState state;

    private AchievementTracker achievementTracker;
    private BoxCollider boxCollider;
    private Rigidbody rigidbody;
    private StatsManager stats;

    private bool paused = false;
    private bool move = false;

    private int cameraIndex = 0;

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
        Camera playerCamera = transform.Find("Cameras/Rear Camera").GetComponent<Camera>();
        
        // Update menu camera
        MenuManager.Instance.GetComponentInParent<Canvas>().worldCamera = playerCamera;
        
        // Display HUD
        MenuManager.Instance.OpenMenu("HUD");
    }

    private void DestroyComponents()
    {
        // Disable cameras
        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach (Camera camera in cameras)
        {
            camera.gameObject.SetActive(false);
        }

        // Destroy waypoint tracker
        Destroy(GetComponent<RouteFollower>());
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
                    // Render other player icon below ours
                    renderer.gameObject.transform.parent.Translate(new Vector3(0, -0.5f, 0));

                    materials[i].color = Color.red;
                }

                //Destroy water mask
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

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        achievementTracker.TrackAchievements(photonView);

        if (paused)
        {
            velocity = 0;
        }
        else
        {
            UpdateSpeed();
            Animate();
        }

#if UNITY_EDITOR

        UserInput();

#endif
    }

    private enum StrokeState
    {
        WaitingForWheelToReachMinSpeed,
        WaitingForWheelToAccelerate,
        Driving,
        DwellingAfterDrive,
        Recovery
    }

    private StrokeState currentState;
    private StrokeState strokeState;

    private void UserInput()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            strokeState = StrokeState.WaitingForWheelToReachMinSpeed;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            strokeState = StrokeState.Driving;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            strokeState = StrokeState.DwellingAfterDrive;
        }
        else if (Input.GetKey(KeyCode.R))
        {
            strokeState = StrokeState.Recovery;
        }
        else if (Input.GetKey(KeyCode.T))
        {
            strokeState = StrokeState.WaitingForWheelToAccelerate;
        }
        
        StateChanged();
    }

    private void StateChanged()
    {
        // Update current state
        currentState = strokeState;

        // Update animation
        foreach (Animator animator in rowingAnimators)
        {
            animator.SetInteger("State", (int) strokeState);
        }
    }

    private float velocity = 0;

    private float prevSpeed = 0;
    private float deltSpeed = 0;
    private float speed = 0;
    private float power = 0;

    private float currTime = 0;
    private void UpdateSpeed()
    {

    #if UNITY_EDITOR

        // Increase time
        currTime += Time.fixedDeltaTime;

        // Simlating concpet2 update speed
        if (currTime > 0.5f)
        {
            speed = (Random.Range(1.75f, 2.25f));
            currTime = 0;
        }

        if (move)
        {
            // Driving
            strokeState = StrokeState.Driving;
        }
        else
        {
            // Resting
            strokeState = 0;
        }

#else

        // get speed from erg
        speed = stats.GetSpeed();

        // get power from erg
        power = stats.GetStrokePower();

        // get stroke state from erg
        strokeState = (StrokeState) stats.GetStrokeState();

#endif

        //// If driving
        if (strokeState == StrokeState.Driving)
        {
            // Apply force
            rigidbody.AddForce(transform.forward * speed * Time.fixedDeltaTime);
        }

        // Upidate velocity
        velocity = rigidbody.velocity.magnitude * boatSpeed;
    }

    private void Animate()
    {
        // Update animation
        foreach (Animator animator in rowingAnimators)
        {
            animator.SetInteger("State", (int) strokeState);
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

    public void Pause()
    {
        this.paused = true;
    }

    public void Resume()
    {
        this.paused = false;
    }

    public bool Paused()
    {
        return this.paused;
    }

    public float GetVelocity()
    {
        return velocity;
    }

    public void ChangeCameraView()
    {
        cameraIndex = (cameraIndex < cameras.Length - 1) ? cameraIndex + 1 : 0;

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == cameraIndex);
        }
    }
}
