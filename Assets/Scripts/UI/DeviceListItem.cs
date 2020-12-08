using UnityEngine;

using TMPro;

public class DeviceListItem : MonoBehaviour
{
    public static int DeviceListItemID = 0;

    public int DeviceID;
    public TMP_Text DeviceName;
    public TMP_Text DeviceAddress;
    public TMP_Text DeviceStatus;

    private Blue Blue;

    public void SetUp(DeviceObject device)
    {
        Blue = FindObjectOfType<Blue>();

        DeviceID = DeviceListItemID;

        DeviceName.text = device.Name;
        DeviceAddress.text = device.Address;

        DeviceListItemID++; 
    }

    public void OnButtonClick(DeviceListItem deviceListItem)
    {
        Blue.OnButtonClick(deviceListItem);
    }

    public void OnSubscribeClick(DeviceListItem deviceListItem)
    {
        Blue.OnSubscribeClick(deviceListItem);
    }

}