using UnityStandardAssets.Utility;
using UnityEngine;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class GhostController : MonoBehaviour
{
    [SerializeField] private Animator[] animators;

    [HideInInspector] public Trial trial;

    private Rigidbody rigidbody;
    private RouteFollower routeFollower;

    private float averageSpeed = 0;
    private bool paused = true;

    private void Awake()
    {
        routeFollower = GetComponent<RouteFollower>();
    }

    public void InstantiateGhostTrial(Trial trial)
    {
        this.trial = trial;
    }

    public void InstantiateGhostSpeed(float averageSpeed)
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
        //routeFollower.UpdateVelocity(0.55f);
    }

    private void Animate()
    {
        //foreach (Animator animator in animators)
        //{
        //    animator.SetBool("Paused", paused);
        //}
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
