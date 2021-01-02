using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CamSliderPiControl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestPage1 : Page
    {
        //public const string BLUETOOTH_DEVICE_NAME = "HC-05";
        public const string BLUETOOTH_DEVICE_NAME = "Dev B";

        public TestPage1()
        {
            this.InitializeComponent();
        }
        private StreamSocket _socket;

        private RfcommDeviceService _service;

        private async void btnSend_Click(object sender,
                                         RoutedEventArgs e)
        {
            int dummy;

            if (!int.TryParse(tbInput.Text, out dummy))
            {
                tbError.Text = "Invalid input";
            }

            var noOfCharsSent = await Send(tbInput.Text);

            if (noOfCharsSent != 0)
            {
                tbError.Text = noOfCharsSent.ToString();
            }
        }
        private async Task<uint> Send(string msg)
        {
            tbError.Text = string.Empty;

            try
            {
                var writer = new DataWriter(_socket.OutputStream);

                writer.WriteString(msg + "\r\n");

                // Launch an async task to 
                //complete the write operation
                var store = writer.StoreAsync().AsTask();

                return await store;
            }
            catch (Exception ex)
            {
                tbError.Text = ex.Message;

                return 0;
            }
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            //ListAvailablePorts();

            tbError.Text = string.Empty;

            try
            {
                var devices =
                      await DeviceInformation.FindAllAsync(
                        RfcommDeviceService.GetDeviceSelector(
                          RfcommServiceId.SerialPort));

                if (devices.Count > 0)
                {
                    var device = devices.Single(x => x.Name == BLUETOOTH_DEVICE_NAME);

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
                    tbError.Text = "No serial port devices found.";
                }
            }
            catch (Exception ex)
            {
                tbError.Text = ex.Message;
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


        private async void btnDisconnect_Click(object sender,
                                             RoutedEventArgs e)
        {
            tbError.Text = string.Empty;

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
                tbError.Text = ex.Message;
            }
        }
    }
}


