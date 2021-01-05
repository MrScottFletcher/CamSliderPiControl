using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamSliderCommander
{
    public class SliderStateInfo
    {
        public bool? SystemEnabled { get; set; }
        public decimal? BatteryLevelPercent { get; private set; }

        internal void SetBatteryLevelPercent(decimal batteryLevelPercent)
        {
            BatteryLevelPercent = batteryLevelPercent;
        }
    }
}
