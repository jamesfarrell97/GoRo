using UnityEngine;

public class Seat : MonoBehaviour
{
    [SerializeField] Transform trackedObject;

    private void FixedUpdate()
    {
        transform.position = trackedObject.position;
    }
}