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
        PhotonNetwork.Instantiate(Path.Combine("Photon Prefabs", "Player", "Player"), Vector3.zero, Quaternion.identity, 0, new object[] { photonView.ViewID });

        //Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint();
        //PhotonNetwork.Instantiate(Path.Combine("Photon Prefabs", "Player", "Player"), spawnPoint.position, spawnPoint.rotation, 0, new object[] { photonView.ViewID });
    }
}
