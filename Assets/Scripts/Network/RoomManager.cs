using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

//Code referenced: https://www.youtube.com/watch?v=6qRNBPPojMA
//
//
//
public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 1)
        {
            PhotonNetwork.Instantiate(Path.Combine("Photon Prefabs", "Player", "Player Manager"), Vector3.zero, Quaternion.identity);

            FindObjectOfType<AudioManager>().Stop("Theme");
            FindObjectOfType<AudioManager>().Play("BackgroundLoop");
        }

        if (scene.buildIndex == 2)
        {
            PhotonNetwork.Instantiate(Path.Combine("Photon Prefabs", "Managers", "Race Manager"), new Vector3(0, 0, -100), Quaternion.identity);
            PhotonNetwork.Instantiate(Path.Combine("Photon Prefabs", "Managers", "Audio Manager"), Vector3.zero, Quaternion.identity);
            PhotonNetwork.Instantiate(Path.Combine("Photon Prefabs", "Player", "Player Manager"), Vector3.zero, Quaternion.identity);
        }
    }
}
