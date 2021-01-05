using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamSliderCommander
{
    public class SliderAxisSetupInfo
    {
        public int? HomePosition { get; set; }
        public double? MaxSpeed { get; set; }
        public int? MaxPosition { get; set; }
        public int? MinPosition { get; set; }

        public int? AccelIncrementDelay { get; set; }
        public bool? Invert { get; set; }
        public int? HomeOffset { get; set; }

        public bool? SystemEnabled { get; set; }
        public bool AxisDisabled { get; set; }
    }
}
