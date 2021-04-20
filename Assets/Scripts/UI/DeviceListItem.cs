using UnityEngine;

using TMPro;
using System;

public class DeviceListItem : MonoBehaviour
{
    public static int DeviceListItemID = 0;

    public int DeviceID;
    public TMP_Text DeviceName;
    public TMP_Text DeviceAddress;
    public bool Connected;
    
    public void SetUp(DeviceObject device)
    {
        DeviceID = DeviceListItemID;

        DeviceName.text = device.Name;
        DeviceAddress.text = device.Address;

        Connected = false;

        DeviceListItemID++;
    }

    public void OnConnectClick(DeviceListItem deviceListItem)
    {
        BluetoothManager.Instance.OnConnectClick(deviceListItem);
    }

    public void Connect()
    {
        Connected = true;
    }

    public void Disconnect()
    {
        Connected = false;
    }
}