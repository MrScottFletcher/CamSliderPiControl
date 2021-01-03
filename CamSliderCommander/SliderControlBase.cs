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
        public delegate void PositionChangedHandler(object sender, decimal newPosition);
        public delegate void DeviceErrorHandler(object sender, string error);
        public delegate void DeviceInfoMessageHandler(object sender, string message);
        public delegate void BatteryLowHandler(object sender, int batteryPercentage);


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
        protected void FirePositionChanged(decimal newPosition)
        {
            PositionChanged?.Invoke(this, newPosition);
        }

        protected void FireDeviceError(string msg)
        {
            DeviceError?.Invoke(this, msg);
        }

        protected void FireDeviceInfoMessage(string msg)
        {
            DeviceInfoMessage?.Invoke(this, msg);
        }

    }
}
