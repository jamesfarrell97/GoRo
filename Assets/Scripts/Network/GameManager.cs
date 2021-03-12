using System.Collections.Generic;
using System.Collections;

using UnityStandardAssets.Utility;
using UnityEngine.SceneManagement;
using UnityEngine;

using Photon.Realtime;
using Photon.Pun;
using TMPro;

//Code referenced: https://www.youtube.com/watch?v=zPZK7C5_BQo
//
//
//

using static PlayerController;
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public enum GameState
    {
        Paused,
        Playing
    }

    public static GameState State;

    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;

    [SerializeField] GameObject startGameButton;
    [SerializeField] TMP_InputField roomNameInputField;

    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;

    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject playerListItemPrefab;

    [SerializeField] GameObject notificationTextPanel;
    [SerializeField] GameObject timePanel;
    [SerializeField] GameObject lapPanel;
    [SerializeField] GameObject countdownPanel;
    [SerializeField] GameObject confirmationText;

    private int buildIndex;
    private int singleplayerIndex = 1;
    private int multiplayerIndex = 1;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
        
        CheckConnection();
    }

    private void Start()
    {
        UpdateState();
        SetSleepTimeout();
        ShowConnectionMenu();
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void UpdateState()
    {
        State = GameState.Playing;
    }

    private void SetSleepTimeout()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public void ShowConnectionMenu()
    {
        if (Application.isEditor)
        {
            MenuManager.Instance.OpenMenu("Main");
        }
        else
        {
            MenuManager.Instance.OpenMenu("Connect");
        }
    }

    public void Singleplayer()
    {
        PhotonNetwork.OfflineMode = true;
        buildIndex = singleplayerIndex;

        CreateRoom();
        StartGame();
    }

    public void Multiplayer()
    {
        CheckConnection();

        if (PhotonNetwork.OfflineMode)
        {
            errorText.text = "Cannot connect to network.";
            MenuManager.Instance.OpenMenu("Error");
            return;
        }

        PhotonNetwork.OfflineMode = false;
        buildIndex = multiplayerIndex;

        Debug.Log("Connected to Server.");
        MenuManager.Instance.OpenMenu("Loading");

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void ConnectedToPerformanceMonitor()
    {
        MenuManager.Instance.OpenMenu("Main");
    }

    public void CheckConnection()
    {
        PhotonNetwork.OfflineMode = Application.internetReachability.Equals(NetworkReachability.NotReachable);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master.");

        if (PhotonNetwork.OfflineMode) return;

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("Multiplayer");
        PhotonNetwork.NickName = "Player" + UnityEngine.Random.Range(0, 1000).ToString("0000");
        Debug.Log("Joined Lobby");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(player);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform transform in roomListContent)
        {
            Destroy(transform.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;

            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(buildIndex);
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            PhotonNetwork.CreateRoom("Default");
            MenuManager.Instance.OpenMenu("Loading");
        }
        else
        {
            PhotonNetwork.CreateRoom(roomNameInputField.text);
            MenuManager.Instance.OpenMenu("Loading");
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "A room with that name already exists!";
        PhotonNetwork.Disconnect();
        PhotonNetwork.OfflineMode = true;
        MenuManager.Instance.OpenMenu("Error");
    }

    public void JoinRoom(RoomInfo roomInfo)
    {
        PhotonNetwork.JoinRoom(roomInfo.Name);
        MenuManager.Instance.OpenMenu("Loading");
    }

    public void Go()
    {        
        // Retrieve all players
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            // Retrieve player view
            PhotonView photonView = player.GetComponent<PhotonView>();

            // If not our view, skip rest of loop
            if (!photonView.IsMine) continue;

            // Go
            player.Go();

            // No need to check any more views, so return
            return;
        }
    }

    public void ToggleCameraView()
    {
        // Retrieve all players
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            // Retrieve player view
            PhotonView photonView = player.GetComponent<PhotonView>();

            // If not our view, skip rest of loop
            if (!photonView.IsMine) continue;

            // Change camera view
            player.GetComponent<PlayerController>().ChangeCameraView();

            // No need to check any more views, so return
            return;
        }
    }

    public void Stop()
    {
        // Retrieve all players
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            // Retrieve player view
            PhotonView photonView = player.GetComponent<PhotonView>();

            // If not our view, skip rest of loop
            if (!photonView.IsMine) continue;

            // Stop
            player.Stop();

            // No need to check any more views, so return
            return;
        }
    }

    public override void OnJoinedRoom()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1) return;
        
        MenuManager.Instance.OpenMenu("Room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("Loading");
    }

    public override void OnLeftRoom()
    {
        // TODO: Uncomment Later
        // MenuManager.Instance.OpenMenu("Main");
    }

    public void OpenMainMenu()
    {
        PhotonNetwork.Disconnect();
        MenuManager.Instance.OpenMenu("Main");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    #region Game Modes
    public void StartRace()
    {
        // Retrieve all players
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            // Retrieve player view
            PhotonView photonView = player.GetComponent<PhotonView>();

            // If not our view, skip rest of loop
            if (!photonView.IsMine) continue;

            // If player not currently participating in event
            if (player.state != PlayerState.ParticipatingInRace && player.state != PlayerState.ParticipatingInTrial)
            {
                // Add player to race
                GameObject.Find("Race Manager").GetComponent<RaceManager>().JoinRace(player, "Race Track");

                // Update player state
                player.state = PlayerState.ParticipatingInRace;

                // Reset track distance
                player.GetComponent<RouteFollower>().progressAlongRoute = 0;

                // Open HUD
                MenuManager.Instance.OpenMenu("HUD");
            }

            // No need to check any more views, so return
            return;
        }
    }

    public void StartTimeTrial()
    {
        // Retrieve all players
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            // Retrieve player view
            PhotonView photonView = player.GetComponent<PhotonView>();

            // If not our view, skip rest of loop
            if (!photonView.IsMine) continue;

            // If player not currently participating in event
            if (player.state != PlayerState.ParticipatingInRace && player.state != PlayerState.ParticipatingInTrial)
            {
                // Add player to time trial
                GameObject.Find("Time Trial Manager").GetComponent<TrialManager>().JoinTrial(player, "Race Track");

                // Update player state
                player.state = PlayerState.ParticipatingInTrial;

                // Reset track distance
                player.GetComponent<RouteFollower>().Reset();

                // Open HUD
                MenuManager.Instance.OpenMenu("HUD");
            }

            // No need to check any more views, so return
            return;
        }
    }

    public void StartJustRow()
    {
        // Retrieve all players
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            // Retrieve player view
            PhotonView photonView = player.GetComponent<PhotonView>();

            // If not our view, skip rest of loop
            if (!photonView.IsMine) continue;

            // Update player state
            player.state = PlayerState.JustRowing;

            // Clear player tracks
            player.trial = null;
            player.race = null;

            // Retrieve progress tracker
            RouteFollower routeFollower = player.GetComponent<RouteFollower>();

            // Reset track distance
            routeFollower.Reset();

            // Reset track
            routeFollower.route = routeFollower.routes[0];

            // Hide time and lap info
            HideTimeAndLap();

            // Open HUD
            MenuManager.Instance.OpenMenu("HUD");

            // No need to check any more views, so return
            return;
        }
    }
    #endregion

    #region HUD
    public void ConfirmLeaveRoom()
    {
        RequestConfirmPlayersChoice("leaveRoom");
    }

    public void ConfirmExitGame()
    {
        RequestConfirmPlayersChoice("exitGame");
    }

    private bool waitingToConfirmLeaveRoom;
    private bool waitingToConfirmExitGame;

    private void RequestConfirmPlayersChoice(string eventBeingConfirmed)
    {
        MenuManager.Instance.OpenMenu("Confirm");

        if (eventBeingConfirmed == "leaveRoom")
        {
            confirmationText.GetComponent<TMP_Text>().text = "Are you sure you wish to leave this lobby?";
            waitingToConfirmLeaveRoom = true;
        }
        else if (eventBeingConfirmed == "exitGame")
        {
            confirmationText.GetComponent<TMP_Text>().text = "Are you sure you wish to exit the game?";
            waitingToConfirmExitGame = true;
        }
    }

    public void Confirm()
    {
        ConfirmChoice(true);
    }

    public void Decline()
    {
        ConfirmChoice(false);
    }

    private void ConfirmChoice(bool confirmPressed)
    {
        if (confirmPressed == true)
        {
            if (waitingToConfirmLeaveRoom == true)
            {
                Instance.LeaveRoom();
            }
            else if (waitingToConfirmExitGame == true)
            {
                Application.Quit();
            }
        }
        else
        {
            MenuManager.Instance.OpenMenu("HUD");

            waitingToConfirmLeaveRoom = false;
            waitingToConfirmExitGame = false;
        }
    }

    public void PauseGame()
    {
        if (!PhotonNetwork.OfflineMode) return;

        // Retrieve all players
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        // Retrieve all ghosts
        GhostController[] ghosts = FindObjectsOfType<GhostController>();

        // For each loaded player
        foreach (PlayerController player in players)
        {
            // Retrieve player view
            PhotonView photonView = player.GetComponent<PhotonView>();

            // If not our view, skip rest of loop
            if (!photonView.IsMine) continue;

            // Pause movement
            player.Pause();

            // No need to check any more views, so return
            return;
        }

        // For each loaded ghost
        foreach (GhostController ghost in ghosts)
        {
            // Retrieve player view
            PhotonView photonView = ghost.GetComponent<PhotonView>();

            // If not our view, skip rest of loop
            if (!photonView.IsMine) continue;

            // Pause movement
            ghost.Pause();

            // No need to check any more views, so return
            return;
        }
    }

    public void UnpauseGame()
    {
        if (!PhotonNetwork.OfflineMode) return;

        // Retrieve all loaded players
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        // Retrieve all loaded ghosts
        GhostController[] ghosts = FindObjectsOfType<GhostController>();

        // For each loaded player
        foreach (PlayerController player in players)
        {
            // Retrieve player view
            PhotonView photonView = player.GetComponent<PhotonView>();

            // If not our view, skip rest of loop
            if (!photonView.IsMine) continue;

            // Resume movement
            player.Resume();

            // No need to check any more views, so return
            return;
        }

        // For each loaded ghost
        foreach (GhostController ghost in ghosts)
        {
            // Retrieve player view
            PhotonView photonView = ghost.GetComponent<PhotonView>();

            // If not our view, skip rest of loop
            if (!photonView.IsMine) continue;

            // Resume movement
            ghost.Resume();

            // No need to check any more views, so return
            return;
        }
    }

    public void DisplayTimeAndLap(string time, string lap)
    {
        timePanel.SetActive(true);
        timePanel.GetComponentInChildren<TMP_Text>().text = time;

        lapPanel.SetActive(true);
        lapPanel.GetComponentInChildren<TMP_Text>().text = lap;
    }

    public void HideTimeAndLap()
    {
        timePanel.SetActive(false);
        lapPanel.SetActive(false);
    }

    public void DisplayNotificationText(string text)
    {
        notificationTextPanel.SetActive(true);
        notificationTextPanel.GetComponentInChildren<TMP_Text>().text = text;
    }

    public IEnumerator DisplayQuickNotificationText(string text, int duration)
    {
        notificationTextPanel.SetActive(true);

        timePanel.SetActive(false);
        lapPanel.SetActive(false);

        notificationTextPanel.GetComponentInChildren<TMP_Text>().text = text;

        yield return new WaitForSeconds(duration);
        notificationTextPanel.SetActive(false);
    }

    public IEnumerator DisplayCountdown(string time, int duration)
    {
        countdownPanel.SetActive(true);
        countdownPanel.GetComponentInChildren<TMP_Text>().text = time;

        yield return new WaitForSeconds(duration);
        countdownPanel.SetActive(false);
    }
    #endregion
}