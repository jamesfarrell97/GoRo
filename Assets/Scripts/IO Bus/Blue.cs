using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Bluetooth;
using UnityEngine;

public class Blue : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("1");
    }

    void Start()
    {
        Debug.Log("A");
        StartCoroutine(Setup());
        StartCoroutine(BLEScan());
    }

    IEnumerator Setup()
    {
        var requestDeviceOptions = new RequestDeviceOptions
        {
            AcceptAllDevices = true
        };
        var devices = Bluetooth.ScanForDevicesAsync(requestDeviceOptions);

        Debug.Log(devices.ToString());

        yield return null;
    }

    IEnumerator BLEScan()
    {
        Debug.Log("2");
        var client = new BluetoothClient();
        var devices = client.DiscoverDevices();
        var bluetoothClient = new BluetoothClient();

        Debug.Log("3");
        string authenticated;
        string classOfDevice;
        string connected;
        string deviceAddress;
        string deviceName;
        string installedServices;

        Debug.Log("PD: " + client.PairedDevices.First().DeviceName);
        Debug.Log("D: " + client.DiscoverDevices());
        Debug.Log("BD: " + bluetoothClient.DiscoverDevices());

        Debug.Log("4");
        foreach (BluetoothDeviceInfo device in devices)
        {
            authenticated = device.Authenticated.ToString();
            classOfDevice = device.ClassOfDevice.ToString();
            connected = device.Connected.ToString();
            deviceAddress = device.DeviceAddress.ToString();
            deviceName = device.DeviceName.ToString();
            installedServices = device.InstalledServices.ToString();

            string[] row = new string[] { authenticated, classOfDevice, connected, deviceAddress, deviceName, installedServices/*, lastSeen, lastUsed, remembered, rssi */ };

            Debug.Log("Device Name:" + row[4] + " || MAC Address:" + row[3]);
        }

        Debug.Log("5");

        Console.WriteLine("Scan Complete");

        yield return null;
    }

}

/*
    lsb_device.Items.Clear();
    BluetoothRadio.PrimaryRadio.Mode = RadioMode.Connectable;
    BluetoothClient client = new BluetoothClient();
    BluetoothDeviceInfo[] devices = client.DiscoverDevices();
    BluetoothClient bluetoothClient = new BluetoothClient();
    String authenticated;
    String classOfDevice;
    String connected;
    String deviceAddress;
    String deviceName;
    String installedServices;
    String lastSeen;
    String lastUsed;
    String remembered;
    String rssi;
    foreach (BluetoothDeviceInfo device in devices)
    {
        lbl_status.Visible = true;
        authenticated = device.Authenticated.ToString();
        classOfDevice = device.ClassOfDevice.ToString();
        connected = device.Connected.ToString();
        deviceAddress = device.DeviceAddress.ToString();
        deviceName = device.DeviceName.ToString();
        installedServices = device.InstalledServices.ToString();
        lastSeen = device.LastSeen.ToString();
        lastUsed = device.LastUsed.ToString();
        remembered = device.Remembered.ToString();
        rssi = device.Rssi.ToString();
        string[] row = new string[] { authenticated, classOfDevice, connected, deviceAddress, deviceName, installedServices, lastSeen, lastUsed, remembered, rssi };

        lsb_device.Items.Add("Device Name:" + row[4] + " || MAC Address:" + row[3]);
    }
    lbl_status.Text = "Scan completed.";
*/