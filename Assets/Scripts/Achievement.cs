using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Achievement : MonoBehaviour
{
    public string title;

    private MeshRenderer meshRenderer;
    
    void Awake()
    {
        // Disable mesh rendered (hide model)
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;

        // Automatically set title 
        title = (title.Length <= 0) ? gameObject.name : title;
    }

    public void Activate(AchievementDisplay achievementDisplay)
    {
        // Store achievement slot
        Transform achievementSlot = achievementDisplay.GetAchievementSlot();

        // Instantiate class
        Achievement achievement = Instantiate(this);

        // Child instance to achievement slot and display mesh
        achievement.transform.SetPositionAndRotation(achievementSlot.position, achievementSlot.rotation);
        achievement.transform.SetParent(achievementSlot);
        achievement.meshRenderer.enabled = true;
    }
}
