using System.Collections.Generic;
using System.Collections;

using UnityStandardAssets.Utility;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

using Photon.Realtime;
using Photon.Pun;
using TMPro;

//Code referenced: https://www.youtube.com/watch?v=zPZK7C5_BQo
//
//
//

using static PlayerController;
using static EventNotification;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public static string[] PLAYER_SETTINGS;

    private const int MENU_INDEX = 0;
    private const int GAME_INDEX = 1;

    public enum GameState
    {
        Paused,
        Playing
    }

    public static GameState State;
    
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] TMP_Text playerNameText;

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField playerNameInputField;

    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;

    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject playerListItemPrefab;

    [SerializeField] GameObject countdownPanel;
    [SerializeField] TMP_Text countdownText;

    [SerializeField] GameObject notificationPanel;
    [SerializeField] TMP_Text notificationText;

    [SerializeField] GameObject eventNotificationPanel;
    [SerializeField] GameObject eventInformationPanel;
    [SerializeField] GameObject eventPositionPanel;
    [SerializeField] TMP_Text eventTimeText;
    [SerializeField] TMP_Text eventLapText;
    [SerializeField] TMP_Text eventPositionText;

    [SerializeField] GameObject confirmationText;

    [SerializeField] GameObject startGameButton;
    [SerializeField] GameObject startRaceButton;
    [SerializeField] GameObject startTrialButton;
    [SerializeField] GameObject exitEventButton;
    [SerializeField] GameObject leaveRoomButton;

    [SerializeField] Button goButton;
    [SerializeField] Button toggleUIButton;

    [SerializeField] Slider localProgressBar;
    [SerializeField] Slider otherProgressBar;

    [SerializeField] GameObject muteLine;

    [SerializeField] GameObject[] panels;

    [SerializeField] GameObject audioManager;
    [SerializeField] GameObject roomManager;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
        
        InstantiateManagers();
        CheckConnection();
        CheckState();
    }

    private void CheckState()
    {
#if !UNITY_EDITOR
    
        goButton.transform.parent.gameObject.SetActive(false);

#endif
    }

    private void InstantiateManagers()
    {
        Instantiate(roomManager);
        Instantiate(audioManager);
    }

    private void Start()
    {
        UpdateState();
        SetSleepTimeout();
        ShowConnectionMenu();
        LoadPlayerSettings();
    }


    private void LoadPlayerSettings()
    {
        PLAYER_SETTINGS = HelperFunctions.ReadArrayFromFile("player_settings", '\n');

        if (PLAYER_SETTINGS != null)
        {
            LoadNickname();
        }
    }

    private void LoadNickname()
    {
        PhotonNetwork.NickName = PLAYER_SETTINGS[0];
        playerNameText.text = PhotonNetwork.NickName;
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
        if (playerNameText.text.Length.Equals(0))
        {
            AssignDefaultNickname();
        }

        MenuManager.Instance.OpenMenu("Multiplayer");
        Debug.Log("Joined Lobby");
    }

    public void AssignDefaultNickname()
    {
        PhotonNetwork.NickName = "Player " + Random.Range(0, 1000);
        playerNameText.text = PhotonNetwork.NickName;
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
            {
                continue;
            }

            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(GAME_INDEX);
    }

    public void UpdateName()
    {
        if (string.IsNullOrEmpty(playerNameInputField.text))
        {
            MenuManager.Instance.OpenMenu("Multiplayer");
        }
        else
        {
            PhotonNetwork.NickName = playerNameInputField.text;
            playerNameText.text = PhotonNetwork.NickName;
            HelperFunctions.WriteDatatoFile(PhotonNetwork.NickName, 0, '\n', "player_settings");
            MenuManager.Instance.OpenMenu("Multiplayer");
        }
    }
    
    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            PhotonNetwork.CreateRoom("Room " + Random.Range(0, 1000));
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

            // No need to check any more views, so break
            break;
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
            player.GetComponent<PlayerController>().ChangeCameraPosition();

            // No need to check any more views, so break
            break;
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

            // No need to check any more views, so break
            break;
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
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        MenuManager.Instance.OpenMenu("Main");

        FindObjectOfType<AudioManager>().Stop("BackgroundLoop");
        FindObjectOfType<AudioManager>().Play("Theme");

        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel(MENU_INDEX);
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
    public void StartRace(string route)
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
                bool joinSuccessful = GameObject.Find("Race Manager").GetComponent<RaceManager>().JoinRace(player, route);

                // Return if unsucessful
                if (!joinSuccessful)
                {
                    // Open HUD
                    MenuManager.Instance.OpenMenu("HUD");

                    // Display error
                    StartCoroutine(DisplayQuickNotificationText("Unable to join event!", 3));
                    return;
                }

                // Update player state
                player.state = PlayerState.ParticipatingInRace;

                // Reset track distance
                player.GetComponent<RouteFollower>().progressAlongRoute = 0;

                // Update menu buttons
                UpdateMenuButtons(false, true, false);

                // Hide non-race ui panels
                SwitchUIState(false, false, false);

                // Disable ui toggle button
                EnableUIToggle(false);

                // Open HUD
                MenuManager.Instance.OpenMenu("HUD");
            }

            // No need to check any more views, so break
            break;
        }
    }

    public void StartTrial(string route)
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
                bool joinSuccessful = GameObject.Find("Time Trial Manager").GetComponent<TrialManager>().JoinTrial(player, route);

                // Return if unsucessful
                if (!joinSuccessful)
                {
                    // Open HUD
                    MenuManager.Instance.OpenMenu("HUD");

                    // Display error
                    StartCoroutine(DisplayQuickNotificationText("Unable to join event!", 3));
                    return;
                }

                // Update player state
                player.state = PlayerState.ParticipatingInTrial;

                // Reset track distance
                player.GetComponent<RouteFollower>().Reset();

                // Update menu buttons
                UpdateMenuButtons(false, true, false);

                // Hide non-race ui panels
                SwitchUIState(false, false, false);

                // Disable ui toggle button
                EnableUIToggle(false);

                // Open HUD
                MenuManager.Instance.OpenMenu("HUD");
            }

            // No need to check any more views, so break
            break;
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

#if !UNITY_EDITOR

            // Reset track distance
            StartCoroutine(ResetTrackDistance());

#endif

            // Retrieve progress tracker
            RouteFollower routeFollower = player.GetComponent<RouteFollower>();

            // Reset track distance
            routeFollower.Reset();

            // Reset track
            routeFollower.route = routeFollower.routes[0];

            // Hide time and lap info
            HideEventInformationPanel();

            // Update menu buttons
            UpdateMenuButtons(true, false, true);

            // Reset UI state
            SwitchUIState(true, true, true);

            // Enable UI toggle
            EnableUIToggle();

            // Open HUD
            MenuManager.Instance.OpenMenu("HUD");

            // No need to check any more views, so break
            break;
        }
    }

    IEnumerator ResetTrackDistance()
    {
        BluetoothManager.Instance.EndJustRow();

        yield return new WaitForSeconds(0.25f);

        BluetoothManager.Instance.StartJustRow();

        yield return new WaitForSeconds(0.25f);

        UnpauseGame();
    }

    public void ExitEvent()
    {
        // Retrieve all players
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            // Retrieve player view
            PhotonView photonView = player.GetComponent<PhotonView>();

            // If not our view, skip rest of loop
            if (!photonView.IsMine) continue;

            // Remove player from race
            if (player.state.Equals(PlayerState.ParticipatingInRace)) player.race.RemovePlayerFromRace(player);

            // Remove player from trial
            if (player.state.Equals(PlayerState.ParticipatingInTrial)) player.trial.EndTrial();

            // No need to check any more views, so exit
            break;
        }
    }

    private void EnableUIToggle(bool enable = true)
    {
        toggleUIButton.interactable = enable;
    }

    public void InstantiateGhostTracker(GhostController ghost)
    {
        Slider progressBar = Instantiate(otherProgressBar, localProgressBar.transform.parent) as Slider;
        progressBar.gameObject.SetActive(true);

        ghost.progressBar = progressBar;

        // Update color
        ghost.progressBar.handleRect.GetComponent<Image>().color = Color.yellow;
    }

    public void DestroyGhostTracker(GhostController ghost)
    {
        if (ghost != null)
            Destroy(ghost.progressBar);
    }

    public void InstantiatePlayerTracker(PlayerController player)
    {
        Slider progressBar = Instantiate(otherProgressBar, localProgressBar.transform.parent) as Slider;
        progressBar.gameObject.SetActive(true);

        player.progressBar = progressBar;

        // Update color
        player.progressBar.handleRect.GetComponent<Image>().color =
            new Color(
                Random.Range(0f, 1f),
                0,
                0
            ); 
    }

    public void DestroyPlayerTracker(PlayerController player)
    {
        if (player != null)
            Destroy(player.progressBar);
    }

    public void UpdateGhostTracker(Route route, GhostController ghost, int numberOfLaps)
    {
        var routeLength = route.routeDistance;
        var ghostProgress = ghost.GetRouteDistance();

        // Calculate distance as a percentage of the total number of laps
        var distance = ghostProgress / (routeLength * numberOfLaps);
        
        // Update ghost progress bar
        ghost.progressBar.value = distance;
    }

    public void UpdatePlayerProgress(Route route, PlayerController player, int numberOfLaps)
    {
        var routeLength = route.routeDistance;
        var playerProgress = player.GetRouteDistance();

        // Calculate distance as a percentage of the total number of laps
        var distance = playerProgress / (routeLength * numberOfLaps);

        // If local view
        if (player.photonView.IsMine)
        {
            // Update local progress bar
            localProgressBar.value = distance;
        }

        else
        {
            // Update other progress bar
            player.progressBar.value = distance;
        }
    }

    public void ResetProgressBar()
    {
        localProgressBar.value = 0;
    }

    private void UpdateMenuButtons(bool showStartEvent, bool showLeaveEvent, bool showLeaveRoom)
    {
        startRaceButton.SetActive(showStartEvent);
        startTrialButton.SetActive(showStartEvent);
        exitEventButton.SetActive(showLeaveEvent);
        leaveRoomButton.SetActive(showLeaveRoom);
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
            confirmationText.GetComponent<TMP_Text>().text = "Are you sure you wish to leave this room?";
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
        if (confirmPressed)
        {
            if (waitingToConfirmLeaveRoom)
            {
                Instance.LeaveRoom();
            }
            else if (waitingToConfirmExitGame)
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

            // No need to check any more views, so break
            break;
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

            // No need to check any more views, so break
            break;
        }

        // Update game state
        State = GameState.Paused;
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

            // No need to check any more views, so break
            break;
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

            // No need to check any more views, so break
            break;
        }

        // Update game state
        State = GameState.Playing;
    }

    private bool mute = false;
    public void ToggleAudio()
    {
        mute = !mute;

        muteLine.SetActive(mute);

        AudioManager.Instance.ToggleAudio(mute);
    }

    private int UIState = 3;
    private const int MAX_UI_STATES = 6;
    public void ToggleUI()
    {
        UIState = (UIState < MAX_UI_STATES - 1) 
            ? UIState + 1 
            : 0;

        switch (UIState)
        {
            case 0:

                SwitchUIState(false, false, false);
                break;

            case 1:

                SwitchUIState(true, false, false);
                break;

            case 2:

                SwitchUIState(true, true, false);
                break;

            case 3:

                SwitchUIState(true, true, true);
                break;

            case 4:

                SwitchUIState(true, false, true);
                break;

            case 5:

                SwitchUIState(false, false, true);
                break;
        }
    }

    public void SwitchUIState(bool state0, bool state1, bool state2)
    {
        panels[0].gameObject.SetActive(state0);
        panels[1].gameObject.SetActive(state1);
        panels[2].gameObject.SetActive(state2);
    }

    public void DisplayEventPanel(string time, string lap, string position = null)
    {
        eventInformationPanel.SetActive(true);

        eventTimeText.text = time;
        eventLapText.text = lap;

        if (position != null)
        {
            eventPositionPanel.gameObject.SetActive(true);
            eventPositionText.text = position;
        }
        else
        {
            eventPositionPanel.gameObject.SetActive(false);
        }
    }

    public void HideEventInformationPanel()
    {
        eventInformationPanel.SetActive(false);
    }

    public void HideEventNotificationPanel()
    {
        eventNotificationPanel.SetActive(false);
    }

    public void ResetEventPanel()
    {
        eventTimeText.text = "00:00";
        eventLapText.text = "0";
        eventPositionText.text = "0";
    }

    public void HideNotificationPanel()
    {
        notificationPanel.SetActive(false);
    }

    public void HideCountdownPanel()
    {
        countdownPanel.SetActive(false);
    }

    public void HideAllPanels()
    {
        HideEventNotificationPanel();
        HideEventInformationPanel();
        HideNotificationPanel();
        HideCountdownPanel();
    }

    public void DisplayNotificationText(string text)
    {
        notificationPanel.SetActive(true);
        notificationText.text = text;
    }

    public IEnumerator DisplayQuickNotificationText(string text, int duration)
    {
        notificationPanel.SetActive(true);
        notificationText.text = text;

        yield return new WaitForSeconds(duration);

        notificationPanel.SetActive(false);
    }

    public IEnumerator DisplayCountdown(string time, int duration)
    {
        countdownPanel.SetActive(true);
        countdownPanel.GetComponentInChildren<TMP_Text>().text = time;

        yield return new WaitForSeconds(duration);
        countdownPanel.SetActive(false);
    }

    public IEnumerator SendEventNotification(EventCategory category, string title, string route, string laps, string participants, int duration)
    {
        eventNotificationPanel.SetActive(true);
        eventNotificationPanel.GetComponent<EventNotification>().Setup(category, title, route, laps, participants);

        yield return new WaitForSeconds(duration);

        eventNotificationPanel.SetActive(false);
    }
#endregion
}