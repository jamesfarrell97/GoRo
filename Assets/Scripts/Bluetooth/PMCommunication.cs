using System.Collections.Generic;

using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using System.Text;

public static class PMDictionary
{
    public static string DeviceUUID = "ce060000-43e5-11e4-916c-0800200c9a66";

    public static string RowingServiceUUID = "ce060030-43e5-11e4-916c-0800200c9a66";
    public static string GeneralRowingStatusCharacteristicUUID = "ce060031-43e5-11e4-916c-0800200c9a66";
    public static string GeneralRowingAdditionalStatusCharacteristicUUID = "ce060032-43e5-11e4-916c-0800200c9a66";
    public static string GeneralRowingStrokeDataCharacteristicUUID = "ce060080-43e5-11e4-916c-0800200c9a66";

    public static string C2PMControlServiceUUID = "ce060020-43e5-11e4-916c-0800200c9a66";
    public static string C2PMReceiveCharacteristic = "ce060021-43e5-11e4-916c-0800200c9a66";
    public static string C2PMTransmitCharacteristic = "ce060022-43e5-11e4-916c-0800200c9a66";
}

public class PMCommunication : MonoBehaviour
{
    [SerializeField] Transform DeviceListContent;
    [SerializeField] GameObject DeviceListItemPrefab;
    [SerializeField] Launcher Launcher;

    private DeviceListItem DeviceListItem;
    private string DeviceAddress;
    
    private float Timeout = 0f;

    public static byte[] RowingData { get; private set; }

    public TMP_Text StatusText;
    private string StatusMessage
    {
        set
        {
            BluetoothLEHardwareInterface.Log(value);
            StatusText.text = value;
        }
    }

    public TMP_Text ErrorText;
    private string ErrorMessage
    {
        set
        {
            BluetoothLEHardwareInterface.Log(value);
            ErrorText.text = value;
        }
    }

    enum States
    {
        None,
        Scan,
        ScanRSSI,
        Connect,
        Connected,
        RequestMTU,
        Notify,
        Write,
        Read,
        Subscribe,
        Unsubscribe,
        Disconnect,
    }

    private States State;

    private void Start()
    {
        ResetDevices();
        Initialize();
    }

    private void Initialize()
    {
        BluetoothLEHardwareInterface.Initialize(true, false, () => {

            SetState(States.Scan, 0.1f);

        }, (error) => {

            ErrorMessage = "Initialize Error: " + error;
        });
    }

    private void ScanForPM()
    {
        FoundDeviceListScript.DeviceAddressList = new List<DeviceObject>();

        string[] Concept2UUID = new string[] { PMDictionary.DeviceUUID };

        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(Concept2UUID, (address, name) => {

            DeviceObject dObj = new DeviceObject(address, name);

            FoundDeviceListScript.DeviceAddressList.Add(dObj);

            UpdateDeviceList(dObj);

        }, null);
    }

    public void OnConnectClick(DeviceListItem deviceListItem)
    {
        DeviceObject device = FoundDeviceListScript.DeviceAddressList[deviceListItem.DeviceID];

        if (device != null)
        {
            if (deviceListItem.Connected)
            {

                DeviceListItem = deviceListItem;
                DeviceAddress = device.Address;

                SetState(States.Disconnect, 3f);
            }
            else
            {
                DeviceListItem = deviceListItem;
                DeviceAddress = device.Address;

                SetState(States.Connect, 3f);
            }
        }
        else
        {
            ErrorMessage = "Device not found";
        }
    }

    private void Update()
    {
        if (Timeout > 0f)
        {
            Timeout -= Time.deltaTime;

            if (Timeout <= 0f)
            {
                Timeout = 0f;

                switch (State)
                {
                    case States.None:
                        break;

                    case States.Connected:
                        Connected();
                        break;

                    case States.Scan:
                        ScanForPM();
                        break;

                    case States.Connect:
                        Connect();
                        break;
                        
                    case States.RequestMTU:
                        RequestMTU();
                        break;

                    case States.Write:
                        WriteCharacteristic();
                        break;

                    case States.Read:
                        ReadCharacteristic();
                        break;

                    case States.Subscribe:
                        SubscribeCharacteristic();
                        break;

                    case States.Disconnect:
                        Disconnect();
                        break;
                }
            }
        }
    }

    private void Connect()
    {
        StatusMessage = "Connecting...";

        BluetoothLEHardwareInterface.ConnectToPeripheral(DeviceAddress, null, (address, serviceUUID) => {

            BluetoothLEHardwareInterface.StopScan();

            StatusMessage = "Connected";

            DeviceListItem.Connect();

            if (IsEqual(serviceUUID, PMDictionary.C2PMControlServiceUUID))
            {
                StatusMessage = "Found Service UUID";
                SetState(States.Write, 1f);
            }

        }, null);
    }

    private void Connected()
    {
        Launcher.Instance.ConnectedToPerformanceMonitor();
    }

    private void RequestMTU()
    {
        StatusMessage = "Requesting MTU";

        BluetoothLEHardwareInterface.RequestMtu(DeviceAddress, 247, (address, newMTU) => {

            StatusMessage = "MTU set to " + newMTU.ToString();
            SetState(States.Write, 1f);
        });
    }

    private void WriteCharacteristic()
    {
        StatusMessage = "Writing characteristic...";

        byte[] data = CSAFECommand.Write(new string[] { "CSAFE_GETVERSION_CMD" }).ToArray();

        BluetoothLEHardwareInterface.WriteCharacteristic(DeviceAddress, PMDictionary.C2PMControlServiceUUID, PMDictionary.C2PMReceiveCharacteristic, data, data.Length, true, (characteristicUUID1) => {

            BluetoothLEHardwareInterface.Log("Write Succeeded");

            StatusMessage = "Reading characteristics...";

            SetState(States.Read, 3f);
        });
    } 

    private void ReadCharacteristic()
    {
        StatusMessage = "Reading characteristics...";

        BluetoothLEHardwareInterface.ReadCharacteristic(DeviceAddress, PMDictionary.C2PMControlServiceUUID, PMDictionary.C2PMTransmitCharacteristic, (characteristicUUID, rawBytes) => {

            BluetoothLEHardwareInterface.Log("Read Succeeded");

            RowingData = rawBytes;

            SetState(States.Write, 3f);
        });
    }

    private void SubscribeCharacteristic()
    {
        StatusMessage = "Subscribe characteristics...";

        BluetoothLEHardwareInterface.SubscribeCharacteristic(DeviceAddress, PMDictionary.C2PMControlServiceUUID, PMDictionary.C2PMTransmitCharacteristic, (x) => {

            SetState(States.Subscribe, 1f);

        }, (characteristicUUID, rawBytes) => {

                BluetoothLEHardwareInterface.Log("Read Succeeded");

                var x = CSAFECommand.Read(rawBytes);

                SetState(States.Write, 1f);
            }
        );

        SetState(States.Subscribe, 1f);
    }

    private void Disconnect()
    {
        StatusMessage = "Disconnecting...";

        BluetoothLEHardwareInterface.DisconnectPeripheral(DeviceAddress, (disconnectAddress) => {

            StatusMessage = "Disconnected";

            BluetoothLEHardwareInterface.DeInitialize(() => {

                DeviceListItem.Disconnect();
                DeviceAddress = null;

                SetState(States.None, 1f);
            });
        });
    }

    private string FullUUID(string uuid)
    {
        if (uuid.Length == 4)
            return "ce06" + uuid + "-43e5-11e4-916c-0800200C9a66";

        return uuid;
    }

    private bool IsEqual(string uuid1, string uuid2)
    {
        if (uuid1.Length == 4)
            uuid1 = FullUUID(uuid1);

        if (uuid2.Length == 4)
            uuid2 = FullUUID(uuid2);

        return (uuid1.ToUpper().Equals(uuid2.ToUpper()));
    }

    private void SetState(States state, float timeout)
    {
        State = state;
        Timeout = timeout;
    }

    private void UpdateDeviceList(DeviceObject dObj)
    {
        Instantiate(DeviceListItemPrefab, DeviceListContent).GetComponent<DeviceListItem>().SetUp(dObj);
    }

    private void ResetList()
    {
        DeviceListItem.DeviceListItemID = 0;
        foreach (Transform transform in DeviceListContent)
        {
            Destroy(transform.gameObject);
        }
    }

    private void ResetDevices()
    {
        StatusMessage = "Scanning...";
        SetState(States.Scan, 1f);

        ResetList();
    }
}