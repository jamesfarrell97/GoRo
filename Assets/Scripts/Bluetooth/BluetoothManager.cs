using System.Collections.Generic;
using System;

using UnityEngine;
using TMPro;

public class PMDictionary
{
    public static string DeviceUUID = "ce060000-43e5-11e4-916c-0800200c9a66";

    public static string DeviceInfoService = "ce060010-43e5-11e4-916c-0800200c9a66";
    public static string SerialNumberStringCharacteristic = "ce060012-43e5-11e4-916c-0800200c9a66";
    public static string HardwareRevisionStringCharacteristic = "ce060013-43e5-11e4-916c-0800200c9a66";
    public static string FirmwareRevisionStringCharacteristic = "ce060014-43e5-11e4-916c-0800200c9a66";

    public static string RowingServiceUUID = "ce060030-43e5-11e4-916c-0800200c9a66";
    public static string GeneralRowingStatusCharacteristicUUID = "ce060031-43e5-11e4-916c-0800200c9a66";
    public static string GeneralRowingAdditionalStatusCharacteristicUUID = "ce060032-43e5-11e4-916c-0800200c9a66";
    public static string GeneralRowingStrokeDataCharacteristicUUID = "ce060035-43e5-11e4-916c-0800200c9a66";
    public static string MultiplexedInformationCharacteristic = "ce060080-43e5-11e4-916c-0800200c9a66";

    public static string C2PMControlServiceUUID = "ce060020-43e5-11e4-916c-0800200c9a66";
    public static string C2PMReceiveCharacteristic = "ce060021-43e5-11e4-916c-0800200c9a66";
    public static string C2PMTransmitCharacteristic = "ce060022-43e5-11e4-916c-0800200c9a66";
}

public class BluetoothManager : MonoBehaviour
{
    [SerializeField] Transform DeviceListContent;
    [SerializeField] GameObject DeviceListItemPrefab;
    [SerializeField] GameManager Launcher;

    private DeviceListItem DeviceListItem;
    private string DeviceAddress;

    private float Timeout = 0f;

    public static Dictionary<string, byte[]> RowingData { get; private set; }

    [SerializeField] TMP_Text StatusText;
    private string StatusMessage
    {
        set
        {
            BluetoothLEHardwareInterface.Log(value);
            StatusText.text = value;
        }
    }

    [SerializeField] TMP_Text ErrorText;
    private string ErrorMessage
    {
        set
        {
            BluetoothLEHardwareInterface.Log(value);
            ErrorText.text = value;
        }
    }

    private enum States
    {
        None,
        Scan,
        ScanRSSI,
        Connect,
        Subscribed,
        RequestMTU,
        Notify,
        Write,
        Read,
        Subscribe,
        Unsubscribe,
        Disconnect,
    }

    private States State;

    public static BluetoothManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ResetDevices();
        Initialize();
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

                    case States.Subscribed:
                        Subscribed();
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
            ErrorMessage = "Error: Device not found";
        }
    }

    private void Initialize()
    {
        BluetoothLEHardwareInterface.Initialize(true, false, () => {

            SetState(States.Scan, 0.1f);

        }, (error) => {

            ErrorMessage = "Initialize Error: " + error;
        });
    }

    private void Subscribed()
    {
        GameManager.Instance.ConnectedToPerformanceMonitor();
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

    private void Connect()
    {
        StatusMessage = "Connecting...";

        BluetoothLEHardwareInterface.ConnectToPeripheral(DeviceAddress, null, (address, serviceUUID) => {

            BluetoothLEHardwareInterface.StopScan();

            StatusMessage = "Connected";

            DeviceListItem.Connect();

            if (IsEqual(serviceUUID, PMDictionary.RowingServiceUUID))
            {
                //StatusMessage = "Found Service UUID";
                SetState(States.RequestMTU, 3f);
            }

        }, null);
    }

    private void RequestMTU()
    {
        StatusMessage = "Loading...";

        BluetoothLEHardwareInterface.RequestMtu(DeviceAddress, 247, (address, newMTU) => {

            //StatusMessage = "MTU set to " + newMTU.ToString();
            SetState(States.Subscribe, 2f);
        });
    }

    private void WriteCharacteristic()
    {
        //StatusMessage = "Writing characteristic...";

        byte[] data = CSAFECommand.Write(new string[] { "CSAFE_STATUS_CMD" }).ToArray();

        BluetoothLEHardwareInterface.WriteCharacteristic(DeviceAddress, PMDictionary.C2PMControlServiceUUID, PMDictionary.C2PMReceiveCharacteristic, data, data.Length, true, (characteristicUUID1) => {

            BluetoothLEHardwareInterface.Log("Write Succeeded");

            //StatusMessage = "Write Succeeded";

            SetState(States.Write, 1f);
        });
    }

    private void ReadCharacteristic()
    {
        //StatusMessage = "Reading characteristics...";

        BluetoothLEHardwareInterface.ReadCharacteristic(DeviceAddress, PMDictionary.RowingServiceUUID, PMDictionary.GeneralRowingStatusCharacteristicUUID, (characteristicUUID, rawBytes) => {

            BluetoothLEHardwareInterface.Log("Read Succeeded");

            //StatusMessage = "Read Succeeded";

            SetState(States.Read, 1f);
        });
    }

    private List<string> MultiplexedCharacteristics = new List<string>() { "31", "32", "33", "35", "36", "37", "38", "39", "3A", "3B", "3C" };

    private void SubscribeCharacteristic()
    {
        //StatusMessage = "Subscribe characteristics...";

        BluetoothLEHardwareInterface.SubscribeCharacteristic(DeviceAddress, PMDictionary.C2PMControlServiceUUID, PMDictionary.C2PMTransmitCharacteristic, (action1) =>
        {
            BluetoothLEHardwareInterface.SubscribeCharacteristic(DeviceAddress, PMDictionary.RowingServiceUUID, PMDictionary.MultiplexedInformationCharacteristic, (action2) =>
            {
                // Successfully subscribed to updates
                SetState(States.Subscribed, 1f);

            }, (characteristicUUID, rawBytes) =>
            {
                // Convert first byte to hex
                string key = Convert.ToInt32(rawBytes[0]).ToString("X");

                // If valid multiplexed data
                if (MultiplexedCharacteristics.Contains(key))
                {
                    // Process into seperate arrays
                    ProcessData(rawBytes);
                }
            });

        }, (characteristicUUID, rawBytes) =>
        {
            // Convert first byte to hex
            string key = Convert.ToInt32(rawBytes[0]).ToString("X");

            // Update RowingData
            RowingData.Add(key, rawBytes);
        });
    }

    private void Disconnect()
    {
        StatusMessage = "Disconnecting...";

        BluetoothLEHardwareInterface.DisconnectPeripheral(DeviceAddress, (disconnectAddress) => {

            StatusMessage = "Disconnected";

            BluetoothLEHardwareInterface.DeInitialize(() => {

                DeviceListItem.Disconnect();
                DeviceAddress = null;

                ResetList();
                ResetDevices();
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

    public static byte[] RowingStatusData = new byte[19];
    public static byte[] RowingStatusData1 = new byte[19];
    public static byte[] RowingStatusData2 = new byte[18];

    public static byte[] StrokeData = new byte[18];
    public static byte[] StrokeData1 = new byte[17];

    public static byte[] SplitIntervalData = new byte[18];
    public static byte[] SplitIntervalData1 = new byte[18];

    public static byte[] HeartRateData = new byte[6];

    public static byte[] EndOfWorkoutData = new byte[18];
    public static byte[] EndOfWorkoutData1 = new byte[18];
    public static byte[] EndOfWorkoutData2 = new byte[10];

    private void ProcessData(byte[] data)
    {
        const byte RowingStatus0 = 0x031;
        const byte RowingStatus1 = 0x032;
        const byte RowingStatus2 = 0x033;

        const byte Stroke = 0x035;
        const byte Stroke1 = 0x036;

        const byte SplitInterval = 0x037;
        const byte SplitInterval1 = 0x038;

        const byte HeartRate = 0x03B;

        const byte EndOfWorkout = 0x039;
        const byte EndOfWorkout1 = 0x03A;
        const byte EndOfWorkout2 = 0x03C;

        int Characteristic = Convert.ToInt32(data[0]);

        int offset = 1;
        int length = data.Length - 1;

        switch (Characteristic)
        {
            case RowingStatus0:
                RowingStatusData = HelperFunctions.SubArray(data, offset, length);
                break;

            case RowingStatus1:
                RowingStatusData1 = HelperFunctions.SubArray(data, offset, length);
                break;

            case RowingStatus2:
                RowingStatusData2 = HelperFunctions.SubArray(data, offset, length);
                break;

            case Stroke:
                StrokeData = HelperFunctions.SubArray(data, offset, length);
                break;

            case Stroke1:
                StrokeData1 = HelperFunctions.SubArray(data, offset, length);
                break;

            case SplitInterval:
                SplitIntervalData = HelperFunctions.SubArray(data, offset, length);
                break;

            case SplitInterval1:
                SplitIntervalData1 = HelperFunctions.SubArray(data, offset, length);
                break;

            case HeartRate:
                HeartRateData = HelperFunctions.SubArray(data, offset, length);
                break;

            case EndOfWorkout:
                EndOfWorkoutData = HelperFunctions.SubArray(data, offset, length);
                break;

            case EndOfWorkout1:
                EndOfWorkoutData1 = HelperFunctions.SubArray(data, offset, length);
                break;

            case EndOfWorkout2:
                EndOfWorkoutData2 = HelperFunctions.SubArray(data, offset, length);
                break;
        }
    }
}