using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamSliderCommander
{
    public abstract class SliderControlBase
    {

        public string Name { get; set; }
        public string ControllerType { get; set; }
        public DateTime InitializedDateTime { get; set; }
        public string LastError { get; set; }
        public SliderAxisState.StatusTypes Status { get; set; }
        public SliderAxisState.StatusReasonTypes StatusReason { get; set; }

        public string ASCII_Command_AutoHome { get; set; }
        public string ASCII_Command_ToggleEnable { get; set; }
        public string ASCII_StatusPrefix_Battery { get; set; }
        public string ASCII_Command_GetStatus { get; set; }

        public SocketManager DeviceComm { get; set; }
        public CommandQueue SharedCommandQueue { get; set; }

        public bool IsControlDisabled { get; set; }

        public bool StayEngagedAfterMove { get; set; }

        public delegate void MoveCompletedHandler(object sender);
        public delegate void PositionChangedHandler(object sender, double newPosition);
        public delegate void DeviceErrorHandler(object sender, string error);
        public delegate void DeviceInfoMessageHandler(object sender, string message);
        public delegate void BatteryLowHandler(object sender, decimal batteryPercentage);


        public event MoveCompletedHandler MoveCompleted;
        public event PositionChangedHandler PositionChanged;
        public event DeviceErrorHandler DeviceError;
        public event DeviceInfoMessageHandler DeviceInfoMessage;
        public event BatteryLowHandler BatteryLow;

        public abstract bool Initialize();
        public abstract bool Enable();
        public abstract bool Disable();
        public abstract bool Shutdown();
        public abstract void Recalibrate();
        public abstract void HandleBatteryLowEvent();
        public abstract void GoHomePosition(bool suspendRefreshFromDevice);
        public abstract void ParseBatteryLevel(string line);
        public abstract void ParseStatusMessage(string line);
        public abstract void UpdateCurrentPositionFromDevice();

        public SliderControlBase()
        {
            //For initializing the main top-level controller
        }

        public SliderControlBase(SocketManager socketManager)
        {
            DeviceComm = socketManager;
            DeviceComm.LineReceived += DeviceComm_LineReceived;
        }

        private void DeviceComm_LineReceived(object sender, string line)
        {
            ParseStatusMessage(line);
        }

        protected void FireMoveCompleted()
        {
            MoveCompleted?.Invoke(this);
        }
        protected void FirePositionChanged(double newPosition)
        {
            PositionChanged?.Invoke(this, newPosition);
        }

        protected void FireDeviceError(string msg)
        {
            DeviceError?.Invoke(this, msg);
        }

        protected void FireBatteryLowAlert(decimal batteryPercentage)
        {
            BatteryLow?.Invoke(this, batteryPercentage);
        }


        protected void FireDeviceInfoMessage(string msg)
        {
            DeviceInfoMessage?.Invoke(this, msg);
        }

        public int? GetIntValueFromLineWithPrefix(string prefix, string report)
        {
            if (String.IsNullOrEmpty(prefix))
            {
                return null;
            }
            else
            {
                string val = GetValueStringFromLineWithPrefix(prefix, report);

                if (String.IsNullOrEmpty(val))
                {
                    return null;
                }
                else
                {
                    int num = 0;
                    //get rid of degrees (0176 or Alt+ 248), ms/sec, etc
                    val = GetNumbers(val);
                    if (Int32.TryParse(val, out num))
                    {
                        return num;
                    }
                    else
                    {
                        //Could not parse
                        return null;
                    }
                }
            }
        }

        public decimal? GetDecimalValueFromLineWithPrefix(string prefix, string report)
        {
            if (String.IsNullOrEmpty(prefix))
            {
                return null;
            }
            else
            {
                string val = GetValueStringFromLineWithPrefix(prefix, report);

                if (String.IsNullOrEmpty(val))
                {
                    return null;
                }
                else
                {
                    decimal num = 0;
                    //get rid of degrees (0176 or Alt+ 248), ms/sec, etc
                    val = GetNumbers(val);
                    if (decimal.TryParse(val, out num))
                    {
                        return num;
                    }
                    else
                    {
                        //Could not parse
                        return null;
                    }
                }
            }
        }

        public double? GetDoubleValueFromLineWithPrefix(string prefix, string report)
        {
            if (String.IsNullOrEmpty(prefix))
            {
                return null;
            }
            else
            {
                string val = GetValueStringFromLineWithPrefix(prefix, report);

                if (String.IsNullOrEmpty(val))
                {
                    return null;
                }
                else
                {
                    double num = 0;
                    //get rid of degrees (0176 or Alt+ 248), ms/sec, etc
                    val = GetNumbers(val);
                    if (double.TryParse(val, out num))
                    {
                        return num;
                    }
                    else
                    {
                        //Could not parse
                        return null;
                    }
                }
            }
        }


        public bool? GetBoolValueFromLineWithPrefix(string prefix, string report)
        {
            if (String.IsNullOrEmpty(prefix))
            {
                return null;
            }
            else
            {
                string val = GetValueStringFromLineWithPrefix(prefix, report);

                if (String.IsNullOrEmpty(val))
                {
                    return null;
                }
                else
                {
                    //get rid of degrees (0176 or Alt+ 248), ms/sec, etc
                    val = GetNumbers(val);
                    if (String.IsNullOrEmpty(val))
                    {
                        return null;
                    }
                    else
                    {
                        if (val.Trim() == "1")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }

        private static string GetNumbers(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return null;
            }
            else
            {
                //bool isNegative = input.Contains("-");
                string val = new string((char[])input.Where(c => char.IsDigit(c) || c == '-' || c == '.').ToArray());
                //if (isNegative) val = "-" + val;

                return val;
            }
        }

        public string GetValueStringFromLineWithPrefix(string prefix, string report)
        {
            string[] lines = report.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string result = lines.SingleOrDefault(l => l.StartsWith(prefix));
            if (!String.IsNullOrEmpty(result))
                result = result.Replace(":", "").Replace(" ", "");

            return result;
        }
    }
}
