using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleNotificationText : MonoBehaviour
{
    [SerializeField] Text notificationText;


    //UI Output Code Reference: https://www.youtube.com/watch?v=9MsPWhqQRxo
    [SerializeField]
    IEnumerator sendNotification(string text, int time)
    {
        notificationText.text = text;
        yield return new WaitForSeconds(time);
        notificationText.text = String.Empty;
    }

}
