using System.Collections.Generic;

using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class Blue : MonoBehaviour
{
    [SerializeField] Transform deviceListContent;
    [SerializeField] GameObject deviceListItemPrefab;
    public TMP_Text log;

    public List<Text> Buttons;
    public List<string> Services;
    public List<string> Characteristics;

    public void OnSearchClick()
    {
        log.text = "Log: ";

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
        log.text += "S";
        Debug.Log("S");

        DeviceObject device = FoundDeviceListScript.DeviceAddressList[listItem.DeviceID];

        string subscribedService = Services[listItem.DeviceID];
        string subscribedCharacteristic = Characteristics[listItem.DeviceID];

        log.text += "O";
        Debug.Log("O");

        if (!string.IsNullOrEmpty(subscribedService) && !string.IsNullOrEmpty(subscribedCharacteristic))
        {
            log.text += "Y";
            Debug.Log("Y");

            BluetoothLEHardwareInterface.Log("subscribing to: " + subscribedService + ", " + subscribedCharacteristic);

            log.text += "SC: " + subscribedService + ", " + subscribedCharacteristic + " ";
            Debug.Log("SC: " + subscribedService + ", " + subscribedCharacteristic + " ");

            BluetoothLEHardwareInterface.SubscribeCharacteristic(device.Address, subscribedService, subscribedCharacteristic, null, (characteristic, bytes) => {

                BluetoothLEHardwareInterface.Log("received data: " + characteristic);

                log.text += "RD: " + characteristic + " ";
                Debug.Log("RD: " + characteristic + " ");
            });
        }
        else
        {
            log.text += "!Y";
            Debug.Log("!Y");
        }
    }

    public void OnButtonClick(DeviceListItem deviceListItem)
    {
        log.text += "B";
        Debug.Log("B");

        DeviceObject device = FoundDeviceListScript.DeviceAddressList[deviceListItem.DeviceID];

        string subscribedService = Services[deviceListItem.DeviceID];
        string subscribedCharacteristic = Characteristics[deviceListItem.DeviceID];

        log.text += "C";
        Debug.Log("C");

        if (device != null)
        {
            log.text += "!N";
            Debug.Log("!N");

            if (deviceListItem.DeviceStatus.text.Equals("Connected"))
            {
                log.text += "C1";
                Debug.Log("C1");

                if (!string.IsNullOrEmpty(subscribedService) && !string.IsNullOrEmpty(subscribedCharacteristic))
                {
                    log.text += "A0";
                    Debug.Log("A0");

                    BluetoothLEHardwareInterface.UnSubscribeCharacteristic(device.Address, subscribedService, subscribedCharacteristic, (characteristic) => {

                        log.text += "A1";
                        Debug.Log("A1");

                        Services[deviceListItem.DeviceID] = null;
                        Characteristics[deviceListItem.DeviceID] = null;

                        BluetoothLEHardwareInterface.DisconnectPeripheral(device.Address, (disconnectAddress) => {

                            log.text += "A2";
                            Debug.Log("A2");

                            deviceListItem.DeviceStatus.text = "Disconnected";
                        });
                    });

                    log.text += "D1";
                    Debug.Log("D1");
                }
                else
                {
                    BluetoothLEHardwareInterface.DisconnectPeripheral(device.Address, (disconnectAddress) => {

                        deviceListItem.DeviceStatus.text = "Disconnected";
                    });

                    log.text += "D2";
                    Debug.Log("D2");
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

                log.text += "D0";
                Debug.Log("D0");
            }
        }
        else
        {
            log.text += "N";
            Debug.Log("N");
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
