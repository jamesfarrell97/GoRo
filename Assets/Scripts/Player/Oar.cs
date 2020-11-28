using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Oar : MonoBehaviour
{
    [SerializeField] Transform trackedObject;
    [SerializeField] Transform pivotPoint;

    private Vector3 offset;

    private void Start()
    {
        offset = pivotPoint.position - trackedObject.position;
    }

    private void FixedUpdate()
    {
        transform.LookAt(trackedObject);

        Vector3 currentRotation = transform.rotation.eulerAngles;
        Vector3 trackedRotation = trackedObject.rotation.eulerAngles;

        transform.position = pivotPoint.position;
    }
}