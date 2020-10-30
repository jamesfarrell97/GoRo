using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

// Code referenced: https://www.youtube.com/watch?v=KHWuTBmT1oI
//
//
//
public class NetworkManager : MonoBehaviourPunCallbacks
{
    //// Start is called before the first frame update
    //void Start()
    //{
    //    ConnectToServer();
    //}

    //// Update is called once per frame
    //void ConnectToServer()
    //{
    //    PhotonNetwork.ConnectUsingSettings();
    //    Debug.Log("Connecting to Server...");
    //}

    //public override void OnConnectedToMaster()
    //{
    //    Debug.Log("Connected to Server.");
    //    base.OnConnectedToMaster();

    //    // Setup room
    //    RoomOptions roomOptions = new RoomOptions();
    //    roomOptions.MaxPlayers = 10;
    //    roomOptions.IsVisible = true;
    //    roomOptions.IsOpen = true;

    //    PhotonNetwork.JoinOrCreateRoom("Room 1", roomOptions, TypedLobby.Default);
    //}

    //public override void OnJoinedRoom()
    //{
    //    Debug.Log("Joined a Room");
    //    base.OnJoinedRoom();
    //}

    //public override void OnPlayerEnteredRoom(Player newPlayer)
    //{
    //    Debug.Log("A new player has joined the room");
    //    base.OnPlayerEnteredRoom(newPlayer);
    //}
}
