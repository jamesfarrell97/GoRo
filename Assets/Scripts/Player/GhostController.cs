using UnityStandardAssets.Utility;
using UnityEngine.UI;
using UnityEngine;

// Code referenced: https://www.youtube.com/watch?v=7bevpWbHKe4&t=315s
//
//
//
public class GhostController : MonoBehaviour
{
    [SerializeField] private Animator[] animators;

    [HideInInspector] public Trial trial;

    [HideInInspector] public Slider progressBar;

    private RouteFollower routeFollower;

    private float[] speedSamples;
    private int sampleIndex;

    private bool paused;
    private float progressAlongRoute;

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
        
        // Update velocity
        routeFollower.UpdateVelocity(speedSamples[sampleIndex]);

        // Select next sample in the speed samples array
        sampleIndex = (sampleIndex < speedSamples.Length - 1) 
            ? sampleIndex + 1 
            : 0;
    }

    public void UpdateProgress(float progressAlongRoute)
    {
        this.progressAlongRoute = progressAlongRoute;
    }

    public void Pause()
    {
        this.paused = true;

        PauseAnimations();
    }

    public void Resume()
    {
        this.paused = false;

        PlayAnimations();
    }

    public void PlayAnimations()
    {
        foreach (Animator animator in animators)
        {
            animator.SetInteger("State", 1);
        }
    }

    public void PauseAnimations()
    {
        foreach (Animator animator in animators)
        {
            animator.SetInteger("State", 0);
        }
    }

    public bool Paused()
    {
        return this.paused;
    }

    public float GetProgress()
    {
        return progressAlongRoute;
    }
}