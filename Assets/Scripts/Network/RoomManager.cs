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

    // EXTRACT INTO APPDATA FILE
    private const int gameIndex = 1;

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
        if (scene.buildIndex == gameIndex)
        {
            PhotonNetwork.Instantiate(Path.Combine("Photon Prefabs", "Managers", "Player Manager"), Vector3.zero, Quaternion.identity);

            FindObjectOfType<AudioManager>().Stop("Theme");
            FindObjectOfType<AudioManager>().Play("BackgroundLoop");
        }
    }
}
