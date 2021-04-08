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

    private float[] samples;
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

        InvokeRepeating("UpdateMovement", 0f, (1f / StatsManager.MOVE_SAMPLE_RATE));
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

    public void InstantiateGhostSamples(float[] samples)
    {
        this.samples = samples;

        routeFollower.UpdateDistance(0);
    }

    public float routeDistance = 0;

    private void UpdateMovement()
    {
        if (paused) return;

        // Update distance
        routeDistance = samples[sampleIndex];

        // Move
        routeFollower.UpdateDistance(routeDistance);
        
        // Select next sample in the speed samples array
        sampleIndex = (sampleIndex < samples.Length - 1) 
            ? sampleIndex + 1 
            : 0;
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

    public float GetRouteDistance()
    {
        return routeDistance;
    }

    public bool Paused()
    {
        return this.paused;
    }
}