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

        public double XAxisSpeedForSetupMoves { get; set; }
        public SliderStateInfo State { get; set; }

        /// <summary>
        /// The value from the device indicating it is ready for the next command
        /// </summary>
        public string ASCII_StatusPrefix_READY_FOR_COMMAND { get; set; }

        public string ASCII_StatusPrefix_Version { get; set; }
        public string ASCII_StatusPrefix_PanoDelayBetweenPics { get; set; }
        public string ASCII_StatusPrefix_PanoAngleBetweenPics { get; set; }
        public string ASCII_StatusPrefix_KeyframeCurrentIndex { get; set; }

        public string ASCII_Command_TriggerShutter { get; set; }
        public string ASCII_CommandPrefix_EXECUTE_Moves { get; set; }
        public string ASCII_Command_ADD_Position { get; set; }
        public string ASCII_Command_STEP_Forward { get; set; }
        public string ASCII_Command_STEP_Backward { get; set; }
        public string ASCII_Command_JUMP_To_Start { get; set; }
        public string ASCII_Command_JUMP_To_End { get; set; }
        public string ASCII_Command_EDIT_CurrentPosInArray { get; set; }
        public string ASCII_Command_ADD_Delay { get; set; }
        public string ASCII_Command_EDIT_Delay { get; set; }
        public string ASCII_Command_CLEAR_Array { get; set; }
        public string ASCII_Command_PANORAMICLAPSE_Start { get; set; }
        public string ASCII_CommandPrefix_ANGLE_BetweenPics { get; set; }
        public string ASCII_CommandPrefix_DELAY_BetweenPics { get; set; }
        public string ASCII_CommandPrefix_TIMELAPSE_StartWithXPics { get; set; }
        public string ASCII_CommandPrefix_ORBIT_InterceptPoint { get; set; }
        public string ASCII_Command_CALCULATE_InterceptPointOf1and2 { get; set; }
        public string ASCII_CommandPrefix_SCALE_Speed { get; set; }

        public Isaac879SliderControl()
        {
            XAxisSpeedForSetupMoves = 170;
            State = new SliderStateInfo();
            State.SystemEnabled = true;

            ASCII_StatusPrefix_READY_FOR_COMMAND = "--";

            ASCII_Command_ToggleEnable = "e";
            ASCII_Command_AutoHome = "A";
            ASCII_Command_GetStatus = "R";

            ASCII_StatusPrefix_Battery = "Battery:";
            ASCII_StatusPrefix_Version = "Version:";
            ASCII_StatusPrefix_PanoDelayBetweenPics = "Panoramiclapse delay between pics:";
            ASCII_StatusPrefix_PanoAngleBetweenPics = "Angle between pics:";
            ASCII_StatusPrefix_KeyframeCurrentIndex = "Keyframe index:";

            ASCII_Command_TriggerShutter = "c";
            ASCII_CommandPrefix_EXECUTE_Moves = ";";
            ASCII_Command_ADD_Position = "#";
            ASCII_Command_STEP_Forward = ">";
            ASCII_Command_STEP_Backward = "<";
            ASCII_Command_JUMP_To_Start = "[";
            ASCII_Command_JUMP_To_End = "]";
            ASCII_Command_EDIT_CurrentPosInArray = "E";
            ASCII_Command_ADD_Delay = "d";
            ASCII_Command_EDIT_Delay = "D";
            ASCII_Command_CLEAR_Array = "C";
            ASCII_Command_PANORAMICLAPSE_Start = "L";
            ASCII_CommandPrefix_ANGLE_BetweenPics = "b";
            ASCII_CommandPrefix_DELAY_BetweenPics = "B";
            ASCII_CommandPrefix_TIMELAPSE_StartWithXPics = "l";
            ASCII_CommandPrefix_ORBIT_InterceptPoint = "@";
            ASCII_Command_CALCULATE_InterceptPointOf1and2 = "T";
            ASCII_CommandPrefix_SCALE_Speed = "W";

            ///NOTE: Not implemented 
            ///- Set homing mode (0: No homing, 1: Slider, 2: Pan & Tilt, 3: All axis) (H)
            ///- Set Step Mode (m)

            DeviceComm = new SocketManager("Dev B");
            DeviceComm.ASCII_READY_FOR_NEXT_COMMAND_INDICATOR = ASCII_StatusPrefix_READY_FOR_COMMAND;

            X_Axis = new SliderAxis(DeviceComm)
            {
                Name = "Slider",
                ASCII_CommandPrefix_Invert = "j",
                ASCII_CommandPrefix_Position = "x",
                ASCII_CommandPrefix_MaxSpeed = "X",
                ASCII_CommandPrefix_AccelIncrementDelay = "w",
                ASCII_CommandPrefix_Enable = "e",
                ASCII_CommandPrefix_HomeOffset = "",
                ASCII_Command_GetStatus = ASCII_Command_GetStatus,
                ASCII_Command_ToggleEnable = "e",
                ASCII_StatusPrefix_Battery = "Battery:",
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
                ASCII_Command_GetStatus = ASCII_Command_GetStatus,
                ASCII_Command_ToggleEnable = "e",
                ASCII_StatusPrefix_Battery = "Battery:",
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
                ASCII_Command_GetStatus = ASCII_Command_GetStatus,
                ASCII_Command_ToggleEnable = "e",
                ASCII_StatusPrefix_Battery = "Battery:",
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
            State.SystemEnabled = false;
            if (X_Axis.CurrentSetup.SystemEnabled.GetValueOrDefault())
            {
                string result = DeviceComm.Send(ASCII_Command_ToggleEnable).Result;
            }
            return true;
        }

        public override bool Enable()
        {
            State.SystemEnabled = true;
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

        /// <summary>
        /// Auto home the axis
        /// </summary>
        public override async void Recalibrate()
        {
            string result = await DeviceComm.Send(ASCII_Command_AutoHome);
            UpdateCurrentPositionFromDevice();
        }

        public override void HandleBatteryLowEvent()
        {
            Shutdown();
        }

        public override void ParseBatteryLevel(string line)
        {
            bool levelUpdated = false;
            decimal? val = GetDecimalValueFromLineWithPrefix(ASCII_StatusPrefix_Battery, line);
            if (val.HasValue)
            {
                if (State.BatteryLevelPercent != val.Value)
                {
                    levelUpdated = true;
                    State.SetBatteryLevelPercent(val.Value);
                }
            }
            if (levelUpdated)
            {
                FireDeviceError("SLIDER BATTERY LOW! " + State.BatteryLevelPercent.GetValueOrDefault().ToString());
                FireBatteryLowAlert(State.BatteryLevelPercent.Value);
            }
        }

        public override void ParseStatusMessage(string line)
        {
            ParseBatteryLevel(line);
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

        private void Axis_PositionChanged(object sender, double newPosition)
        {
            this.FireDeviceInfoMessage(((SliderControlBase)sender).Name + ": Position Changed to " + newPosition.ToString());
        }

        #endregion

        #region STEP PROGRAMMING
         
        protected async Task<bool> DoCommandIfAble(string command)
        {
            bool bOK = false;
            if (State.SystemEnabled.GetValueOrDefault())
            {
                bOK = true;
                await DeviceComm.Send(command);
            }
            return bOK;
        }

        /// <summary>
        /// Trigger camera shutter
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TriggerShutter()
        {
            return await DoCommandIfAble(ASCII_Command_TriggerShutter);
        }

        /// <summary>
        /// Execute moves array
        /// </summary>
        /// <param name="repeatCount"></param>
        /// <returns></returns>
        public async Task<bool> StartExecuteMoves(int repeatCount)
        {
            return await DoCommandIfAble(ASCII_CommandPrefix_EXECUTE_Moves + repeatCount);
        }

        /// <summary>
        /// Add current position to moves array
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AddPosition(double sliderSpeed)
        {
            //sleep until we can implement the message semiphore to wait.  :(
            await DoCommandIfAble(X_Axis.ASCII_CommandPrefix_MaxSpeed + sliderSpeed);
            System.Threading.Thread.Sleep(1000);
            await DoCommandIfAble(ASCII_Command_ADD_Position);
            System.Threading.Thread.Sleep(1000);
            return await DoCommandIfAble(X_Axis.ASCII_CommandPrefix_MaxSpeed + XAxisSpeedForSetupMoves);
        }

        /// <summary>
        /// Step forward a position in the moves array
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StepForward()
        {
            return await DoCommandIfAble(ASCII_Command_STEP_Forward);
        }

        /// <summary>
        /// Step backward a position in the moves array
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StepBackward()
        {
            return await DoCommandIfAble(ASCII_Command_STEP_Backward);
        }

        /// <summary>
        /// Move to the first position in the moves array
        /// </summary>
        /// <returns></returns>
        public async Task<bool> JumpToStart()
        {
            return await DoCommandIfAble(ASCII_Command_JUMP_To_Start);
        }

        /// <summary>
        /// Move to the last position in the moves array
        /// </summary>
        /// <returns></returns>
        public async Task<bool> JumpToEnd()
        {
            return await DoCommandIfAble(ASCII_Command_JUMP_To_End);
        }

        /// <summary>
        /// Update the current position in the moves array with current position
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UpdateCurrentArrayPosition()
        {
            return await DoCommandIfAble(ASCII_Command_EDIT_CurrentPosInArray);
        }

        /// <summary>
        /// Add a delay in the moves array (ms)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AddDelay()
        {
            return await DoCommandIfAble(ASCII_Command_ADD_Delay);
        }

        /// <summary>
        /// Edit a delay at the current position in the moves array (ms)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> EditDelay()
        {
            return await DoCommandIfAble(ASCII_Command_EDIT_Delay);
        }

        /// <summary>
        /// Clear all position in the moves array
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ClearEntireArray()
        {
            return await DoCommandIfAble(ASCII_Command_CLEAR_Array);
        }

        /// <summary>
        /// Start a panoramic-lapse
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartPanoramicsTimeLapse()
        {
            return await DoCommandIfAble(ASCII_Command_PANORAMICLAPSE_Start);
        }

        /// <summary>
        /// Set angle between pictures (deg)
        /// </summary>
        /// <param name="angleDegrees"></param>
        /// <returns></returns>
        public async Task<bool> SetAngleBewteenPics(decimal angleDegrees)
        {
            return await DoCommandIfAble(ASCII_CommandPrefix_ANGLE_BetweenPics + angleDegrees);
        }

        /// <summary>
        /// Set delay between pictures (ms)
        /// </summary>
        /// <param name="delayMs"></param>
        /// <returns></returns>
        public async Task<bool> SetDelayBetweenPics(int delayMs)
        {
            return await DoCommandIfAble(ASCII_CommandPrefix_DELAY_BetweenPics + delayMs);
        }

        /// <summary>
        /// Start a time lapse with x pictures
        /// </summary>
        /// <param name="numberOfPics"></param>
        /// <returns></returns>
        public async Task<bool> StartPanoramicTimeLapse(int numberOfPics)
        {
            return await DoCommandIfAble(ASCII_CommandPrefix_TIMELAPSE_StartWithXPics + numberOfPics);
        }

        /// <summary>
        /// Move between the 2 keyframes, keeping pointed at the intercept
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartOrbitAroundIntercept(int repeatCount)
        {
            return await DoCommandIfAble(ASCII_CommandPrefix_ORBIT_InterceptPoint + repeatCount);
        }

        /// <summary>
        /// Calculate the intercept of the first 2 keyframes
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CalculateInterceptForFirst2()
        {
            return await DoCommandIfAble(ASCII_Command_CALCULATE_InterceptPointOf1and2);
        }

        /// <summary>
        /// Scale the speed of all keyframed movements
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ScaleSpeedOfAllKeyframedMoves()
        {
            return await DoCommandIfAble(ASCII_CommandPrefix_SCALE_Speed);
        }


        #endregion
    }
}
