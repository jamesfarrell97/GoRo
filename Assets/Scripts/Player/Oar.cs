using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Oar : MonoBehaviour
{
    [SerializeField] Transform trackedObject;
    [SerializeField] Transform pivotPoint;

    private Vector3 offset;
    private bool playCollisionSound;

    private void Start()
    {
        offset = pivotPoint.position - trackedObject.position;
    }

    private void FixedUpdate()
    {
        transform.LookAt(trackedObject);
        
        transform.Rotate(new Vector3(0, 0, trackedObject.localEulerAngles.x));
        transform.position = pivotPoint.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag.Equals("Water") && playCollisionSound)
        {
            FindObjectOfType<AudioManager>().Play("Rowing" + Random.Range(1,5));
            playCollisionSound = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag.Equals("Water"))
        {
            playCollisionSound = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Water") && playCollisionSound)
        {
            FindObjectOfType<AudioManager>().Play("Rowing" + Random.Range(1, 5));
            playCollisionSound = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Water"))
        {
            playCollisionSound = true;
        }
    }
}