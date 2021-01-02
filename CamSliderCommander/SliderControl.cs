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

            DeviceComm  = new SocketManager("Dev B");
            //Name, move, maxSpeed, home offset, accel delay, invert,
            X_Axis = new SliderAxis(DeviceComm)
            {
                Name = "Slide",
                ASCII_CommandPrefix_Invert = "j",
                ASCII_CommandPrefix_Position = "x",
                ASCII_CommandPrefix_MaxSpeed = "X",
                ASCII_CommandPrefix_AccelIncrementDelay = "w",
                ASCII_CommandPrefix_Enable = "",
                ASCII_CommandPrefix_HomeOffset = "",
                ASCII_StatusPrefix_Position = "",
            };
            Pan_Axis = new SliderAxis(DeviceComm)
            {
                Name = "Pan",
                ASCII_CommandPrefix_Invert = "i",
                ASCII_CommandPrefix_Position = "p",
                ASCII_CommandPrefix_MaxSpeed = "s",
                ASCII_CommandPrefix_AccelIncrementDelay = "q",
                ASCII_CommandPrefix_Enable = "",
                ASCII_CommandPrefix_HomeOffset = "o",
                ASCII_StatusPrefix_Position = ""
            };
            Tilt_Axis = new SliderAxis(DeviceComm)
            {
                Name = "Tilt",
                ASCII_CommandPrefix_Invert = "I",
                ASCII_CommandPrefix_Position = "t",
                ASCII_CommandPrefix_MaxSpeed = "S",
                ASCII_CommandPrefix_AccelIncrementDelay = "Q",
                ASCII_CommandPrefix_Enable = "",
                ASCII_CommandPrefix_HomeOffset = "O",
                ASCII_StatusPrefix_Position = ""
            };

            AxisList = new List<SliderAxis>()
            {
                X_Axis, Pan_Axis, Tilt_Axis
            };


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

        public override void GoHomePosition()
        {
            AxisList.ForEach(a => a.GotoPosition(a.HomePosition.GetValueOrDefault()));
        }

        public override bool Initialize()
        {
            DeviceComm.Connect();
            return true;
        }

        public override void Recalibrate()
        {
            string result = DeviceComm.Send(ASCII_Command_AutoHome).Result;
        }

        public override bool Shutdown()
        {
            //Toggle the enable bit
            Disable();
            return true;
        }
    }
}
