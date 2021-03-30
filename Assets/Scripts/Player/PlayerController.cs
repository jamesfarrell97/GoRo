using UnityStandardAssets.Utility;
using UnityEngine.UI;
using UnityEngine;

using Photon.Pun;
using TMPro;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class PlayerController : MonoBehaviour
{
    // TODO: Extract into external APPDATA file
    private static readonly int ROWER_LAYER = 14;
    private static readonly int CULL_HIDDEN_LAYER = 15;
    private static readonly int CULL_VISIBLE_LAYER = 16;
    
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

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera cullCamera;
    [SerializeField] private Transform[] cameraPositions;

    [SerializeField] private GameObject rower;
    [SerializeField] private GameObject localBoat;
    [SerializeField] private GameObject networkedBoat;
    [SerializeField] private GameObject minimapIcon;
    [SerializeField] private GameObject playerTag;

    [HideInInspector] public Trial trial;
    [HideInInspector] public Race race;

    [HideInInspector] public Slider progressBar;

    [HideInInspector] public PhotonView photonView { get; private set; }
    [HideInInspector] public PlayerState state;

    private Rigidbody rigidBody;
    private BoxCollider boxCollider;

    private AchievementTracker achievementTracker;
    private RouteFollower routeFollower;
    
    private bool paused = false;
    private bool move = false;

    private int cameraIndex = 0;

    private float rowingSpeed = 0;
    private float playerVelocity = 0;

    private int playerCount = 0;
    private float progressAlongRoute;

    private void Awake()
    {
        achievementTracker = GetComponent<AchievementTracker>();
        routeFollower = GetComponent<RouteFollower>();
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            AssignMenuCamera();
            AssignRigidbody();
            UpdatePlayerTag();
            UpdateBoat();
        }
        else
        {
            DisableCameras();
            AssignBillboard();
            UpdateMinimapIcon();
            UpdateOffset();
            UpdateLayers();
            UpdateBoat();
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        if (paused)
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

    #region Local Player

    private void AssignMenuCamera()
    {
        MenuManager.Instance.GetComponentInParent<Canvas>().worldCamera = mainCamera;
        
        MenuManager.Instance.OpenMenu("HUD");
    }

    private void AssignRigidbody()
    {
        rigidBody = GameObject.Find("Rigidbody").GetComponent<Rigidbody>();
    }

    private void UpdatePlayerTag()
    {
        photonView.RPC("RPC_UpdatePlayerTag", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName);
    }

    [PunRPC]
    private void RPC_UpdatePlayerTag(string nickname)
    {
        playerTag.GetComponentInChildren<TMP_Text>().text = nickname;
    }

    #endregion

    #region Networked Player

    private void DisableCameras()
    {
        Camera[] cameras = GetComponentsInChildren<Camera>();

        foreach (Camera camera in cameras)
        {
            camera.gameObject.SetActive(false);
        }
    }

    private void AssignBillboard()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            if (!player.photonView.IsMine) continue;

            playerTag.GetComponent<Billboard>().camTransform = player.mainCamera.transform;
            playerTag.GetComponent<Canvas>().worldCamera = player.mainCamera;

            break;
        }
    }

    private void UpdateMinimapIcon()
    {
        // Change minimap icon colour
        foreach (SpriteRenderer spriteRenderer in minimapIcon.GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.color = Color.red;
        }

        // Place higher than our icon in minimap
        minimapIcon.transform.Translate(new Vector3(0, 10f, 0));
    }

    private void UpdateOffset()
    {
        // Update player count
        playerCount++;

        // Calculate offset
        Vector3 offset = ((playerCount % 2 == 0) ? Vector3.left * (playerCount * 5) : Vector3.right * (playerCount * 5));

        // Update positions
        minimapIcon.transform.position += offset;
        networkedBoat.transform.position += offset;
        playerTag.transform.position += offset;
    }


    private void UpdateLayers()
    {
        HelperFunctions.SetLayerRecursively(rower, CULL_VISIBLE_LAYER);
        HelperFunctions.SetLayerRecursively(playerTag, CULL_VISIBLE_LAYER);
    }

    private void UpdateBoat()
    {
        if (photonView.IsMine)
        {
            Destroy(networkedBoat);
        }
        else
        {
            Destroy(localBoat);
        }
    }

    public void UpdateProgress(float progressAlongRoute)
    {
        photonView.RPC("RPC_UpdateProgress", RpcTarget.AllBuffered, progressAlongRoute);
    }

    [PunRPC]
    public void RPC_UpdateProgress(float progressAlongRoute)
    {
        this.progressAlongRoute = progressAlongRoute;
    }

    #endregion

    private void UpdateSpeed()
    {

#if UNITY_EDITOR
        
        // If debug 'move' button pressed
        if (move || Input.GetKey(KeyCode.W))
        {
            // Apply force to the rigibody
            rigidBody.AddForce(transform.forward * 5 * Time.fixedDeltaTime);
            
            // Update stroke state
            strokeState = StrokeState.Driving;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            // Update stroke state
            strokeState = StrokeState.Recovery;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            // Update stroke state
            strokeState = StrokeState.WaitingForWheelToAccelerate;
        }

#else

        // Get speed from erg
        rowingSpeed = StatsManager.Instance.GetSpeed();

        // Get stroke state from erg
        strokeState = (StrokeState) StatsManager.Instance.GetStrokeState();

        // If the user is currently driving
        if (strokeState == StrokeState.Driving)
        {
            // Apply force to the rigidbody
            rigidBody.AddForce(transform.forward * rowingSpeed * Time.fixedDeltaTime);
        }

#endif

        // Calculate velocity based on rigibody current speed
        playerVelocity = rigidBody.velocity.magnitude * boatSpeed;
        
        // Update velocity
        routeFollower.UpdateVelocity(playerVelocity);
    }

    private void Animate()
    {
        foreach (Animator animator in rowingAnimators)
        {
            animator.SetInteger("State", (int) strokeState);
        }
    }

    #region Accessors & Mutators

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

    public int GetCurrentLap()
    {
        return routeFollower.currentLap;
    }

    public float GetProgress()
    {
        return progressAlongRoute;
    }

    public void ReduceVelocity()
    {
        rigidBody.velocity = Vector3.zero;
    }

    public void UpdateRace(Race race)
    {
        this.race = race;
    }

    public void UpdateTrial(Trial trial)
    {
        this.trial = trial;
    }

    #endregion

    public void ChangeCameraPosition()
    {
        cameraIndex = (cameraIndex < cameraPositions.Length - 1) ? cameraIndex + 1 : 0;

        HelperFunctions.SetLayerRecursively(rower, (cameraIndex != 0) ? ROWER_LAYER : CULL_HIDDEN_LAYER);

        for (int i = 0; i < cameraPositions.Length; i++)
        {
            mainCamera.transform.SetPositionAndRotation(cameraPositions[cameraIndex].transform.position, cameraPositions[cameraIndex].transform.rotation);
            cullCamera.transform.SetPositionAndRotation(cameraPositions[cameraIndex].transform.position, cameraPositions[cameraIndex].transform.rotation);
        }
    }
}
