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
        PhotonNetwork.Instantiate(Path.Combine("Photon Prefabs", "Player", "Player"), new Vector3(-250, 0.55f, -1000), Quaternion.Euler(0, 180, 0));
    }
}
