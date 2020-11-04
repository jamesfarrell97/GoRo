using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject leftHand;
    [SerializeField] GameObject rightHand;
    [SerializeField] GameObject leftTurningPoint;
    [SerializeField] GameObject rightTurningPoint;
    [SerializeField] float boatSpeed = 30f;
    [SerializeField] float boatTurningSpeed = 1f;
    [SerializeField] float handSpeedFactor = 5f;

    [SerializeField] [Range(0f, 1f)] readonly float triggerPressureThreshold = 0.5f;
    [SerializeField] [Range(0f, 1f)] readonly float gripPressureThreshold = 0.5f;
    [SerializeField] XRNode leftInputSource;
    [SerializeField] XRNode rightInputSource;

    [SerializeField] Boat boat;
    [SerializeField] AchievementTracker achievementTracker;
    [SerializeField] Stats stats;

    private InputDevice leftDevice;
    private InputDevice rightDevice;

    private PhotonView photonView;
    private BoxCollider boxCollider;
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

    private float leftTriggerPressure;
    private float rightTriggerPressure;
    private float leftGripPressure;
    private float rightGripPressure;

    private float handSpeed;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        rigidBody = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (!photonView.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rigidBody);
        }

        playerPositionPreviousFrame = transform.position;
        leftHandPositionPreviousFrame = leftHand.transform.position;
        rightHandPositionPreviousFrame = rightHand.transform.position;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        achievementTracker.TrackAchievements(photonView, stats);

        // Left Oar Input
        leftDevice = InputDevices.GetDeviceAtXRNode(leftInputSource);

        leftDevice.TryGetFeatureValue(CommonUsages.trigger, out leftTriggerPressure);
        leftDevice.TryGetFeatureValue(CommonUsages.grip, out leftGripPressure);

        leftTrigger = (leftTriggerPressure >= triggerPressureThreshold);
        leftGrip = (leftGripPressure >= gripPressureThreshold);

        // Right Oar Input
        rightDevice = InputDevices.GetDeviceAtXRNode(rightInputSource);

        rightDevice.TryGetFeatureValue(CommonUsages.trigger, out rightTriggerPressure);
        rightDevice.TryGetFeatureValue(CommonUsages.grip, out rightGripPressure);

        rightTrigger = (rightTriggerPressure >= triggerPressureThreshold);
        rightGrip = (rightGripPressure >= gripPressureThreshold);
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag.Equals("Obstacle"))
        {
            FindObjectOfType<AudioManager>().Play("Collision");
        }    
    }
}
