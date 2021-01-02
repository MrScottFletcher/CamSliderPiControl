using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamSliderCommander
{
    public class SliderAxisState
    {
        public enum StatusTypes
        {
            InError = -1,
            Unknown,
            Ready,
            Busy,
            Unavailable,
        }

        public enum StatusReasonTypes
        {
            InError = -1,
            Unknown,
            Initializing,
            Calibrating,
            GoingHome,
            Sleeping,
            Awake,
            ExecutingMove,
            ShutDown,
        }

        public string Name { get; set; }
        public string ControllerType { get; set; }
        public DateTime InitializedDateTime { get; set; }
        public string LastError { get; set; }
        public StatusReasonTypes StatusReason { get; set; }

        public SliderAxisPositionInfo CurrentPosition { get; set; }
        public SliderAxisPositionInfo DesiredPosition { get; set; }

        public SliderAxisSetupInfo Setup { get; set; }

        public bool MaxLimitDetected { get; set; }
        //public decimal MotorTemp { get; set; }
        //public bool HasLimitSensor { get; set; }
    }
}
