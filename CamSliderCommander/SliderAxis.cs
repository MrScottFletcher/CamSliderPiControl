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

        public int? MinPosition { get; set; }
        public int? MaxPosition { get; set; }
        public int? HomePosition { get; set; }

        public string ASCII_CommandPrefix_ReportStatus { get; set; }
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

        private void Send_Set_MaxSpeed(int? maxSpeed)
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
            string report = DeviceComm.Send(ASCII_CommandPrefix_ReportStatus).Result;
            UpdateCurrentSetupFromDevice(report);
        }
        private void UpdateCurrentSetupFromDevice(string report)
        {
            CurrentSetup.AccelIncrementDelay = GetIntValueFromLineWithPrefix(ASCII_StatusPrefix_AccelIncrementDelay, report);
            CurrentSetup.HomeOffset = GetIntValueFromLineWithPrefix(ASCII_StatusPrefix_HomeOffset, report);
            CurrentSetup.Invert = GetBoolValueFromLineWithPrefix(ASCII_StatusPrefix_Invert, report);
            CurrentSetup.MaxSpeed = GetIntValueFromLineWithPrefix(ASCII_StatusPrefix_MaxSpeed, report);
            CurrentSetup.SystemEnabled = GetBoolValueFromLineWithPrefix(ASCII_StatusPrefix_StepersEnabled, report);

            //Might as well...
            UpdateCurrentPositionFromDevice(report);
        }
        private void UpdateCurrentPositionFromDevice()
        {
            string report = DeviceComm.Send(ASCII_CommandPrefix_ReportStatus).Result;
            UpdateCurrentPositionFromDevice(report);
        }
        private void UpdateCurrentPositionFromDevice(string report)
        {
            int? val = GetIntValueFromLineWithPrefix(ASCII_CommandPrefix_Position, report);
            if (val.HasValue) CurrentPosition.Position = val.Value;
        }

        public int? GetIntValueFromLineWithPrefix(string prefix, string report)
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

        public bool? GetBoolValueFromLineWithPrefix(string prefix, string report)
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

        private static string GetNumbers(string input)
        {
            bool isNegative = input.Contains("-");
            string val = new string(input.Where(c => char.IsDigit(c)).ToArray());
            if (isNegative) val = "-" + val;

            return val;
        }

        public string GetValueStringFromLineWithPrefix(string prefix, string report)
        {
            string[] lines = report.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string result = lines.SingleOrDefault(l => l.StartsWith(prefix));
            result = result.Replace(":", "").Replace(" ", "");

            return result;
        }

        protected bool UpdateCurrenPositionWithDesired(bool suspendRefreshFromDevice = false)
        {
            bool moved = false;
            if (DesiredPosition.Position != CurrentPosition.Position)
            {
                //do the move
                if (CurrentSetup.SystemEnabled.GetValueOrDefault())
                {
                    Send_Command_Position(DesiredPosition.Position);
                }
            }
            if (moved && !suspendRefreshFromDevice)
            {
                UpdateCurrentPositionFromDevice();
            }
            return moved;
        }


        private void Send_Command_Position(int position)
        {
            throw new NotImplementedException();
        }

        public virtual bool CancelApplyState()
        {
            return false;
        }
        public virtual void GotoPosition(int position)
        {
            if (CurrentSetup.MaxPosition.HasValue && position > CurrentSetup.MaxPosition.Value)
                DesiredPosition.Position = CurrentSetup.MaxPosition.Value;
            else if (CurrentSetup.MinPosition.HasValue && position < CurrentSetup.MinPosition.Value)
                DesiredPosition.Position = CurrentSetup.MinPosition.Value;
            else
                DesiredPosition.Position = position;
            
            UpdateCurrenPositionWithDesired();
        }

        public override bool Initialize()
        {
            throw new NotImplementedException();
        }

        public override bool Enable()
        {
            throw new NotImplementedException();
        }

        public override bool Disable()
        {
            throw new NotImplementedException();
        }

        public override bool Shutdown()
        {
            throw new NotImplementedException();
        }

        public override void Recalibrate()
        {
            throw new NotImplementedException();
        }

        public override void GoHomePosition()
        {
            throw new NotImplementedException();
        }
    }
}
