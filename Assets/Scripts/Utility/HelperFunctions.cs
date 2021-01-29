using UnityEngine;

public class HelperFunctions : MonoBehaviour
{
    public static int[] SecondsToHMS(int seconds)
    {
        int s = seconds % 60;
        int m = seconds / 60;
        int h = m / 60;

        return new int[] { h, m, s };
    }
}
