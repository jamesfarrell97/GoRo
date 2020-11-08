using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// Code referenced: https://www.youtube.com/watch?v=eL_zHQEju8s&t=529s
//
//
//
public class Floater : MonoBehaviour
{
    [SerializeField] Rigidbody rigidBody;

    [SerializeField] int floaterCount = 1;
    [SerializeField] float depthBeforeSubmerged = 1f;
    [SerializeField] float displacementAmount = 3f;
    [SerializeField] float waterDrag = 0.99f;
    [SerializeField] float waterAngularDrag = 0.5f;

    private PhotonView photonView;
    
    private void Start()
    {
        photonView = GetComponentInParent<PhotonView>();
    }
    
    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        rigidBody.AddForceAtPosition(Physics.gravity / floaterCount, transform.position, ForceMode.Acceleration);

        float waveHeight = WaveManager.Instance.GetWaveHeight(transform.position.x);

        if (transform.position.y < waveHeight)
        {
            float displacementMultiplier = Mathf.Clamp01((waveHeight - transform.position.y) / depthBeforeSubmerged) * displacementAmount;
            rigidBody.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), transform.position, ForceMode.Acceleration);
            rigidBody.AddForce(displacementMultiplier * -rigidBody.velocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
            rigidBody.AddTorque(displacementMultiplier * -rigidBody.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }
}
