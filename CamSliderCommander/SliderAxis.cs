using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamSliderCommander
{
    public class SliderAxis : SliderControlBase
    {

        public SliderAxisPositionInfo CurrentPosition { get; set; }
        public SliderAxisPositionInfo DesiredPosition { get; set; }
        public SliderAxisSetupInfo CurrentSetup { get; set; }
        public SliderAxisSetupInfo DesiredSetup { get; set; }

        public string ASCII_CommandPrefix_Enable { get; set; }
        public string ASCII_CommandPrefix_Position { get; set; }
        public string ASCII_CommandPrefix_MaxSpeed { get; set; }
        public string ASCII_CommandPrefix_Invert { get; set; }
        public string ASCII_CommandPrefix_HomeOffset { get; set; }
        public string ASCII_CommandPrefix_AccelIncrementDelay { get; set; }
        public string ASCII_StatusPrefix_StepersEnabled { get; set; }
        public string ASCII_StatusPrefix_Position { get; set; }
        public string ASCII_StatusPrefix_MaxSpeed { get; set; }
        public string ASCII_StatusPrefix_Invert { get; set; }
        public string ASCII_StatusPrefix_HomeOffset { get; set; }
        public string ASCII_StatusPrefix_AccelIncrementDelay { get; set; }

        

        public SliderAxis(SocketManager socketManager) : base(socketManager)
        {
            CurrentSetup = new SliderAxisSetupInfo();
            DesiredSetup = new SliderAxisSetupInfo();
            CurrentPosition = new SliderAxisPositionInfo();
            DesiredPosition = new SliderAxisPositionInfo();
        }
        public SliderAxisState GetState()
        {
            throw new NotImplementedException("Need to implement GetState()");
        }
        public virtual void ApplySetup(SliderAxisSetupInfo setup)
        {
            DesiredSetup = setup;
            UpdateCurrenSetupWithDesired();
        }
        public virtual void ApplyPosition(SliderAxisPositionInfo pos)
        {
            DesiredPosition = pos;
            UpdateCurrenPositionWithDesired();
        }

        protected bool UpdateCurrenSetupWithDesired(bool suspendRefreshFromDevice = false)
        {
            bool updated = false;
            if (DesiredSetup.HomeOffset != CurrentSetup.HomeOffset)
            {
                Send_Set_HomeOffset(DesiredSetup.HomeOffset);
                updated = true;
            }
            if (DesiredSetup.Invert != CurrentSetup.Invert)
            {
                Send_Set_Invert(DesiredSetup.Invert);
                updated = true;
            }
            if (DesiredSetup.MaxSpeed != CurrentSetup.MaxSpeed)
            {
                Send_Set_MaxSpeed(DesiredSetup.MaxSpeed);
                updated = true;
            }
            if (DesiredSetup.AccelIncrementDelay != CurrentSetup.AccelIncrementDelay)
            {
                Send_Set_AccelIncrementDelay(DesiredSetup.AccelIncrementDelay);
                updated = true;
            }
            if (DesiredSetup.HomePosition != CurrentSetup.HomePosition)
            {
                CurrentSetup.HomePosition = DesiredSetup.HomePosition;
                //Not stored in the Isaacdevice, so no need to update
            }
            if (DesiredSetup.MaxPosition != CurrentSetup.MaxPosition)
            {
                CurrentSetup.MaxPosition = DesiredSetup.MaxPosition;
                //Not stored in the Isaacdevice, so no need to update
            }
            if (DesiredSetup.MinPosition != CurrentSetup.MinPosition)
            {
                CurrentSetup.MinPosition = DesiredSetup.MinPosition;
                //Not stored in the Isaacdevice, so no need to update
            }
            if (updated && !suspendRefreshFromDevice)
            {
                UpdateCurrentSetupFromDevice();
            }
            return updated;
        }

        private void Send_Set_MaxSpeed(double? maxSpeed)
        {
            string result = "";
            if (maxSpeed.HasValue && !String.IsNullOrEmpty(ASCII_CommandPrefix_MaxSpeed))
                result = DeviceComm.Send(ASCII_CommandPrefix_MaxSpeed + maxSpeed).Result;
        }

        private void Send_Set_Invert(bool? invert)
        {
            string result = "";
            if (invert.HasValue && !String.IsNullOrEmpty(ASCII_CommandPrefix_Invert))
            {
                int val = (invert.Value)? 1 : 0;
                result = DeviceComm.Send(ASCII_CommandPrefix_Invert + val).Result;
            }
        }

        private void Send_Set_HomeOffset(int? homeOffset)
        {
            string result = "";
            if (homeOffset.HasValue && !String.IsNullOrEmpty(ASCII_CommandPrefix_HomeOffset))
                result = DeviceComm.Send(ASCII_CommandPrefix_HomeOffset + homeOffset).Result;
        }

        private void Send_Set_AccelIncrementDelay(int? accellDelay)
        {
            string result = "";
            if (accellDelay.HasValue && !String.IsNullOrEmpty(ASCII_CommandPrefix_AccelIncrementDelay))
                result = DeviceComm.Send(ASCII_CommandPrefix_AccelIncrementDelay + accellDelay).Result;
        }

        private void UpdateCurrentSetupFromDevice()
        {
            string report = DeviceComm.Send(ASCII_Command_GetStatus).Result;
            UpdateCurrentSetupFromDevice(report);
        }
        private void UpdateCurrentSetupFromDevice(string report)
        {
            bool setupUpdated = false;
            int? delay = GetIntValueFromLineWithPrefix(ASCII_StatusPrefix_AccelIncrementDelay, report);
            if (delay.HasValue)
            {
                if (CurrentSetup.AccelIncrementDelay != delay)
                {
                    setupUpdated = true;
                    CurrentSetup.AccelIncrementDelay = delay;
                }
            }

            int? offset = GetIntValueFromLineWithPrefix(ASCII_StatusPrefix_HomeOffset, report);
            if (offset.HasValue)
            {
                if (CurrentSetup.HomeOffset != offset)
                {
                    setupUpdated = true;
                    CurrentSetup.HomeOffset = offset;
                }
            }

            bool? invert = GetBoolValueFromLineWithPrefix(ASCII_StatusPrefix_Invert, report);
            if (invert.HasValue)
            {
                if (CurrentSetup.Invert != invert)
                {
                    setupUpdated = true;
                    CurrentSetup.Invert = invert;
                }
            }

            int? maxSpeed = GetIntValueFromLineWithPrefix(ASCII_StatusPrefix_MaxSpeed, report);
            if (maxSpeed.HasValue)
            {
                if (CurrentSetup.MaxSpeed != maxSpeed)
                {
                    setupUpdated = true;
                    CurrentSetup.MaxSpeed = maxSpeed;
                }
            }

            bool? enabled = GetBoolValueFromLineWithPrefix(ASCII_StatusPrefix_StepersEnabled, report);
            if (enabled.HasValue)
            {
                if (!CurrentSetup.SystemEnabled.HasValue || CurrentSetup.SystemEnabled != enabled)
                {
                    setupUpdated = true;
                    CurrentSetup.SystemEnabled = enabled;
                }
            }

            if (setupUpdated)
                FireDeviceInfoMessage("Setup updated");

        }

        //should ask for only one refresh from the Isaac device.
        public override void UpdateCurrentPositionFromDevice()
        {
            if (!String.IsNullOrEmpty(ASCII_Command_GetStatus))
            {
                DeviceComm.Send(ASCII_Command_GetStatus);
            }
        }

        private void  UpdateCurrentPositionFromDevice(string report)
        {
            bool positionUpdated = false;
            double? val = GetDoubleValueFromLineWithPrefix(ASCII_StatusPrefix_Position, report);
            if (val.HasValue)
            {
                if (CurrentPosition.Position != val.Value)
                {
                    positionUpdated = true;
                    CurrentPosition.Position = val.Value;
                }
            }
            if (positionUpdated)
                FirePositionChanged(val.Value);
        }

 

        protected bool UpdateCurrenPositionWithDesired(bool suspendRefreshFromDevice = false)
        {
            bool moved = false;
            if (DesiredPosition.Position != CurrentPosition.Position)
            {
                //do the move
                moved = Send_Command_Position(DesiredPosition.Position, suspendRefreshFromDevice);
            }
            return moved;
        }


        private bool Send_Command_Position(double position, bool suspendRefreshFromDevice = false)
        {
            //
            if ((!CurrentSetup.SystemEnabled.HasValue || CurrentSetup.SystemEnabled.Value == true)  && !this.IsControlDisabled)
            {
                DeviceComm.Send(ASCII_CommandPrefix_Position + position.ToString());

                //Not doing this until we can do the command queueing or blocking on the serial comm
                //if(!suspendRefreshFromDevice)
                //    UpdateCurrentPositionFromDevice();

                return true;
            }
            else
            {
                return false;
            }
        }

        public override void ParseStatusMessage(string line)
        {
            UpdateCurrentSetupFromDevice(line);
            UpdateCurrentPositionFromDevice(line);
        }

        public virtual bool CancelApplyState()
        {
            return false;
        }
        public virtual void GotoPosition(double position, bool suspendRefreshFromDevice = false)
        {
            if (CurrentSetup.MaxPosition.HasValue && position > CurrentSetup.MaxPosition.Value)
                DesiredPosition.Position = CurrentSetup.MaxPosition.Value;
            else if (CurrentSetup.MinPosition.HasValue && position < CurrentSetup.MinPosition.Value)
                DesiredPosition.Position = CurrentSetup.MinPosition.Value;
            else
                DesiredPosition.Position = position;

            UpdateCurrenPositionWithDesired(suspendRefreshFromDevice);
        }

        public override bool Initialize()
        {
            throw new NotImplementedException();
        }

        public override bool Enable()
        {
            //Some devices might have independent stepper controls.  Override if neceesary
            this.IsControlDisabled = false;
            return true;
        }

        public override bool Disable()
        {
            //Some devices might have independent stepper controls.  Override if neceesary
            this.IsControlDisabled = true;
            return true;
        }

        public override bool Shutdown()
        {
            //no op for most devices.  Controller will handle it
            return true;
        }

        public override void Recalibrate()
        {
            //no op for most devices.  Controller will handle it
        }

        public override void GoHomePosition(bool suspendRefreshFromDevice = false)
        {
            GotoPosition(CurrentSetup.HomePosition.GetValueOrDefault(), suspendRefreshFromDevice);
        }

        public override void HandleBatteryLowEvent()
        {
            //no op.  It's the controller's job
        }

        public override void ParseBatteryLevel(string line)
        {
            //no op.  It's the controller's job
        }

 
    }
}
