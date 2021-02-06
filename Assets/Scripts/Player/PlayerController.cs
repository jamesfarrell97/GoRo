using System.Collections;

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
    [SerializeField] public float boatSpeed = 0f;
    [SerializeField] float boatTurningSpeed = 1f;
    [SerializeField] public bool participatingInRace = false;
    [SerializeField] public bool participatingInTimeTrial = false;
    [SerializeField] public Transform[] route;

    [SerializeField] Animator[] rowingAnimators;
    [SerializeField] [Range(0f, 1f)] float otherPlayerTransparency = 0.5f;
    [SerializeField] Material otherPlayerMaterial;

    #region UI Variables
    [SerializeField] public GameObject hudCanvas;
    [SerializeField] public GameObject gameMenuCanvas;
    [SerializeField] public GameObject confirmationCanvas;
    [SerializeField] public GameObject speedSlider;
    [SerializeField] public GameObject notificationTextPanel;
    [SerializeField] public GameObject notificationText;
    [SerializeField] public GameObject timePanel;
    [SerializeField] public GameObject timeText;
    [SerializeField] public GameObject lapPanel;
    [SerializeField] public GameObject lapText;
    [SerializeField] public GameObject countdownPanel;
    [SerializeField] public GameObject countdownText;
    [SerializeField] public GameObject confirmationText;

    private bool waitingToConfirmLeaveRoom;
    private bool waitingToConfirmExitGame;
    private Launcher launcher;
    #endregion

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
        waitingToConfirmLeaveRoom = false;
        waitingToConfirmExitGame = false;

        launcher = FindObjectOfType<Launcher>();
    }

    private void Start()
    {
        if (photonView.IsMine) return;

        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach (Camera c in cameras)
        {
            c.gameObject.SetActive(false);
        }

        Canvas[] canvas = GetComponentsInParent<Canvas>();
        foreach (Canvas c in canvas)
        {
            Destroy(c);
        }

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
                else if (renderer.gameObject.name.Contains("Mask"))
                {
                    Destroy(renderer.transform.parent.gameObject.GetComponent<SetRenderQueue>());
                    Destroy(renderer.gameObject);
                }
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

    private float currDist = 0;
    private float prevDist = 0;
    private float deltDist = 0;

    private float currTime = 0;
    private float prevTime = 0;
    private float deltTime = 0;

    private void UpdateSpeed()
    {
        //#if UNITY_EDITOR
        //        if (moveForward)
        //        {
        //            currDist += 3 * Time.fixedDeltaTime; //     3 meters
        //            currTime += 1 * Time.fixedDeltaTime; // per 1 second
        //        }
        //#else
        //        currDist = stats.GetMetersRowed();
        //        currTime = stats.GetSecondsRowing();
        //#endif

        //        deltDist = currDist - prevDist;
        //        deltTime = currTime - prevTime;

        //        if (deltTime <= 0) deltTime = Time.fixedDeltaTime;

        //        speed = ((deltDist) / (deltTime) * Time.fixedDeltaTime);

        //        prevDist = currDist;
        //        prevTime = currTime;

        if (moveForward)
        {
            currDist += 2 * Time.fixedDeltaTime; //     3 meters
            currTime += 1 * Time.fixedDeltaTime; // per 1 second
        }

        //currDist += 2 * Time.fixedDeltaTime; //     3 meters
        //currTime += 1 * Time.fixedDeltaTime; // per 1 second

        //currDist = stats.GetMetersRowed();
        //currTime = stats.GetSecondsRowing();

        deltDist = currDist - prevDist;
        deltTime = currTime - prevTime;

        if (deltTime <= 0) deltTime = Time.fixedDeltaTime;

        speed = (deltDist) / (deltTime);

        prevDist = currDist;
        prevTime = currTime;
    }

    private float velocity;
    private void CalculateVelocity()
    {
        //if (velocity < 0)
        //{
        //    velocity = 0;
        //}
        //else if (speed == 0)
        //{
        //    velocity -= speedDecayFactor;
        //}
        //else
        //{
        //    velocity = speed;
        //}

        // Decay velocity over time if no speed applied
        if (speed <= 0)
        {
            velocity -= speedDecayFactor * Time.fixedDeltaTime;
        }

        // Increase velocity over time if speed applied
        else
        {
            velocity += speedIncreaseFactor * Time.fixedDeltaTime;
        }

        // Min speed
        if (velocity < 0)
        {
            velocity = 0;
        }

        // Max speed
        else if (speed != 0 && velocity > speed)
        {
            velocity = speed;
        }

        // speed is measured in meters per second and this function is ran 
        // fixedDeltaTime's per second - so, we can figure out how far to 
        // move this update, if we evenly spread the speed out across the 
        // second by multiplying it by fixedDeltaTime

        //velocity = speed * Time.deltaTime;
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

#region UI Interactions
    public IEnumerator DisplayQuickNotificationText(string text, int duration)
    {
        notificationTextPanel.SetActive(true);
        notificationText.GetComponent<TMP_Text>().text = text;
        yield return new WaitForSeconds(duration);
        notificationTextPanel.SetActive(false);
    }

    //Call in when you wish to keep text on screen infinitely, OR
    //If the text will be overwritten later on, such as in Race->UpdateStopWatch();
    public void DisplayNotificationText(string text)
    {
        notificationTextPanel.SetActive(true);
        notificationText.GetComponent<TMP_Text>().text = text;
    }

    public void DisplayTimeAndLap(string time, string lap)
    {
        timePanel.SetActive(true);
        timeText.GetComponent<Text>().text = time;

        lapPanel.SetActive(true);
        lapText.GetComponent<Text>().text = lap;
    }

    public IEnumerator DisplayCountdown(string time, int duration)
    {
        countdownPanel.SetActive(true);
        countdownText.GetComponent<Text>().text = time;
        yield return new WaitForSeconds(duration);
        countdownPanel.SetActive(false);
    }
#endregion

#region Game Menu Interactions
    public void OpenGameMenu()
    {
        if (PhotonNetwork.OfflineMode == true)
        {
            PauseGame();
        }

        hudCanvas.SetActive(false);
        gameMenuCanvas.SetActive(true);
    }

    public void CloseGameMenu()
    {
        if (PhotonNetwork.OfflineMode == true)
        {
            UnpauseGame();
        }

        gameMenuCanvas.SetActive(false);
        hudCanvas.SetActive(true);
    }

    public void ConfirmPressed()
    {
        ConfirmChoice(true);
    }

    public void CancelPressed()
    {
        ConfirmChoice(false);
    }

    private void ConfirmChoice(bool confirmPressed)
    {
        if (confirmPressed == true)
        {
            if (waitingToConfirmLeaveRoom == true)
            {
                launcher.LeaveRoom();
            }
            else if (waitingToConfirmExitGame == true)
            {
                Application.Quit();
            }
        }
        else
        {
            waitingToConfirmLeaveRoom = false;
            waitingToConfirmExitGame = false;
            confirmationCanvas.SetActive(false);
        }
    }

    public void LeaveRoom()
    {
        if(photonView.IsMine)
        {
            RequestConfirmPlayersChoice("leaveRoom");
        }
    }

    public void ExitGame()
    {
        if (photonView.IsMine)
        {
            RequestConfirmPlayersChoice("exitGame");
        }
    }

    private void RequestConfirmPlayersChoice(string eventBeingConfirmed)
    {
        confirmationCanvas.SetActive(true);

        if(eventBeingConfirmed == "leaveRoom")
        {
            confirmationText.GetComponent<Text>().text = "Are you sure you wish to leave this lobby?";
            waitingToConfirmLeaveRoom = true;
        }
        else if(eventBeingConfirmed == "exitGame")
        {
            confirmationText.GetComponent<Text>().text = "Are you sure you wish to exit the game?";
            waitingToConfirmExitGame = true;
        }
    }

    private void PauseGame()
    {
        if (participatingInRace == true)
        {
            GetComponent<WaypointProgressTracker>().currentRace.GetComponent<Race>().PauseSingleplayerRace();
        }
        else if(participatingInTimeTrial == true)
        {
            GetComponent<WaypointProgressTracker>().currentTimeTrial.GetComponent<TimeTrial>().PauseSingleplayerTimeTrial();
        }
    }

    private void UnpauseGame()
    {
        if (participatingInRace == true)
        {
            GetComponent<WaypointProgressTracker>().currentRace.GetComponent<Race>().UnpauseSingleplayerRace();
        }
        else if (participatingInTimeTrial == true)
        {
            GetComponent<WaypointProgressTracker>().currentTimeTrial.GetComponent<TimeTrial>().UnpauseSingleplayerTimeTrial();
        }
    }
#endregion

#region Race/Time Trial Event Methods
    public void StartARace()
    {
        // Multiplayer race not yet supported
        if (!PhotonNetwork.OfflineMode) return;

        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            // Skip to next player if not our photon view
            if (!player.photonView.IsMine) continue;

            if (participatingInTimeTrial == false && participatingInRace == false)
            {
                CloseGameMenu();
                participatingInRace = true;
            
                // Reset track distance
                GetComponent<WaypointProgressTracker>().progressDistance = 0;

                GameObject.Find("Race Manager").GetComponent<RaceManager>().AddPlayerToRace(player);
            }

            // Found our view, so return
            return;
        }
    }

    public void StartATimeTrial()
    {   
        // Multiplayer time-trial not yet supported
        if (!PhotonNetwork.OfflineMode) return;

        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            // Skip to next player if not our photon view
            if (!player.photonView.IsMine) continue;

            if (participatingInRace == false && participatingInTimeTrial == false)
            {
                CloseGameMenu();
                participatingInTimeTrial = true;

                // Reset track distance
                GetComponent<WaypointProgressTracker>().progressDistance = 0;

                GameObject.Find("Time Trial Manager").GetComponent<TimeTrialManager>().AddPlayerToTimeTrial(player);
            }

            // Found our view, so return
            return;
        }
    }

    public void JustRow()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            // Skip to next player if not our photon view
            if (!player.photonView.IsMine) continue;

            WaypointProgressTracker waypointProgressTracker = GetComponent<WaypointProgressTracker>();

            // Reset track distance
            waypointProgressTracker.progressDistance = 0;

            // Reset track
            waypointProgressTracker.Circuit = waypointProgressTracker.Routes[0];

            // Found our view, so return
            return;
        }
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
