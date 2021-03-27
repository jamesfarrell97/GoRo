using UnityEngine;

// Code referenced: https://github.com/ThisIsFix/Unity-Billboard/blob/master/BillboardFX.cs
//
//
//
public class Billboard : MonoBehaviour
{
    public Transform camTransform;

    Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (camTransform == null) return;

        transform.rotation = camTransform.rotation * originalRotation;
    }
}