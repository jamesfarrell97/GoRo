using System.Collections;
using System.Collections.Generic;

using UnityEngine;

// Code referenced: https://www.youtube.com/watch?v=eL_zHQEju8s&t=529s
//
//
//
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [SerializeField] float amplitude = 1f;
    [SerializeField] float length = 2f;
    [SerializeField] float speed = 1f;
    [SerializeField] float offset = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } 
        else if (Instance != this)
        {
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
