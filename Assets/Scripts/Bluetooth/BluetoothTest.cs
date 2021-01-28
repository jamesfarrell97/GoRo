using System.Collections.Generic;

using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class Blue : MonoBehaviour
{
    [SerializeField] Transform deviceListContent;
    [SerializeField] GameObject deviceListItemPrefab;

    public List<Text> Buttons;
    public List<string> Services;
    public List<string> Characteristics;

    public void OnSearchClick()
    {
        ResetList();

        BluetoothLEHardwareInterface.Initialize(true, false, () => {
            
            FoundDeviceListScript.DeviceAddressList = new List<DeviceObject>();
            
            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) => {

                DeviceObject dObj = new DeviceObject(address, name);

                FoundDeviceListScript.DeviceAddressList.Add(dObj);
                UpdateDeviceList(dObj);

            }, null);

        }, (error) => {
            
            BluetoothLEHardwareInterface.Log("BLE Error: " + error);

        });
    }

    private void ResetList()
    {
        DeviceListItem.DeviceListItemID = 0;
        foreach (Transform transform in deviceListContent)
        {
            Destroy(transform.gameObject);
        }
    }

    public void OnSubscribeClick(DeviceListItem listItem)
    {
        DeviceObject device = FoundDeviceListScript.DeviceAddressList[listItem.DeviceID];

        string subscribedService = Services[listItem.DeviceID];
        string subscribedCharacteristic = Characteristics[listItem.DeviceID];
        
        if (!string.IsNullOrEmpty(subscribedService) && !string.IsNullOrEmpty(subscribedCharacteristic))
        {
            BluetoothLEHardwareInterface.Log("subscribing to: " + subscribedService + ", " + subscribedCharacteristic);
            
            BluetoothLEHardwareInterface.SubscribeCharacteristic(device.Address, subscribedService, subscribedCharacteristic, null, (characteristic, bytes) => {

                BluetoothLEHardwareInterface.Log("received data: " + characteristic);
            });
        }
    }

    public void OnButtonClick(DeviceListItem deviceListItem)
    {
        DeviceObject device = FoundDeviceListScript.DeviceAddressList[deviceListItem.DeviceID];

        string subscribedService = Services[deviceListItem.DeviceID];
        string subscribedCharacteristic = Characteristics[deviceListItem.DeviceID];
        
        if (device != null)
        {
            if (deviceListItem.DeviceStatus.text.Equals("Connected"))
            {
                if (!string.IsNullOrEmpty(subscribedService) && !string.IsNullOrEmpty(subscribedCharacteristic))
                {
                    BluetoothLEHardwareInterface.UnSubscribeCharacteristic(device.Address, subscribedService, subscribedCharacteristic, (characteristic) => {
                        
                        Services[deviceListItem.DeviceID] = null;
                        Characteristics[deviceListItem.DeviceID] = null;

                        BluetoothLEHardwareInterface.DisconnectPeripheral(device.Address, (disconnectAddress) => {
                            
                            deviceListItem.DeviceStatus.text = "Disconnected";
                        });
                    });
                }
                else
                {
                    BluetoothLEHardwareInterface.DisconnectPeripheral(device.Address, (disconnectAddress) => {

                        deviceListItem.DeviceStatus.text = "Disconnected";
                    });
                }
            }
            else
            {
                BluetoothLEHardwareInterface.ConnectToPeripheral(device.Address, (address) => {

                }, null, (address, service, characteristic) => {

                    if (string.IsNullOrEmpty(Services[deviceListItem.DeviceID]) && string.IsNullOrEmpty(Characteristics[deviceListItem.DeviceID]))
                    {
                        Services[deviceListItem.DeviceID] = FullUUID(service);
                        Characteristics[deviceListItem.DeviceID] = FullUUID(characteristic);
                        deviceListItem.DeviceStatus.text = "Connected";
                    }

                }, null);
            }
        }
    }

    private void UpdateDeviceList(DeviceObject dObj)
    {
        Instantiate(deviceListItemPrefab, deviceListContent).GetComponent<DeviceListItem>().SetUp(dObj);
        
        Services.Add(null);
        Characteristics.Add(null);
    }

    private void OnCharacteristic(string characteristic, byte[] bytes)
    {
        BluetoothLEHardwareInterface.Log("received: " + characteristic);
    }

    private string FullUUID(string uuid)
    {
        if (uuid.Length == 4)
            return "0000" + uuid + "-0000-1000-8000-00805f9b34fb";

        return uuid;
    }
}
