////using InTheHand.Net.Bluetooth;
////using InTheHand.Net.Sockets;
////using InTheHand.Net;

//namespace dConsoleApp
//{
//    using System;
//    using System.IO;
//    using System.Linq;
//    using System.Threading;

//    using InTheHand.Net;
//    using InTheHand.Net.Bluetooth;
//    using InTheHand.Net.Sockets;
//    using InTheHand;
//    public static class BluetoothHandler
//    {
//        #region Fields
//        private static BluetoothClient _cli;
//        private static bool _isConnected = false;
//        #endregion Fields

//        #region Methods
//        public static void Close()
//        {
//            if (_cli != null)
//            {
//                _cli.Close();
//            }
//        }

//        //public void x ()
//        //{
//        //    var client = new BluetoothClient();
//        //    // Select the bluetooth device
//        //    var dlg = new SelectBluetoothDeviceDialog();
//        //    DialogResult result = dlg.ShowDialog(this);
//        //    if (result != DialogResult.OK)
//        //    {
//        //        return;
//        //    }
//        //    BluetoothDeviceInfo device = dlg.SelectedDevice;
//        //    BluetoothAddress addr = device.DeviceAddress;
//        //    Console.WriteLine(device.DeviceName);
//        //    BluetoothSecurity.PairRequest(addr, "PIN"); // set the pin here or take user input
//        //    device.SetServiceState(BluetoothService.HumanInterfaceDevice, true);
//        //    Thread.Sleep(100); // Precautionary
//        //    if (device.InstalledServices.Length == 0)
//        //    {
//        //        // handle appropriately
//        //    }
//        //    client.Connect(addr, BluetoothService.HumanInterfaceDevice);

//        //}

//        public static string GetStrFromBluetooth()
//        {
//            Stream peerStream = _cli.GetStream();

//            byte[] buffer = new byte[1000];
//            string str = string.Empty;
//            byte length = (byte)peerStream.ReadByte();
//            int byteRead = peerStream.ReadByte();

//            for (int i = 0; i < length; ++i)
//            {
//                str += (char)byteRead;
//                buffer[i] = (byte)byteRead;
//                byteRead = peerStream.ReadByte();
//            }

//            byte[] encrypted = new byte[length];
//            for (int i = 0; i < length; ++i)
//            {
//                encrypted[i] = buffer[i];
//            }

//            Console.WriteLine("Received {0} bytes", length);
//            //string decrypted = AesHandler.DecryptStringFromBytes(encrypted);
//            //Console.WriteLine("Decrypted string:" + decrypted);
//            peerStream.ReadByte();
//            peerStream.Flush();
//            Thread.Sleep(2000);

//            return decrypted;
//        }

//        public static bool IsConnected()
//        {
//            return _isConnected;
//        }

//        public static void MakeConnection(BluetoothAddress btAddress)
//        {
//            var serviceClass = BluetoothService.SerialPort;
//            if (_cli != null)
//            {
//                _cli.Close();
//            }

//            _cli = new BluetoothClient();
//            var bluetoothDeviceInfos = _cli.DiscoverDevices();
//            var deviceInfos = bluetoothDeviceInfos.ToList();
//            BluetoothDeviceInfo device = null;
//            foreach (var bluetoothDeviceInfo in deviceInfos)
//            {
//                var scannedDeviceAddress = bluetoothDeviceInfo.DeviceAddress;

//                if (scannedDeviceAddress == btAddress)
//                {
//                    device = bluetoothDeviceInfo;
//                }
//            }

//            if (device == null)
//            {
//                return;
//            }

//            var ep = new BluetoothEndPoint(device.DeviceAddress, serviceClass);

//            try
//            {
//                Guid g = new Guid("CE060000 -43E5-11E4-916C-0800200C9A66");

//                if (!device.Connected)
//                {
//                    _cli.Connect(device.DeviceAddress, g);
//                }
//            }
//            catch (System.Net.Sockets.SocketException e)
//            {
//                _cli.Close();
//                _isConnected = false;
//                return;
//            }

//            _isConnected = true;
//        }
//        #endregion Methods
//    }
//}