using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityStandardAssets.Utility;
using System.Collections;
using UnityEngine.SceneManagement;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class PlayerController : MonoBehaviour
{
    [SerializeField] Boat boat;
    [SerializeField] public float boatSpeed = 0f;
    [SerializeField] float boatTurningSpeed = 1f;
    [SerializeField] public bool participatingInRace = false;
    [SerializeField] public bool participatingInTimeTrial = false;
    [SerializeField] public Transform[] route;

    [SerializeField] Animator[] rowingAnimators;
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

    private BoxCollider boxCollider;
    private Rigidbody rigidBody;

    private PhotonView photonView;
    private AchievementTracker achievementTracker;
    private Stats stats;

    private bool allowedMove = true;
    private bool moveForward = false;
    private bool moveBack = false;
    private bool moveLeft = false;
    private bool moveRight = false;
    private bool rotateLeft = false;
    private bool rotateRight = false;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        rigidBody = GetComponent<Rigidbody>();

        photonView = GetComponent<PhotonView>();
        achievementTracker = GetComponent<AchievementTracker>();
        stats = GetComponent<Stats>();
        waitingToConfirmLeaveRoom = false;
        waitingToConfirmExitGame = false;

        launcher = FindObjectOfType<Launcher>();
    }

    private void Start()
    {
        //GetComponent<WaypointProgressTracker>().Circuit = FindObjectOfType<WaypointCircuit>();

        if (photonView.IsMine) return;

        previousPosition = transform.position;

        Destroy(GetComponentInChildren<Camera>().gameObject);
        Destroy(rigidBody);
    }

    int nodeIndex = 0;
    Transform currentNode;

    private float currDist = 0;
    private float prevDist = 0;
    private float currTime = 0;
    private float prevTime = 0;
    private float deltDist = 0;
    private float deltTime = 0;
    private float velocity = 0;

    private float distance = 0;
    private float angle = 0;
    private float prevAngl = 0;
    private float deltaDistance = 0;
    private float deltAngl = 0;
    private float time = 0;
    private float speed = 0;

    private Vector3 previousPosition;
    private Vector3 cross;

    private const float MIN_DISTANCE = 1f;

    Vector3 force;
    int count = 0;
    private void Update()
    {
        if (!photonView.IsMine) return;

        achievementTracker.TrackAchievements(photonView, stats);

        if (!allowedMove) return;

        // RowingStatusData[0] == TimeLo
        // RowingStatusData[3] == DistanceLo

        currDist = BluetoothManager.RowingStatusData[3] * 10;   // Convert to meters
        currTime = BluetoothManager.RowingStatusData[0] * 100;  // Convert to seconds

        deltDist = currDist - prevDist;
        deltTime = Time.time - prevTime;

        if (deltTime != 0)
        {
            velocity = deltDist / deltTime;

            force = -transform.forward * boatSpeed * Mathf.Abs(velocity / 100);

            rigidBody.AddForce(force);

            if (count > 50)
            {
                Debug.Log("DUNITY: CurrDist: " + currDist);
                Debug.Log("DUNITY: CurrTime: " + currTime);
                Debug.Log("DUNITY: DeltDist: " + deltDist);
                Debug.Log("DUNITY: DeltTime: " + deltTime);
                Debug.Log("DUNITY: Velocity: " + velocity);
                Debug.Log("DUNITY: Force: " + force);
                count = 0;

            }

            count++;
        }

        prevDist = currDist;
        prevTime = currTime;

        foreach (Animator animator in rowingAnimators)
        {
            if (deltDist > 0)
            {
                animator.SetBool("Play", true);
            }
            else
            {
                animator.SetBool("Play", false);
            }
        }

        CalculateVelocity();
    }

    #region UI Interactions
    public IEnumerator DisplayQuickNotificationText(string text, int duration)
    {
        notificationTextPanel.SetActive(true);
        notificationText.GetComponent<Text>().text = text;
        yield return new WaitForSeconds(duration);
        notificationTextPanel.SetActive(false);
    }

    //Call in when you wish to keep text on screen infinitely, OR
    //If the text will be overwritten later on, such as in Race->UpdateStopWatch();
    public void DisplayNotificationText(string text)
    {
        notificationTextPanel.SetActive(true);
        notificationText.GetComponent<Text>().text = text;
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
        if(PhotonNetwork.OfflineMode == true)
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
                //launcher.ShowConnectionMenu();                
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
            //    Application.Quit();
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
        if(participatingInRace == true)
        {
            boat.GetComponent<WaypointProgressTracker>().currentRace.GetComponent<Race>().PauseSingleplayerRace();
        }
        else if(participatingInTimeTrial == true)
        {
            boat.GetComponent<WaypointProgressTracker>().currentTimeTrial.GetComponent<TimeTrial>().PauseSingleplayerTimeTrial();
        }
    }

    private void UnpauseGame()
    {
        if (participatingInRace == true)
        {
            boat.GetComponent<WaypointProgressTracker>().currentRace.GetComponent<Race>().UnpauseSingleplayerRace();
        }
        else if (participatingInTimeTrial == true)
        {
            boat.GetComponent<WaypointProgressTracker>().currentTimeTrial.GetComponent<TimeTrial>().UnpauseSingleplayerTimeTrial();
        }
    }

    #region Race/Time Trial Event Methods
    public void StartARace()
    {
        if (participatingInTimeTrial == false && participatingInRace == false)
        {
            CloseGameMenu();
            participatingInRace = true;
            speedSlider.SetActive(true);
            GameObject.Find("Race Manager").GetComponent<RaceManager>().AddPlayerToRace(boat);
        }
    }

    public void StartATimeTrial()
    {
        if (participatingInRace == false && participatingInTimeTrial == false)
        {
            CloseGameMenu();
            participatingInTimeTrial = true;
            speedSlider.SetActive(true);
            GameObject.Find("Time Trial Manager").GetComponent<TimeTrialManager>().AddPlayerToTimeTrial(boat);
        }
    }
    #endregion

    #endregion

    public void CalculateVelocity()
    {
        //currDist = BluetoothManager.RowingStatusData[3] * 10;   // Convert to meters
        //currTime = BluetoothManager.RowingStatusData[0] * 100;  // Convert to seconds

        //deltDist = currDist - prevDist;
        //deltTime = currTime - prevTime;

        //velocity = deltDist / deltTime;

        //rigidBody.AddForce(-transform.forward * boatSpeed * Mathf.Abs(velocity / 100));

        //prevDist = currDist;
        //prevTime = currTime;

        if (Input.GetKey(KeyCode.W) || moveForward)
        {
            speed = boatSpeed;
            transform.position += transform.forward * speed;
        }
        else if (Input.GetKey(KeyCode.S) || moveBack)
        {
            speed = -boatSpeed;
            transform.position += transform.forward * speed;
        }
    }

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag.Equals("Obstacle"))
        {
            FindObjectOfType<AudioManager>().Play("Collision");
        }
    }
}
