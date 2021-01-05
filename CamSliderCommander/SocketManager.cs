using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        
        //Maybe not needed...
        private StreamSocketListener _listener;

        DataReader _socketReader;
        DataWriter _socketWriter;

        StringBuilder _receiveBuffer;

        public delegate void SerialCommLineReceivedHandler(object sender, string line);
        public delegate void CommErrorHandler(object sender, string error);
        public delegate void CommInfoMessageHandler(object sender, string message);

        public event SerialCommLineReceivedHandler LineReceived;
        public event CommErrorHandler CommError;
        public event CommInfoMessageHandler CommInfoMessage;

        /// <summary>
        /// The value from the device indicating it is ready for the next command.  Still need to implement this
        /// </summary>
        public string ASCII_READY_FOR_NEXT_COMMAND_INDICATOR { get; set; }

        Task _receiveLoopTask;

        public SocketManager(string bluetoothDeviceName)
        {
            BluetoothDeviceName = bluetoothDeviceName;
            _receiveBuffer = new StringBuilder();
        }

        public async Task<string> Send(string msg, int retryCount = 0)
        {
            try
            {
                if (_socket == null)
                {
                    bool bOK = await Connect();
                    if (!bOK)
                    {
                        string recoMsg = "Failed to reconnect.  Cannot retry the Send() of " + msg;
                        FireCommError(recoMsg);
                        return "Failed.";
                    }
                }
                string resultMsg = string.Empty;

                if (retryCount > 4)
                {
                    string errMsg = $"Failed to re-open the socket after {retryCount} attempts.";
                    FireCommError(errMsg);
                    return errMsg;
                }

                try
                {
                    if (LoopTaskIsDead())
                    {
                        //make another receive loop
                        ReceiveLoop();
                    }
                }
                catch (Exception exLoopTask)
                {
                    FireCommError("Error making ReceiveLoop task: " + exLoopTask.Message);
                }

                try
                {
                    // Launch an async task to complete the write operation
                    var writer = new DataWriter(_socket.OutputStream);
                    //writer.WriteUInt32((uint)msg.Length + 2);
                    writer.WriteString(msg + "\r\n");
                    var store = await writer.StoreAsync();
                    //var store = writer.StoreAsync().AsTask();

                    //dummy response for now
                    writer.DetachStream();
                    string response = "";
                    return response;
                }
                catch (Exception ex)
                {
                    if (ex.Message == "A method was called at an unexpected time. (Exception from HRESULT: 0x8000000E)")
                    {
                        resultMsg = "Bluetooth on the RaspberryPi might need a reboot (" + ex.Message + ")";
                    }
                    else if (ex.Message == "The object has been closed. (Exception from HRESULT: 0x80000013)")
                    {
                        //call connect and myself again
                        bool bOK = await Connect();
                        if (bOK)
                        {
                            retryCount++;
                            Send(msg, retryCount);
                        }
                        else
                        {
                            resultMsg = "Failed to reconnect.  Cannot retry the Send() of " + msg;
                        }
                    }
                    else
                    {
                        resultMsg = ex.Message;
                    }
                    FireCommError(resultMsg);

                    return "";
                }
            }
            catch (AggregateException aggEx)
            {
                string errMsg = $"Aggregate exception occurred in Send(). " + aggEx.Message;
                FireCommError(errMsg);
                return errMsg;
            }
            catch (Exception anyEx)
            {
                string errMsg = $"Wide exception occurred in Send(). " + anyEx.Message;
                FireCommError(errMsg);
                return errMsg;
            }
        }

        private bool LoopTaskIsDead()
        {
            return (_receiveLoopTask == null ||
                    _receiveLoopTask.IsCompleted ||
                    _receiveLoopTask.Status == TaskStatus.Faulted ||
                    _receiveLoopTask.Status == TaskStatus.Canceled ||
                    _receiveLoopTask.Status == TaskStatus.RanToCompletion);
        }

        public async Task<bool> Connect()
        {
            bool bOK = false;
            string msg = string.Empty;

            try
            {
                var devices =
                        await DeviceInformation.FindAllAsync(
                        RfcommDeviceService.GetDeviceSelector(
                            RfcommServiceId.SerialPort));


                if (devices.Count > 0)
                {
                    lock (this)
                    {
                        if (_socket != null && _socket.Information.LocalAddress == null)
                        {
                            //Old socket is sad.  Finish it off
                            Disconnect("Socket was abandoned.");
                        }
                        if (_socket == null)
                        {
                            _socket = new StreamSocket();
                            _socket.Control.KeepAlive = true;

                            //Kill the old stream reader
                            _socketReader = null;

                            //_socket.Control.QualityOfService = SocketQualityOfService.LowLatency;
                        }
                    }

                    var device = devices.Single(x => x.Name == BluetoothDeviceName);
                    if (_service == null)
                    {
                        _service = await RfcommDeviceService.FromIdAsync(device.Id);
                    }

                    //if (_listener == null)
                    //{

                    //    //To listen for a connection on the StreamSocketListener object, an app must assign the ConnectionReceived event to an 
                    //    //event handler and then call either the BindEndpointAsync or BindServiceNameAsync method to bind the StreamSocketListener 
                    //    //to a local service name or TCP port on which to listen.To listen for Bluetooth RFCOMM, the bind is to the Bluetooth Service ID.
                    //    _listener = new StreamSocketListener();
                    //    _listener.ConnectionReceived += _listener_ConnectionReceived;

                    //    //If you get an "Access is denied" error on this next line, be sure to set PrivateNetworkClientServer capability in the app manifest
                    //    await _listener.BindServiceNameAsync(_service.ServiceId.AsString());
                    //}

                    try
                    {
                        await _socket.ConnectAsync(
                              _service.ConnectionHostName,
                              _service.ConnectionServiceName,
                              SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);


                        //_socketReader = new DataReader(_socket.InputStream);
                        //_socketWriter = new DataWriter(_socket.OutputStream);

                        //start the receive loop.  Runs as an asyc task...
                        ReceiveLoop();
                        bOK = true;
                    }
                    catch (Exception ex) when ((uint)ex.HResult == 0x80070490) // ERROR_ELEMENT_NOT_FOUND
                    {
                        msg += "Please verify that you are connecting to the Camera Slider.";

                    }
                    catch (Exception ex) when ((uint)ex.HResult == 0x80072740) // WSAEADDRINUSE
                    {
                        msg += "Please verify that there is no other RFCOMM connection to the same device.";
                    }
                    catch (Exception ex) when ((uint)ex.HResult == 0x800710DF)
                    {
                        msg += "Make sure your Bluetooth Radio is on: " + ex.Message;
                    }
                }
                else
                {
                    msg += "No serial port devices found.";
                }
            }
            catch (Exception ex)
            {
                msg += ex.Message;
                FireCommError(msg);
            }
            return bOK;
        }

        private void _listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            //DataReader socketReader = new DataReader(_socket.InputStream);

            //Do not await this.  It should fire async
            //ReceiveStringLoop(socketReader, "");
        }


        void ReceiveLoop()
        {
            if (!LoopTaskIsDead())
                return;

            //...if it was dead, make a new one
            _receiveLoopTask = Task.Run(async () => {
                while (true)
                {
                    try
                    {
                        if(_socketReader == null)
                        {
                            _socketReader = new DataReader(_socket.InputStream);
                        }

                        uint buf;
                        buf = await _socketReader.LoadAsync(1);
                        if (_socketReader.UnconsumedBufferLength > 0)
                        {
                            //this chokes on weird characters like the degree symbol
                            //string s = _socketReader.ReadString(1);

                            //Do this to avoid "No mapping for the Unicode character exists in the target multi-byte code page."
                            byte[] streamContent = new byte[1];
                            _socketReader.ReadBytes(streamContent);
                            string s = Encoding.UTF8.GetString(streamContent, 0, streamContent.Length);

                            _receiveBuffer.Append(s);
                            if (s.Equals("\n") || s.Equals("\r"))
                            {
                                try
                                {
                                    //fire line received event
                                    string line = _receiveBuffer.ToString();
                                    Debug.Write("Message Received:" + line); //already has the lline feed :)
                                    FireLineReceived(line);
                                    _receiveBuffer.Clear();
                                }
                                catch (Exception exc)
                                {
                                    //Log(exc.Message);
                                    FireCommError(exc.Message);

                                }
                            }
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(0));
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (this)
                        {
                            if (_socket == null)
                            {
                                // Do not print anything here -  the user closed the socket.
                                if ((uint)ex.HResult == 0x80072745)
                                {
                                    FireCommError("Disconnect triggered by remote device");
                                }
                                else if ((uint)ex.HResult == 0x800703E3)
                                {
                                    FireCommError("The I/O operation has been aborted because of either a thread exit or an application request.");
                                }
                                else if ((uint)ex.HResult == 0x800710DF)
                                {
                                    FireCommError("Make sure your Bluetooth Radio is on: .");
                                }

                            }
                            else
                            {
                                FireCommInfoMessage("Disconnecting socket...");
                                Disconnect("Read stream failed with error: " + ex.Message);
                            }
                        }
                        //dump out of the loop and kill the task.  Do a return instead?
                        return;
                    }
                }
            });
            FireCommInfoMessage("Listening...");
        }

        public async void Disconnect(string disconnectReason)
        {
            string msg = string.Empty;

            try
            {
                if (_listener != null)
                    _listener.Dispose();
            }
            catch (Exception ex)
            {
                msg += "Disposing _listener: " + ex.Message;
            }
            try
            {
                if (_socketWriter != null)
                {
                    try
                    {
                        _socketWriter.DetachStream();
                    }
                    catch (Exception exDetachWriter)
                    {
                        msg += "Detaching _socketWriter: " + exDetachWriter.Message;
                    }
                    try
                    {
                        _socketWriter.Dispose();
                    }
                    catch (Exception exDisposeWriter)
                    {
                        msg += "Disposing _socketWriter: " + exDisposeWriter.Message;
                    }
                    _socketWriter = null;
                }
            }
            catch (Exception ex)
            {
                msg += "General error Disposing _socketWriter: " + ex.Message;
            }
            try
            {
                if (_socketReader != null)
                {
                    try
                    {
                        _socketReader.DetachStream();
                    }
                    catch (Exception exDetachReader)
                    {
                        msg += "Detaching _socketReader: " + exDetachReader.Message;
                    }
                    try
                    {
                        _socketReader.Dispose();
                    }
                    catch (Exception exDisposeReader)
                    {
                        msg += "Disposing _socketReader: " + exDisposeReader.Message;
                    }
                    _socketReader = null;
                }
            }
            catch (Exception ex)
            {
                msg += "General error Disposing _socketReader: " + ex.Message;
            }

            try
            {
                if (_socket != null)
                {
                    try
                    {
                        await _socket.CancelIOAsync();
                    }
                    catch (Exception exCancelIO)
                    {
                        msg += "Cancelling _socket IO: " + exCancelIO.Message;
                    }

                    _socket.Dispose();
                    _socket = null;
                }
            }
            catch (Exception ex)
            {
                msg += "Disposing _socket: " + ex.Message;
            }

            try
            {
                if (_service != null)
                {
                    _service.Dispose();
                    _service = null;
                }
            }
            catch (Exception ex)
            {
                msg += "Disposing _service: " + ex.Message;
            }
            FireCommInfoMessage("Disconnected. " + disconnectReason + " " + msg);
        }

        protected void FireLineReceived(string line)
        {
            LineReceived?.Invoke(this, line);
        }

        protected void FireCommError(string errMsg)
        {
            CommError?.Invoke(this, errMsg);
        }

        protected void FireCommInfoMessage(string errMsg)
        {
            CommInfoMessage?.Invoke(this, errMsg);
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
