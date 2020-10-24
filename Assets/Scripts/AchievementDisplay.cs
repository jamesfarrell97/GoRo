using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementDisplay : MonoBehaviour
{
    public Transform[] achievementSlots;

    private int slotCount;
    private int slotIndex;

    void Start()
    {
        slotCount = achievementSlots.Length;
        slotIndex = 0;
    }

    public Transform GetAchievementSlot()
    {
        if (slotIndex == slotCount)
        {
            slotIndex = 0;
        }

        // Index updates after variable (slotIndex) is returned
        return achievementSlots[slotIndex++];
    }
}
