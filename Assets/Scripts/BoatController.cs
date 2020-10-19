using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class BoatController : MonoBehaviour
{
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject midLeft;
    public GameObject midRight;
    public float boatSpeed = 15f;
    public float boatTurningSpeed = 1f;
    public float handSpeedFactor = 5f;

    public XRNode leftInputSource;
    public XRNode rightInputSource;

    private InputDevice leftDevice;
    private InputDevice rightDevice;

    private Rigidbody rigidBody;
    private Vector3 movement;

    private Vector3 leftHandPositionPreviousFrame;
    private Vector3 rightHandPositionPreviousFrame;
    private Vector3 playerPositionPreviousFrame;
    private Vector3 leftHandPositionThisFrame;
    private Vector3 rightHandPositionThisFrame;
    private Vector3 playerPositionThisFrame;

    private bool leftTrigger;
    private bool rightTrigger;
    private bool leftGrip;
    private bool rightGrip;

    private float handSpeed;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        playerPositionPreviousFrame = transform.position;
        leftHandPositionPreviousFrame = leftHand.transform.position;
        rightHandPositionPreviousFrame = rightHand.transform.position;
    }

    void Update()
    {
        leftDevice = InputDevices.GetDeviceAtXRNode(leftInputSource);
        rightDevice = InputDevices.GetDeviceAtXRNode(rightInputSource);

        leftDevice.TryGetFeatureValue(CommonUsages.triggerButton, out leftTrigger);
        leftDevice.TryGetFeatureValue(CommonUsages.gripButton, out leftGrip);

        rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out rightTrigger);
        rightDevice.TryGetFeatureValue(CommonUsages.gripButton, out rightGrip);
    }

    private void FixedUpdate()
    {
        Vector3 forward = rigidBody.transform.forward;
        Vector3 up = rigidBody.transform.up;

        leftHandPositionThisFrame = new Vector3(
            leftHand.transform.position.x * forward.x,
            leftHand.transform.position.y * forward.y,
            leftHand.transform.position.z * forward.z
        );

        rightHandPositionThisFrame = new Vector3(
            rightHand.transform.position.x * forward.x,
            rightHand.transform.position.y * forward.y,
            rightHand.transform.position.z * forward.z
        );

        playerPositionThisFrame = new Vector3(
            transform.position.x * forward.x,
            transform.position.y * forward.y,
            transform.position.z * forward.z
        );

        float playerDistancedMoved = Vector3.Distance(playerPositionThisFrame, playerPositionPreviousFrame);
        float leftHandDistancedMoved = Vector3.Distance(leftHandPositionThisFrame, leftHandPositionPreviousFrame);
        float rightHandDistancedMoved = Vector3.Distance(rightHandPositionThisFrame, rightHandPositionPreviousFrame);

        handSpeed = ((leftHandDistancedMoved - playerDistancedMoved) + (rightHandDistancedMoved - playerDistancedMoved));

        if (Time.timeSinceLevelLoad > 1f)
        {
            // Move Forwards
            if (leftGrip && rightGrip && leftTrigger && rightTrigger)
            {
                rigidBody.AddForce(forward * (-handSpeed * handSpeedFactor) * boatSpeed);
            }

            // Move Towards Right
            else if (leftGrip && rightGrip && leftTrigger)
            {
                rigidBody.AddTorque(up * (-handSpeed * handSpeedFactor) * boatTurningSpeed);
                rigidBody.AddForce(forward * (-handSpeed * handSpeedFactor) * boatSpeed);
            }

            // Move Towards Left
            else if (leftGrip && rightGrip && rightTrigger)
            {
                rigidBody.AddTorque(-up * (-handSpeed * handSpeedFactor) * boatTurningSpeed);
                rigidBody.AddForce(forward * (-handSpeed * handSpeedFactor) * boatSpeed);
            }

            // Turn Right
            else if (leftGrip && leftTrigger)
            {

                rigidBody.AddTorque(-up * (-handSpeed * handSpeedFactor) * boatTurningSpeed);
            }

            // Turn Left
            else if (rightGrip && rightTrigger)
            {
                rigidBody.AddTorque(up * (-handSpeed * handSpeedFactor) * boatTurningSpeed);
            }

            // Move Backwards
            else if (leftGrip && rightGrip)
            {
                rigidBody.AddForce(forward * (handSpeed * handSpeedFactor) * boatSpeed);
            }
        }

        leftHandPositionPreviousFrame = leftHandPositionThisFrame;
        rightHandPositionPreviousFrame = rightHandPositionThisFrame;
        playerPositionPreviousFrame = playerPositionThisFrame;
    }
}
