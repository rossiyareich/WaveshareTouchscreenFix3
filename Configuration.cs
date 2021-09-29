using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveshareTouchscreenFix3
{
    public class Configuration
    {
        public bool MapDisplay {get; set;}
        public RectSize DisplaySize {get; set;}
        public RectSize DigitizerSize { get; set; }
        public string DeviceName { get; set;}
        public int HoldMs { get; set;}
    }
    public class RectSize
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
