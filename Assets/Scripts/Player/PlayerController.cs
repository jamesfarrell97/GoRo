using UnityStandardAssets.Utility;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class PlayerController : MonoBehaviour
{
    // TODO: Extract into external APPDATA file
    private static readonly int HIDDEN_ROWER_LAYER = 15;
    private static readonly int VISIBLE_ROWER_LAYER = 16;

    private static readonly int HIDDEN_PLAYERTAG_LAYER = 17;
    private static readonly int VISIBLE_PLAYERTAG_LAYER = 18;

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

    [SerializeField] private Camera camera;
    [SerializeField] private Transform[] cameraPositions;

    [SerializeField] private GameObject rowerSkin;
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

    public List<float> DistanceSample { get; private set; }
    public List<float> PowerSample { get; private set; }
    public List<float> StrokeRateSample { get; private set; }
    public List<float> AvgSplitSample { get; private set; }
    public List<float> SpeedSample { get; private set; }

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
            UpdateCanva();
        }
        else
        {
            DisableCameras();
            DestroyWaypointTracker();
            UpdateAppearance();
            UpdateLayers();
        }

        ResetSamples();

        InvokeRepeating("UpdateMovement", 0f, (1f / StatsManager.SAMPLE_RATE));
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        if (!paused)
        {
            Animate();
        }
    }

    private void AssignMenuCamera()
    {        
        // Update menu camera
        MenuManager.Instance.GetComponentInParent<Canvas>().worldCamera = camera;
        
        // Display HUD
        MenuManager.Instance.OpenMenu("HUD");
    }

    private void AssignRigidbody()
    {
        rigidBody = GameObject.Find("Rigidbody").GetComponent<Rigidbody>();
    }

    private void UpdateCanva()
    {
        photonView.RPC("RPC_UpdateCanva", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer.NickName);
    }

    [PunRPC]
    private void RPC_UpdateCanva(string nickname)
    {
        // Update canvas to display users nickname
        playerTag.GetComponentInChildren<TMP_Text>().text = nickname;
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

    private void UpdateLayers()
    {
        rowerSkin.gameObject.layer = VISIBLE_ROWER_LAYER; // Networked Rower

        playerTag.gameObject.layer = VISIBLE_PLAYERTAG_LAYER; // Networked Player Tag

        foreach (GameObject gameObject in playerTag.GetComponentsInChildren<GameObject>())
        {
            gameObject.layer = VISIBLE_PLAYERTAG_LAYER;
        }
    }

    public float routeDistance = 0;
    
    private void SampleStats()
    {
        // Sample distance
        DistanceSample.Add(routeDistance);

        // Sample stroke power
        PowerSample.Add(StatsManager.Instance.GetStrokePower());

        // Sample stroke rate
        StrokeRateSample.Add(StatsManager.Instance.GetStrokeRate());

        // Sample split avg
        AvgSplitSample.Add(StatsManager.Instance.GetAvgSplit());

        // Sample speed
        SpeedSample.Add(StatsManager.Instance.GetSpeed());
    }

    public void ResetSamples()
    {
        DistanceSample = new List<float>();
        PowerSample = new List<float>();
        StrokeRateSample = new List<float>();
        AvgSplitSample = new List<float>();
        SpeedSample = new List<float>();
    }

    private void UpdateMovement()
    {
        // Don't execute while paused
        if (paused) return;

#if UNITY_EDITOR

        // If debug 'move' button pressed
        if (move || Input.GetKey(KeyCode.W))
        {
            // Update distance
            routeDistance += boatSpeed;
            
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

        routeFollower.UpdateProgress(StatsManager.Instance.GetDistance());

        StatsManager.Instance.SetDebugDisplay(routeFollower.progressAlongRoute.ToString());

#endif

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

    public void ResetProgress()
    {
        routeDistance = 0;
    }

    public void ChangeCameraPosition()
    {
        cameraIndex = (cameraIndex < cameraPositions.Length - 1) ? cameraIndex + 1 : 0;

        rowerSkin.layer = (cameraIndex != 0) ? VISIBLE_ROWER_LAYER : HIDDEN_ROWER_LAYER;

        for (int i = 0; i < cameraPositions.Length; i++)
        {
            camera.transform.SetPositionAndRotation(cameraPositions[cameraIndex].transform.position, cameraPositions[cameraIndex].transform.rotation);
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
