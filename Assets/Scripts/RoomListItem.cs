using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Photon.Realtime;
using TMPro;

//Code referenced: https://www.youtube.com/watch?v=zPZK7C5_BQo
//
//
//
public class RoomListItem : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    public RoomInfo info;

    public void SetUp(RoomInfo roomInfo)
    {
        info = roomInfo;
        text.text = roomInfo.Name;
    }

    public void OnClick()
    {
        Launcher.Instance.JoinRoom(info);
    }
}
