using UnityEngine;

// Code referenced: https://github.com/ThisIsFix/Unity-Billboard/blob/master/BillboardFX.cs
//
//
//
public class Billboard : MonoBehaviour
{
    public Transform camera;

    Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (camera == null) return;

        transform.LookAt(
            transform.position + camera.transform.rotation * Vector3.forward,
            camera.transform.rotation * Vector3.up
        );

        //Vector3 eulerAngles = transform.eulerAngles;
        //eulerAngles.x = 0;
        //transform.eulerAngles = eulerAngles;
    }
}