using System.Collections;
using System.Collections.Generic;

using UnityEngine.XR;
using UnityEngine;

public class Oar : MonoBehaviour
{
    [SerializeField] XRNode inputSource;
    [SerializeField] Transform trackedObject;

    [SerializeField] [Range(0f, 1f)] float triggerPressureThreshold = 0.7f;
    [SerializeField] [Range(0f, 1f)] float gripPressureThreshold = 0.7f;

    private InputDevice inputDevice;
    private float triggerPressure;
    private float gripPressure;

    private bool trigger;
    private bool grip;

    //[SerializeField] Transform pivotPoint;

    //[SerializeField] float positiveAngleLimitX = 45f;
    //[SerializeField] float positiveAngleLimitY = 45f;
    //[SerializeField] float positiveAngleLimitZ = 45f;

    //[SerializeField] float negativeAngleLimitX = -45f;
    //[SerializeField] float negativeAngleLimitY = -45f;
    //[SerializeField] float negativeAngleLimitZ = -45f;

    //private Vector3 previousPosition;
    //private Vector3 currentPosition;
    //private Vector3 deltaPosition;
    //private Vector3 pivotOffset;

    //private Vector3 initialRotation;
    //private Vector3 currentRotation;

    //private Quaternion previousRotation;

    //private float xRotation = 0;
    //private float yRotation = 0;
    //private float zRotation = 0;

    void Start()
    {
    }

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

            Vector3 currentRotation = transform.localRotation.eulerAngles;
            Vector3 trackedRotation = trackedObject.localRotation.eulerAngles;
            Quaternion desiredRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, -trackedRotation.x);

            transform.rotation = desiredRotation;
        }
    }
}

//previousPosition = trackedObject.position;
//previousRotation = trackedObject.rotation;
//initialRotation = transform.rotation.eulerAngles;

////pivotOffset = new Vector3(0, trackedObject.position.y - pivotPoint.position.y, trackedObject.position.z - pivotPoint.position.z);
////pivotOffset = Vector3.Distance(trackedObject.position, pivotPoint.position);

//currentPosition = trackedObject.position;
////currentRotation = trackedObject.rotation;

////var h = Vector3.Distance(trackedObject.position, pivotPoint.position);
////var o = Vector3.Distance(currentPosition, previousPosition);
////deltaPosition = currentPosition - previousPosition;

//var hy = trackedObject.position.y - pivotPoint.position.y;
//var oy = currentPosition.y - previousPosition.y;

//Debug.Log("HY: " + hy);
//Debug.Log("OY: " + oy);

//var hz = trackedObject.position.z - pivotPoint.position.z;
//var oz = currentPosition.z - previousPosition.z;

//Debug.Log("HZ: " + hz);
//Debug.Log("OZ: " + oz);

////xRotation = Mathf.Rad2Deg * currentRotation.x - Mathf.Rad2Deg * previousRotation.x;
////yRotation = Mathf.Rad2Deg * Mathf.Atan2(deltaPosition.z, Vector3.Distance(trackedObject.position, pivotPoint.position) /*Vector3.Magnitude(pivotOffset)*/);
////zRotation = Mathf.Rad2Deg * Mathf.Atan2(deltaPosition.y, Vector3.Distance(trackedObject.position, pivotPoint.position) /*Vector3.Magnitude(pivotOffset)*/);

//if (hz > 0)
//yRotation = Mathf.Rad2Deg * Mathf.Asin(oz / hz);

//if (hy > 0)
//zRotation = Mathf.Rad2Deg * Mathf.Asin(oy / hy);

////Debug.Log("CR: " + currentRotation.x);
////Debug.Log("PR: " + previousRotation.x);
////Debug.Log("XR: " + xRotation);
////Debug.Log("");

////yRotation = Vector3.Angle(new Vector3(0, 0, currentPosition.z), new Vector3(0, 0, previousPosition.z));
////zRotation = Vector3.Angle(new Vector3(0, currentPosition.y, 0), new Vector3(0, previousPosition.z, 0));

////Debug.Log("DP: " + deltaPosition.z);
////Debug.Log("PO: " + pivotOffset.x);
//Debug.Log("YR: " + yRotation);
////Debug.Log("");
////Debug.Log("DP: " + deltaPosition.y);
////Debug.Log("PO: " + pivotOffset.x);
//Debug.Log("ZR: " + zRotation);

//transform.Rotate(xRotation, yRotation, zRotation);
////var t = transform.rotation.eulerAngles;
////var g = trackedObject.rotation.eulerAngles;
////var rot = Quaternion.Euler(g.x, t.y, t.z);
////transform.rotation = rot;

//previousPosition = currentPosition;
////previousRotation = currentRotation;