using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

// Code referenced: https://www.youtube.com/watch?v=eL_zHQEju8s&t=529s
//
//
//
public class WaterManager : MonoBehaviour
{
    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }
    
    private void Update()
    {
        Vector3[] vertices = meshFilter.mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y = WaveManager.Instance.GetWaveHeight(transform.position.x + vertices[i].x);
        }

        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.RecalculateNormals();
    }
}
