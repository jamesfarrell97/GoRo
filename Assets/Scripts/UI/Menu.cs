using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code referenced: https://www.youtube.com/watch?v=zPZK7C5_BQo
//
//
//
public class Menu : MonoBehaviour
{
    public string menuName;
    public bool open;

    public void Open()
    {
        open = true;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        open = false;
        gameObject.SetActive(false);
    }
}
