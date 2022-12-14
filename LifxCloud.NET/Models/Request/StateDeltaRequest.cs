using System;
using System.Collections.Generic;
using System.Text;

namespace LifxCloud.NET.Models
{
    public class StateDeltaRequest
    {
        public string power { get; set; }
        public double? hue { get; set; }
        public double? saturation { get; set; }
        public double? kelvin { get; set; }
        public double? brightness { get; set; }
        public double? duration { get; set; } = 1.0;
        public double? infrared { get; set; }
    }
}
