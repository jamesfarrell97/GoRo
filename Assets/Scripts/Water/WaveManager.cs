using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code referenced: https://www.youtube.com/watch?v=eL_zHQEju8s&t=529s
//
//
//
public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    public float amplitude = 1f;
    public float length = 2f;
    public float speed = 1f;
    public float offset = 0f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } 

        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }
    
    private void Update()
    {
        offset += speed * Time.deltaTime;
    }

    public float GetWaveHeight(float x)
    {
        return amplitude * Mathf.Sin(x / length + offset);
    }
}
