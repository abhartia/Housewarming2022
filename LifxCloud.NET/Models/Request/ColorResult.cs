using System;
using System.Collections.Generic;
using System.Text;

namespace LifxCloud.NET.Models
{
    public class ColorResult
    {
        public int hue { get; set; }
        public float saturation { get; set; }
        public object brightness { get; set; }
        public object kelvin { get; set; }
    }
}
