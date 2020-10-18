using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    public GameObject midLeft;
    public GameObject midRight;
    public float boatSpeed = 15f;
    public float boatTurningSpeed = 5f;

    private Rigidbody rigidBody;
    private Vector3 movement;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
        {
            rigidBody.AddForce(rigidBody.transform.forward * boatSpeed);
        }

        else if (Input.GetKey(KeyCode.S))
        {
            rigidBody.AddForce(-rigidBody.transform.forward * boatSpeed);
        }

        else if (Input.GetKey(KeyCode.A))
        {
            rigidBody.AddTorque(rigidBody.transform.up * boatTurningSpeed);
        }

        else if (Input.GetKey(KeyCode.D))
        {
            rigidBody.AddTorque(-rigidBody.transform.up * boatTurningSpeed);
        }
    }
}
