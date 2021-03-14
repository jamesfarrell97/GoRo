using UnityEngine;

// Code referenced: https://www.youtube.com/watch?v=I64KMcVDq4E&list=PLhsVv9Uw1WzjI8fEBjBQpTyXNZ6Yp1ZLw&index=11&ab_channel=RugbugRedfern
//
//
//
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    SpawnPoint[] spawnPoints;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        spawnPoints = GetComponentsInChildren<SpawnPoint>();
    }

    public Transform GetSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length - 1)].transform;
    }
}
