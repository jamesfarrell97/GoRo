using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rower : MonoBehaviour
{
    public AchievementDisplay achievementDisplay;

    // Achievements
    private Achievement[] achievements;

    private bool gettingStarted = false;
    private bool seasick = false;

    // Stats
    private static readonly float KILOMETERS_METER = 50;  // TEST VALUES
    private static readonly float SECS_MINUTE = 5;         // TEST VALUES

    private Vector3 startingPos;
    private float distanceMeters;
    private float timeSecs;

    // Start is called before the first frame update
    void Start()
    {
        achievements = Resources.FindObjectsOfTypeAll(typeof(Achievement)) as Achievement[];

        startingPos = transform.position;
        distanceMeters = 0;
        timeSecs = 0;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDistance();
        UpdateTime();

        if (!gettingStarted && distanceMeters / KILOMETERS_METER > 1)
        {
            Debug.Log("Achieved! Getting Started!");
            Array.Find(achievements, a => a.title == "Getting Started").Activate(achievementDisplay);
            gettingStarted = true;
        }

        if (!seasick && timeSecs / SECS_MINUTE > 1)
        {
            Debug.Log("Achieved! Seasick!");
            Array.Find(achievements, a => a.title == "Seasick!").Activate(achievementDisplay);
            seasick = true;
        }
    }

    private void UpdateDistance()
    {
        distanceMeters += Vector3.Magnitude(transform.position - startingPos);
        Debug.Log("DM: " + (int) distanceMeters / KILOMETERS_METER);

        startingPos = transform.position;
    }

    private void UpdateTime()
    {
        timeSecs = Time.timeSinceLevelLoad;
        Debug.Log("TS: " + (int) timeSecs / SECS_MINUTE);
    }

}
