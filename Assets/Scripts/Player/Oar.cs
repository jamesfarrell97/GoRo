using System.Collections;
using System.Collections.Generic;

using UnityEngine.XR;
using UnityEngine;

public class Oar : MonoBehaviour
{
    [SerializeField] XRNode inputSource;
    [SerializeField] Transform trackedObject;
    [SerializeField] Transform pivotPoint;

    [SerializeField] [Range(0f, 1f)] float triggerPressureThreshold = 0.7f;
    [SerializeField] [Range(0f, 1f)] float gripPressureThreshold = 0.7f;

    private InputDevice inputDevice;
    private float triggerPressure;
    private float gripPressure;

    private bool trigger;
    private bool grip;

    private void Update()
    {
        inputDevice = InputDevices.GetDeviceAtXRNode(inputSource);

        inputDevice.TryGetFeatureValue(CommonUsages.trigger, out triggerPressure);
        inputDevice.TryGetFeatureValue(CommonUsages.grip, out gripPressure);

        trigger = (triggerPressure >= triggerPressureThreshold);
        grip = (gripPressure >= gripPressureThreshold);
    }

    void FixedUpdate()
    {
        if (trigger && grip)
        {
            transform.LookAt(trackedObject);

            Vector3 currentRotation = transform.rotation.eulerAngles;
            Vector3 trackedRotation = trackedObject.rotation.eulerAngles;

            float rotationDirection = (inputSource.Equals(XRNode.LeftHand)) ? trackedRotation.x : -trackedRotation.x;
            Quaternion desiredRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, rotationDirection);

            transform.rotation = desiredRotation;
        }

        transform.position = pivotPoint.position;
    }
}