using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Stats : MonoBehaviour
{
    public static readonly float KILOMETERS_METER = 100;    // TEST VALUES
    public static readonly float SECS_MINUTE = 10;          // TEST VALUES

    private Vector3 position;
    private float metersRowed;
    private float secondsRowing;

    private void Awake()
    {
        ResetStats();
    }

    private void Update()
    {
        UpdateDistance();
        UpdateTime();
    }

    public void ResetStats()
    {
        position = transform.position;
        metersRowed = 0;
        secondsRowing = 0;
    }

    private void UpdateDistance()
    {
        metersRowed += Vector3.Magnitude(transform.position - position);
        position = transform.position;
    }

    private void UpdateTime()
    {
        secondsRowing += Time.deltaTime;
    }

    public float GetMetersRowed()
    {
        return this.metersRowed;
    }

    public float GetSecondsRowing()
    {
        return this.secondsRowing;
    }
}
