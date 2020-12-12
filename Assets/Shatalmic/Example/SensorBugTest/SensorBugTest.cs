using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SensorBugTest : MonoBehaviour
{
	public string DeviceName = "SensorBug";

	public Text AccelerometerText;
	public Text SensorBugStatusText;

	public GameObject PairingMessage;
	public GameObject TopPanel;
	public GameObject MiddlePanel;

	public class Characteristic
	{
		public string ServiceUUID;
		public string CharacteristicUUID;
		public bool Found;
	}

	public static List<Characteristic> Characteristics = new List<Characteristic>
	{
		new Characteristic { ServiceUUID = "9DC84838-7619-4F09-A1CE-DDCF63225B10", CharacteristicUUID = "9DC84838-7619-4F09-A1CE-DDCF63225B11", Found = false },
		new Characteristic { ServiceUUID = "9DC84838-7619-4F09-A1CE-DDCF63225B10", CharacteristicUUID = "9DC84838-7619-4F09-A1CE-DDCF63225B12", Found = false },
		new Characteristic { ServiceUUID = "3188AC28-72D4-4006-BD96-C6C4BC6153A0", CharacteristicUUID = "3188AC28-72D4-4006-BD96-C6C4BC6153A1", Found = false },
	};

	public Characteristic ConfigureAccelerometer = Characteristics[0];
	public Characteristic SubscribeAccelerometer = Characteristics[1];
	public Characteristic PairingManagementStatus = Characteristics[2];

	public bool AllCharacteristicsFound { get { return !(Characteristics.Where (c => c.Found == false).Any ()); } }
	public Characteristic GetCharacteristic (string serviceUUID, string characteristicsUUID)
	{
		return Characteristics.Where (c => IsEqual (serviceUUID, c.ServiceUUID) && IsEqual (characteristicsUUID, c.CharacteristicUUID)).FirstOrDefault ();
	}

	enum States
	{
		None,
		Scan,
		Connect,
		ReadPairingStatus,
		WaitPairingStatus,
		ConfigureAccelerometer,
		SubscribeToAccelerometer,
		SubscribingToAccelerometer,
		Disconnect,
		Disconnecting,
	}

	private bool _connected = false;
	private float _timeout = 0f;
	private States _state = States.None;
	private string _deviceAddress;
	private bool _pairing = false;

	private byte[] _accelerometerConfigureBytes = new byte[] { 0x01, 0x01 };

	string SensorBugStatusMessage
	{
		set
		{
			if (!string.IsNullOrEmpty(value))
				BluetoothLEHardwareInterface.Log (value);
			if (SensorBugStatusText != null)
				SensorBugStatusText.text = value;
		}
	}

	void Reset ()
	{
		_connected = false;
		_timeout = 0f;
		_state = States.None;
		_deviceAddress = null;

		if (!_pairing)
		{
			PairingMessage.SetActive (true);
			TopPanel.SetActive (false);
			MiddlePanel.SetActive (false);

			SensorBugStatusMessage = "";
		}

		_pairing = false;
	}

	void SetState (States newState, float timeout)
	{
		_state = newState;
		_timeout = timeout;
	}

	void StartProcess ()
	{
		Reset ();
		BluetoothLEHardwareInterface.Initialize (true, false, () => {

			SetState (States.Scan, 0.1f);

		}, (error) => {

			if (_state == States.SubscribingToAccelerometer)
			{
				_pairing = true;
				SensorBugStatusMessage = "Pairing to SensorBug";

				// if we get an error when trying to subscribe to the SensorBug it is
				// most likely because we just paired with it. Right after pairing you
				// have to disconnect and reconnect before being able to subscribe.
				SetState (States.Disconnect, 0.1f);
			}

			BluetoothLEHardwareInterface.Log ("Error: " + error);
		});
	}

	// Use this for initialization
	void Start ()
	{
		StartProcess ();
	}

	// Update is called once per frame
	void Update ()
	{
		if (_timeout > 0f)
		{
			_timeout -= Time.deltaTime;
			if (_timeout <= 0f)
			{
				_timeout = 0f;

				switch (_state)
				{
				case States.None:
					break;

				case States.Scan:
					BluetoothLEHardwareInterface.ScanForPeripheralsWithServices (null, (address, deviceName) => {

						if (deviceName.Contains (DeviceName))
						{
							SensorBugStatusMessage = "Found a SensorBug";

							BluetoothLEHardwareInterface.StopScan ();

							PairingMessage.SetActive (false);
							TopPanel.SetActive (true);

							// found a device with the name we want
							// this example does not deal with finding more than one
							_deviceAddress = address;
							SetState (States.Connect, 0.5f);
						}

					}, null, true);
					break;

				case States.Connect:
					SensorBugStatusMessage = "Connecting to SensorBug...";

					BluetoothLEHardwareInterface.ConnectToPeripheral (_deviceAddress, null, null, (address, serviceUUID, characteristicUUID) => {

						var characteristic = GetCharacteristic (serviceUUID, characteristicUUID);
						if (characteristic != null)
						{
							BluetoothLEHardwareInterface.Log (string.Format ("Found {0}, {1}", serviceUUID, characteristicUUID));

							characteristic.Found = true;

							if (AllCharacteristicsFound)
							{
								_connected = true;
								SetState (States.ReadPairingStatus, 3f);
							}
						}
					}, (disconnectAddress) => {
						SensorBugStatusMessage = "Disconnected from SensorBug";
						Reset ();
						SetState (States.Scan, 1f);
					});
					break;

				case States.ReadPairingStatus:
					SetState (States.WaitPairingStatus, 5f);
					BluetoothLEHardwareInterface.ReadCharacteristic (_deviceAddress, PairingManagementStatus.ServiceUUID, PairingManagementStatus.CharacteristicUUID, (characteristic, bytes) => {
						if (bytes.Length >= 9)
						{
							SensorBugStatusMessage = string.Format ("Status byte: {0}", bytes[8]);
							if ((bytes[8] & 0x01) == 0x01)
							{
								// we are paired
								// move on to configuring the accelerometer
								SetState (States.ConfigureAccelerometer, 0.5f);
							}
							else
							{
								// we are not paired
								// write the control register to trigger pairing
								BluetoothLEHardwareInterface.WriteCharacteristic (_deviceAddress, PairingManagementStatus.ServiceUUID, PairingManagementStatus.CharacteristicUUID, new byte[] { 0x00 }, 1, true, (characteristic2) => {
									SetState (States.ReadPairingStatus, 0.5f);
								});
							}
						}
						else
						{
							SensorBugStatusMessage = "Error retrieving status from pairing SensorBug";
						}
					});
					break;

				case States.WaitPairingStatus:
					// if we got here we timed out waiting for pairing status
					SetState (States.Disconnect, 0.5f);
					break;

				case States.ConfigureAccelerometer:
					SensorBugStatusMessage = "Configuring SensorBug Accelerometer...";
					BluetoothLEHardwareInterface.WriteCharacteristic (_deviceAddress, ConfigureAccelerometer.ServiceUUID, ConfigureAccelerometer.CharacteristicUUID, _accelerometerConfigureBytes, _accelerometerConfigureBytes.Length, true, (address) => {
						SensorBugStatusMessage = "Configured SensorBug Accelerometer";
						SetState (States.SubscribeToAccelerometer, 2f);
					});
					break;

				case States.SubscribeToAccelerometer:
					SetState (States.SubscribingToAccelerometer, 5f);
					SensorBugStatusMessage = "Subscribing to SensorBug Accelerometer...";
					BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress (_deviceAddress, SubscribeAccelerometer.ServiceUUID, SubscribeAccelerometer.CharacteristicUUID, null, (deviceAddress, characteristric, bytes) => {

						_state = States.None;
						MiddlePanel.SetActive (true);

						var sBytes = BitConverter.ToString (bytes);
						AccelerometerText.text = "Accelerometer: " + sBytes;
					});
					break;

				case States.SubscribingToAccelerometer:
					// if we got here it means we timed out subscribing to the accelerometer
					SetState (States.Disconnect, 0.5f);
					break;

				case States.Disconnect:
					SetState (States.Disconnecting, 5f);
					if (_connected)
					{
						BluetoothLEHardwareInterface.DisconnectPeripheral (_deviceAddress, (address) => {
							// since we have a callback for disconnect in the connect method above, we don't
							// need to process the callback here.
						});
					}
					else
					{
						Reset ();
						SetState (States.Scan, 1f);
					}
					break;

				case States.Disconnecting:
					// if we got here we timed out disconnecting, so just go to disconnected state
					Reset ();
					SetState (States.Scan, 1f);
					break;
				}
			}
		}
	}

	bool IsEqual (string uuid1, string uuid2)
	{
		return (uuid1.ToUpper ().CompareTo (uuid2.ToUpper ()) == 0);
	}
}
