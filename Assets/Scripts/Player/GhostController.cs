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
    
    private RouteFollower routeFollower;

    private float[] speedSamples;
    private int sampleIndex;

    private bool paused;

    private void Awake()
    {
        routeFollower = GetComponent<RouteFollower>();
    }

    private void Start()
    {
        Reset();
        InvokeRepeating("UpdateSpeed", 0, 0.5f);
    }

    private void Reset()
    {
        sampleIndex = 0;
        paused = true;
    }

    public void InstantiateGhostTrial(Trial trial)
    {
        this.trial = trial;
    }

    public void InstantiateGhostSamples(float[] speedSamples)
    {
        this.speedSamples = speedSamples;

        routeFollower.UpdateVelocity(speedSamples[++sampleIndex]);
    }

    private void UpdateSpeed()
    {
        if (paused) return;

        // Select next sample in the speed samples array
        sampleIndex = (sampleIndex < speedSamples.Length - 1) 
            ? sampleIndex + 1 
            : 0;
        
        // Update velocity
        routeFollower.UpdateVelocity(speedSamples[sampleIndex]);

        // Animate
        Animate();
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