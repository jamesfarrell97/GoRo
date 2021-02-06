using UnityEngine;

public class Boat : MonoBehaviour
{
    [SerializeField] Transform[] achievementSlots;

    private int slotCount;
    private int slotIndex;

    private void Start()
    {
        slotCount = achievementSlots.Length;
        slotIndex = 0;
    }

    public bool IsSlotAvailable()
    {
        return (slotIndex != slotCount);
    }

    public Transform GetAchievementSlot()
    {
        // Index updates after variable (slotIndex) is returned
        return achievementSlots[slotIndex++];
    }
}
