using UnityStandardAssets.Utility;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class PlayerController : MonoBehaviour
{
    // TODO: Extract into external APPDATA file
    private static readonly int CULL_HIDDEN = 15;
    private static readonly int CULL_VISIBLE = 16;

    private static Camera playerCamera;

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
            UpdateOffset();
            DisableCameras();
            AssignBillboard();
            DestroyWaypointTracker();
            UpdateMinimapIcon();
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

    private void AssignMenuCamera()
    {
        // Assign camera
        playerCamera = mainCamera;

        // Update menu camera
        MenuManager.Instance.GetComponentInParent<Canvas>().worldCamera = playerCamera;
        
        // Display HUD
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
        // Update canvas to display users nickname
        playerTag.GetComponentInChildren<TMP_Text>().text = nickname;
    }

    private int playerCount = 0;
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

    private void DisableCameras()
    {
        Camera[] cameras = GetComponentsInChildren<Camera>();

        foreach (Camera camera in cameras)
        {
            camera.gameObject.SetActive(false);
        }
    }

    private void DestroyWaypointTracker()
    {
        Destroy(GetComponent<RouteFollower>());
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

    private void UpdateLayers()
    {
        SetLayerRecursively(rower, CULL_VISIBLE);
        SetLayerRecursively(playerTag, CULL_VISIBLE);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }

            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private void AssignBillboard()
    {
        playerTag.GetComponent<Billboard>().camTransform = playerCamera.transform;
        playerTag.GetComponent<Canvas>().worldCamera = playerCamera;
    }

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

    public float GetPlayerProgress()
    {
        return routeFollower.progressAlongRoute;
    }

    public void ReduceVelocity()
    {
        rigidBody.velocity = Vector3.zero;
    }

    public void ChangeCameraPosition()
    {
        cameraIndex = (cameraIndex < cameraPositions.Length - 1) ? cameraIndex + 1 : 0;

        SetLayerRecursively(rower, (cameraIndex != 0) ? CULL_VISIBLE : CULL_HIDDEN);

        for (int i = 0; i < cameraPositions.Length; i++)
        {
            mainCamera.transform.SetPositionAndRotation(cameraPositions[cameraIndex].transform.position, cameraPositions[cameraIndex].transform.rotation);
            cullCamera.transform.SetPositionAndRotation(cameraPositions[cameraIndex].transform.position, cameraPositions[cameraIndex].transform.rotation);
        }
    }

    public void UpdateRace(Race race)
    {
        this.race = race;
    }

    public void UpdateTrial(Trial trial)
    {
        this.trial = trial;
    }
}
