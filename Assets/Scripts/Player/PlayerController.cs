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

    public enum StrokeState
    {
        WaitingForWheelToReachMinSpeed,
        WaitingForWheelToAccelerate,
        Driving,
        DwellingAfterDrive,
        Recovery
    }

    private StrokeState currentState;
    private StrokeState strokeState;

    [SerializeField] [Range(0, 3)] public float boatSpeed = 1f;

    [SerializeField] private Animator[] rowingAnimators;
    [SerializeField] private Material otherPlayerMaterial;
    [SerializeField] private Oar leftOar;
    [SerializeField] private Oar rightOar;

    [SerializeField] private Camera[] cameras;

    [HideInInspector] public Trial trial;
    [HideInInspector] public Race race;

    [HideInInspector] public PhotonView photonView { get; private set; }
    [HideInInspector] public PlayerState state;

    private Rigidbody rigidbody;
    private BoxCollider boxCollider;

    private AchievementTracker achievementTracker;
    private RouteFollower routeFollower;
    private StatsManager stats;

    private bool paused = false;
    private bool move = false;

    private int cameraIndex = 0;

    private float rowingSpeed = 0;
    private float playerVelocity = 0;

    private void Awake()
    {
        stats = GameManager.Instance.GetComponent<StatsManager>();

        achievementTracker = GetComponent<AchievementTracker>();
        routeFollower = GetComponent<RouteFollower>();
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
            AssignRigidbody();
        }
        else
        {
            DestroyComponents();
            UpdateAppearance();
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        achievementTracker.TrackAchievements(photonView);

        if (!paused)
        {
            if (PhotonNetwork.OfflineMode)
            {
                routeFollower.UpdateVelocity(0);
            }
        }
        else
        {
            UpdateSpeed();
            Animate();
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

    private void AssignRigidbody()
    {
        // Retrieve rigidbody from scene
        rigidbody = GameObject.Find("Rigidbody").GetComponent<Rigidbody>();
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

    private void UpdateSpeed()
    {
        // Get speed from erg
        rowingSpeed = stats.GetSpeed();

        // Get stroke state from erg
        strokeState = (StrokeState) stats.GetStrokeState();

        // If the user is currently driving
        if (strokeState == StrokeState.Driving)
        {
            // Apply force to rigidbody
            rigidbody.AddForce(transform.forward * rowingSpeed * Time.fixedDeltaTime);
        }

        // Upidate velocity based on rigibody current speed
        routeFollower.UpdateVelocity(rigidbody.velocity.magnitude * boatSpeed);
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
        return playerVelocity;
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
