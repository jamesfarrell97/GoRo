using UnityEngine;

// Code referenced: https://www.youtube.com/watch?v=I64KMcVDq4E&list=PLhsVv9Uw1WzjI8fEBjBQpTyXNZ6Yp1ZLw&index=11&ab_channel=RugbugRedfern
//
//
//
public class SpawnPoint : MonoBehaviour
{
    [SerializeField] GameObject graphics;

    void Awake()
    {
        graphics.SetActive(false);
    }
}
