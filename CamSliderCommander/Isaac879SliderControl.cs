using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamSliderCommander
{
    public class Isaac879SliderControl : SliderControlBase
    {
        public SliderAxis X_Axis { get; set; }
        public SliderAxis Tilt_Axis { get; set; }
        public SliderAxis Pan_Axis { get; set; }
        public List<SliderAxis> AxisList { get; set; }
        

        public Isaac879SliderControl()
        {
            ASCII_Command_ToggleEnable = "e";
            ASCII_Command_AutoHome = "A";
            ASCII_Command_GetStatus = "R";
            ASCII_StatusPrefix_Battery = "Battery:";

            //Still need to do panoramic :(

            DeviceComm  = new SocketManager("Dev B");
            X_Axis = new SliderAxis(DeviceComm)
            {
                Name = "Slide",
                ASCII_CommandPrefix_Invert = "j",
                ASCII_CommandPrefix_Position = "x",
                ASCII_CommandPrefix_MaxSpeed = "X",
                ASCII_CommandPrefix_AccelIncrementDelay = "w",
                ASCII_CommandPrefix_Enable = "e",
                ASCII_CommandPrefix_HomeOffset = "",
                ASCII_CommandPrefix_ReportStatus = ASCII_Command_GetStatus,
                ASCII_StatusPrefix_Position = "Slider position:",
                ASCII_StatusPrefix_AccelIncrementDelay = "Slider accel delay:",
                ASCII_StatusPrefix_HomeOffset = "",
                ASCII_StatusPrefix_Invert = "Slider inversion:",
                ASCII_StatusPrefix_MaxSpeed = "Slider max speed:",
                ASCII_StatusPrefix_StepersEnabled = "Enable state:",
            };
            Pan_Axis = new SliderAxis(DeviceComm)
            {
                Name = "Pan",
                ASCII_CommandPrefix_Invert = "i",
                ASCII_CommandPrefix_Position = "p",
                ASCII_CommandPrefix_MaxSpeed = "s",
                ASCII_CommandPrefix_AccelIncrementDelay = "q",
                ASCII_CommandPrefix_Enable = "e",
                ASCII_CommandPrefix_HomeOffset = "o",
                ASCII_CommandPrefix_ReportStatus = ASCII_Command_GetStatus,
                ASCII_StatusPrefix_Position = "Pan angle:",
                ASCII_StatusPrefix_AccelIncrementDelay = "Pan accel delay:",
                ASCII_StatusPrefix_HomeOffset = "Pan offset:",
                ASCII_StatusPrefix_Invert = "Pan inversion:",
                ASCII_StatusPrefix_MaxSpeed = "Pan max speed:",
                ASCII_StatusPrefix_StepersEnabled = "Enable state:",
            };
            Tilt_Axis = new SliderAxis(DeviceComm)
            {
                Name = "Tilt",
                ASCII_CommandPrefix_Invert = "I",
                ASCII_CommandPrefix_Position = "t",
                ASCII_CommandPrefix_MaxSpeed = "S",
                ASCII_CommandPrefix_AccelIncrementDelay = "Q",
                ASCII_CommandPrefix_Enable = "e",
                ASCII_CommandPrefix_HomeOffset = "O",
                ASCII_CommandPrefix_ReportStatus = ASCII_Command_GetStatus,
                ASCII_StatusPrefix_Position = "Tilt angle:",
                ASCII_StatusPrefix_AccelIncrementDelay = "Tilt access delay:",
                ASCII_StatusPrefix_HomeOffset = "Tilt offset:",
                ASCII_StatusPrefix_Invert = "Tilt inversion:",
                ASCII_StatusPrefix_MaxSpeed = "Tilt max speed:",
                ASCII_StatusPrefix_StepersEnabled = "Enable state:",
            };

            AxisList = new List<SliderAxis>()
            {
                X_Axis, Pan_Axis, Tilt_Axis
            };
            AxisList.ForEach(a =>
            {
                a.PositionChanged += Axis_PositionChanged;
                a.MoveCompleted += Axis_MoveCompleted;
                a.DeviceInfoMessage += Axis_DeviceInfoMessage;
                a.DeviceError += Axis_DeviceError;
            });

            DeviceComm.CommError += DeviceComm_CommError;
            DeviceComm.CommInfoMessage += DeviceComm_CommInfoMessage;
        }

        public override bool Initialize()
        {
            DeviceComm.Connect();
            return true;
        }

        public override bool Disable()
        {
            if (X_Axis.CurrentSetup.SystemEnabled.GetValueOrDefault())
            {
                string result = DeviceComm.Send(ASCII_Command_ToggleEnable).Result;
            }
            return true;
        }

        public override bool Enable()
        {
            if (!X_Axis.CurrentSetup.SystemEnabled.GetValueOrDefault())
            {
                string result = DeviceComm.Send(ASCII_Command_ToggleEnable).Result;
            }
            return true;
        }

        public override async void GoHomePosition(bool suspendRefreshFromDevice = false)
        {
            //do not let each access to a status update request
            AxisList.ForEach(a => a.GoHomePosition(true));

            //might need to wait for the move to finish?
            if(!suspendRefreshFromDevice)
                UpdateCurrentPositionFromDevice();
        }

        public override async void UpdateCurrentPositionFromDevice()
        {
            string result2 = await DeviceComm.Send(ASCII_Command_GetStatus);
        }

        public override async void Recalibrate()
        {
            string result = await DeviceComm.Send(ASCII_Command_AutoHome);
            UpdateCurrentPositionFromDevice();
        }

        public override void HandleBatteryLowEvent()
        {
            throw new NotImplementedException();
        }

        public override void ParseBatteryLevel(string line)
        {
            throw new NotImplementedException();
        }

        public override void ParseStatusMessage(string line)
        {
            //throw new NotImplementedException();
        }

        public override bool Shutdown()
        {
            //Toggle the enable bit
            Disable();
            DeviceComm.Disconnect("Slider shutdown requested.");
            return true;
        }

        #region Event message passthroughs
        private void DeviceComm_CommInfoMessage(object sender, string message)
        {
            this.FireDeviceInfoMessage("DeviceComm INFO: " + message);
        }

        private void DeviceComm_CommError(object sender, string error)
        {
            this.FireDeviceInfoMessage("DeviceComm error: " + error);
        }

        private void Axis_DeviceError(object sender, string error)
        {
            this.FireDeviceError(((SliderControlBase)sender).Name + ": " + error);
        }

        private void Axis_DeviceInfoMessage(object sender, string message)
        {
            this.FireDeviceInfoMessage(((SliderControlBase)sender).Name + ": " + message);
        }

        private void Axis_MoveCompleted(object sender)
        {
            this.FireDeviceInfoMessage(((SliderControlBase)sender).Name + ": Move Completed");
        }

        private void Axis_PositionChanged(object sender, decimal newPosition)
        {
            this.FireDeviceInfoMessage(((SliderControlBase)sender).Name + ": Position Changed to " + newPosition.ToString());
        }

        #endregion

    }
}
