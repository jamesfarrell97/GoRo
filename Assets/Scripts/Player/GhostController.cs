using UnityStandardAssets.Utility;
using UnityEngine;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class GhostController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private Rigidbody rigidbody;
    private RouteFollower routeFollower;

    private Trial trial;

    private float averageSpeed = 0;
    private bool paused = true;

    private void Awake()
    {
        routeFollower = GetComponent<RouteFollower>();
    }

    private void InstantiateGhostTrial(Trial trial)
    {
        this.trial = trial;
    }

    private void InstantiateGhostSpeed(float averageSpeed)
    {
        this.averageSpeed = averageSpeed;
    }

    private void FixedUpdate()
    {
        UpdateSpeed();
        Animate();
    }
    
    private void UpdateSpeed()
    {
        routeFollower.UpdateVelocity(averageSpeed);
    }

    private void Animate()
    {
        animator.SetBool("Paused", paused);
    }

    public void Pause()
    {
        this.paused = true;
    }

    public void Resume()
    {
        this.paused = false;
    }

    public bool Paused()
    {
        return this.paused;
    }
}
