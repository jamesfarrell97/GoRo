using System.Collections.Generic;

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

    private AchievementTracker achievementTracker;
    private RouteFollower routeFollower;
    
    private bool paused = false;
    private bool move = false;

    private int cameraIndex = 0;

    private float rowingSpeed = 0;
    private float playerVelocity = 0;

    public List<float> DistanceSample { get; private set; }
    public List<float> PowerSample { get; private set; }
    public List<float> StrokeRateSample { get; private set; }
    public List<float> PaceSample { get; private set; }
    public List<float> SpeedSample { get; private set; }

    private int playerCount = 0;

    private void Awake()
    {
        achievementTracker = GetComponent<AchievementTracker>();
        routeFollower = GetComponent<RouteFollower>();
        photonView = GetComponent<PhotonView>();

        StatsManager.Instance.SetPlayerController(this);
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            AssignMenuCamera();
            UpdatePlayerTag();
            UpdateBoat();
        }
        else
        {
            UpdateBoat();
            UpdateLayers();
            DisableCameras();
            AssignBillboard();
            UpdateMinimapIcon();
            UpdateOffset();
        }

        ResetSamples();
        UpdatePosition();

        InvokeRepeating("UpdateMovement", 0f, (1f / StatsManager.MOVE_SAMPLE_RATE));
    }

    private void UpdatePosition()
    {
        routeFollower.SetPosition();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        if (!paused)
        {
            Animate();
        }
    }

    #region Local Player

    private void AssignMenuCamera()
    {
        MenuManager.Instance.GetComponentInParent<Canvas>().worldCamera = mainCamera;

        MenuManager.Instance.OpenMenu("HUD");
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

            playerTag.GetComponent<Billboard>().camera = player.cullCamera.transform;
            playerTag.GetComponent<Canvas>().worldCamera = player.cullCamera;

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
        Vector3 offset = ((playerCount % 2 == 0) ? -transform.right * (playerCount * 5) : transform.right * (playerCount * 5));

        // Update offsets
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

    public void UpdateRouteDistance(float routeDistance)
    {
        photonView.RPC("RPC_UpdateRouteDistance", RpcTarget.AllBuffered, routeDistance);
    }

    [PunRPC]
    public void RPC_UpdateRouteDistance(float routeDistance)
    {
        this.routeDistance = routeDistance;
    }

    #endregion

    public float routeDistance = 0;

    public void SampleStats()
    {

#if UNITY_EDITOR

        // Sample distance
        DistanceSample.Add(routeDistance);

#else

        // Sample distance
        DistanceSample.Add(StatsManager.Instance.GetDistance());

        // Sample stroke power
        PowerSample.Add(StatsManager.Instance.GetStrokePower());

        // Sample stroke rate
        StrokeRateSample.Add(StatsManager.Instance.GetStrokeRate());

        // Sample pace
        PaceSample.Add(StatsManager.Instance.GetPace());

        // Sample speed
        SpeedSample.Add(StatsManager.Instance.GetSpeed());

#endif

    }

    public void ResetSamples()
    {
        DistanceSample = new List<float>();
        PowerSample = new List<float>();
        StrokeRateSample = new List<float>();
        PaceSample = new List<float>();
        SpeedSample = new List<float>();
    }

    private void UpdateMovement()
    {

#if UNITY_EDITOR

        // If debug 'move' button pressed
        if (move || Input.GetKey(KeyCode.W))
        {
            float x = Random.Range(15f, 20f);

            // Update distance
            routeDistance += x;

            // Update stroke state
            strokeState = StrokeState.Driving;

            // Update distance
            routeFollower.UpdateDistance(routeDistance);

            // Update debug display
            StatsManager.Instance.SetDebugDisplay(routeFollower.progressAlongRoute.ToString());

            // Sample stats
            SampleStats();
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

#endif

    }

    public float range = 0;
    private float delta;

    public void ERGUpdateDistance(float distance)
    {
        // Don't execute if paused
        if (paused) return;

        // Update distance
        routeDistance = distance;

        // Update distance
        routeFollower.UpdateDistance(routeDistance);

        //// Update debug display
        //StatsManager.Instance.SetDebugDisplay(routeFollower.progressAlongRoute.ToString());
    }

    private void Animate()
    {

#if UNITY_EDITOR

        foreach (Animator animator in rowingAnimators)
        {
            animator.SetInteger("State", (int) strokeState);
        }

#else

        foreach (Animator animator in rowingAnimators)
        {
            animator.SetInteger("State", StatsManager.Instance.GetStrokeState());
        }

#endif

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

    public float GetRouteDistance()
    {
        return routeDistance;
    }

    public void ResetProgress()
    {
        routeDistance = 0;
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
