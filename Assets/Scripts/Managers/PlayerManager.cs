using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Photon.Pun;
using System.IO;

//Code referenced: https://www.youtube.com/watch?v=6qRNBPPojMA
//
//
//
public class PlayerManager : MonoBehaviour
{
    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            CreateController();
        }
    }

    private void CreateController()
    {
        PhotonNetwork.Instantiate(Path.Combine("Photon Prefabs", "Player", "Player"), new Vector3(-1000, 0, -617), Quaternion.Euler(0, -14, 0), 0, new object[] { photonView.ViewID });
    }
}
