using System;
using System.IO;
using UnityEngine;

public static class HelperFunctions
{
    public static int[] SecondsToHMS(int seconds)
    {
        int s = seconds % 60;
        int m = seconds / 60;
        int h = m / 60;

        return new int[] { h, m, s };
    }

    // Code referenced: https://www.codegrepper.com/code-examples/csharp/c%23+get+array+subarray
    public static T[] SubArray<T>(this T[] array, int offset, int length)
    {
        T[] result = new T[length];
        Array.Copy(array, offset, result, 0, length);
        return result;
    }

    // Code referenced: https://www.youtube.com/playlist?list=PLzDRvYVwl53v5ur4GluoabyckImZz3TVQ
    public static float GetAngleFromVectorFloat(Vector3 direction)
    {
        direction = direction.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        return angle;
    }

    // Code referenced: https://forum.unity.com/threads/change-gameobject-layer-at-run-time-wont-apply-to-child.10091/
    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }

            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    // Code referenced: https://forum.unity.com/threads/local-android-save-data.130740/
    public static void WriteStringToFile(string str, string filename)
    {

#if !WEB_BUILD

        string path = PathForDocumentsFile(filename);
        FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);

        StreamWriter sw = new StreamWriter(file);
        sw.WriteLine(str);

        sw.Close();
        file.Close();

#endif

    }

    // Code referenced: https://forum.unity.com/threads/local-android-save-data.130740/
    public static void WriteArrayToFile(string[] str, string filename)
    {

#if !WEB_BUILD

        string path = PathForDocumentsFile(filename);
        File.WriteAllLines(path, str);

#endif

    }

    // Code referenced: https://forum.unity.com/threads/local-android-save-data.130740/
    public static void WriteDatatoFile(string str, int pos, char delim, string filename)
    {

#if !WEB_BUILD

        string[] data = ReadArrayFromFile(filename, delim);

        if (data != null)
        {
            data[pos] = str;

            string path = PathForDocumentsFile(filename);
            File.WriteAllLines(path, data);
        }
        else
        {
            WriteStringToFile(str, filename);
        }

#endif

    }

    // Code referenced: https://forum.unity.com/threads/local-android-save-data.130740/
    public static string ReadStringFromFile(string filename)
    {

#if !WEB_BUILD

        string path = PathForDocumentsFile(filename);

        if (File.Exists(path))
        {
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(file);

            string str = null;
            str = sr.ReadLine();

            sr.Close();
            file.Close();

            return str;
        }

        else
        {
            return null;
        }

#else

        return null;

#endif
    }

    // Code referenced: https://forum.unity.com/threads/local-android-save-data.130740/
    public static string[] ReadArrayFromFile(string filename, char delim)
    {

#if !WEB_BUILD

        string path = PathForDocumentsFile(filename);

        if (File.Exists(path))
        {
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(file);

            string str = null;
            str = sr.ReadLine();

            sr.Close();
            file.Close();

            return str.Split(delim);
        }

        else
        {
            return null;
        }

#else

        return null;

#endif
    }

    // Code referenced: https://forum.unity.com/threads/local-android-save-data.130740/
    public static string PathForDocumentsFile(string filename)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Path.Combine(path, "Documents"), filename);
        }

        else if (Application.platform == RuntimePlatform.Android)
        {
            string path = Application.persistentDataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }

        else
        {
            string path = Application.dataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
    }
}
