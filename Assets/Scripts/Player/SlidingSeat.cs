using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingSeat : MonoBehaviour
{
    [SerializeField] Transform trackedObject;
    [SerializeField] Transform maxPoint;
    [SerializeField] Transform minPoint;

    private float offset;

    private void Start()
    {
        offset = trackedObject.position.z - transform.position.z;
    }

    private void FixedUpdate()
    {
        Vector3 position = transform.position;
        Vector3 trackedPosition = trackedObject.position;

        if (trackedPosition.z - offset < minPoint.position.z && trackedPosition.z - offset > maxPoint.position.z)
        {
            transform.position = new Vector3(position.x, position.y, trackedPosition.z - offset);
        }
    }
}
