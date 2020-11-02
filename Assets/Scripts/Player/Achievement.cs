using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Photon.Pun;

public class Achievement : MonoBehaviour
{
    void Start()
    {
        
    }

    void Awake()
    {
        
    }

    [PunRPC]
    void GitParent(Transform achievementSlot)
    {
        Debug.Log("RPC Test");
        //this.gameObject.transform.SetParent(achievementSlot);
    }
}
