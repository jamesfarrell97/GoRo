using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    [SerializeField] Camera mapCamera;
    [SerializeField] GameObject targetTerrain;
    [SerializeField] public int fileCounter;

    void Start()
    {
        mapCamera.transform.LookAt(targetTerrain.transform.position);
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Capture();
        }
    }

    public void Capture()
    {
        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = mapCamera.targetTexture;

        mapCamera.Render();

        Texture2D image = new Texture2D(mapCamera.targetTexture.width, mapCamera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, mapCamera.targetTexture.width, mapCamera.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = activeRenderTexture;

        byte[] bytes = image.EncodeToPNG();
        Destroy(image);

        File.WriteAllBytes(Application.dataPath + "/Backgrounds/" + fileCounter + ".png", bytes);
        fileCounter++;
    }
}
