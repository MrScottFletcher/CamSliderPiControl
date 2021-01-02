using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace CamSliderCommander
{
    public class SocketManager
    {
        //public const string BLUETOOTH_DEVICE_NAME = "HC-05";
        //public const string BLUETOOTH_DEVICE_NAME = "Dev B";

        public string BluetoothDeviceName { get; set; }
        private StreamSocket _socket;
        private RfcommDeviceService _service;

        public SocketManager(string bluetoothDeviceName)
        {
            BluetoothDeviceName = bluetoothDeviceName;
        }

        public async Task<string> Send(string msg)
        {
            if(_socket == null)
            {
                Connect();
            }
            if(_socket == null)
            {
                throw new ApplicationException("Failed to open socket for Send " + msg);
            }
            string resultMsg = string.Empty;

            try
            {
                var writer = new DataWriter(_socket.OutputStream);

                writer.WriteString(msg + "\r\n");

                // Launch an async task to 
                //complete the write operation
                var store = writer.StoreAsync().AsTask();

                //store is an int

                //get the response back, and return it.
                string response = "";

                return response;
            }
            catch (Exception ex)
            {
                resultMsg = ex.Message;

                return "";
            }
        }

        public async void Connect()
        {
            string msg = string.Empty;

            try
            {
                var devices =
                      await DeviceInformation.FindAllAsync(
                        RfcommDeviceService.GetDeviceSelector(
                          RfcommServiceId.SerialPort));

                if (devices.Count > 0)
                {
                    var device = devices.Single(x => x.Name == BluetoothDeviceName);

                    _service = await RfcommDeviceService.FromIdAsync(
                                                            device.Id);

                    _socket = new StreamSocket();

                    await _socket.ConnectAsync(
                          _service.ConnectionHostName,
                          _service.ConnectionServiceName,
                          SocketProtectionLevel.
                          BluetoothEncryptionAllowNullAuthentication);
                }
                else
                {
                    msg = "No serial port devices found.";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
        }

        public async void Disconnect()
        {
            string msg = string.Empty;

            try
            {
                await _socket.CancelIOAsync();
                _socket.Dispose();
                _socket = null;
                _service.Dispose();
                _service = null;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
        }

        //private async void ListAvailablePorts()
        //{
        //    bool done = false;
        //    try
        //    {
        //        string aqs = SerialDevice.GetDeviceSelector();
        //        var dis = await DeviceInformation.FindAllAsync(aqs);
        //        //DeviceInformationCollection listofDevices = new DeviceInformationCollection();
        //        //listofDevices.Clear();
        //        int numDevices = dis.Count;
        //        if (numDevices != 0)
        //        {
        //            for (int i = 0; i < numDevices; i++)
        //            {
        //                string name = dis[i].Name;
        //                if ((name.Contains("USB")) || (dis[i].Id.Contains("FTDI")))
        //                {
        //                    DeviceInformation found = dis[i];
        //                }
        //            }
        //            //User selects the USB Serial device from the list
        //            //DeviceListSource.Source = listofDevices;
        //            //comPortInput.IsEnabled = true;
        //            //ConnectDevices.SelectedIndex = -1;
        //            //Actual code (see GitHub repository) here contains code to automatically connect
        //            // ..if device Id (disi.Id is listed in the Json Configuration data.
        //            // .. or if only one device connect to it.
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}



    }
}
