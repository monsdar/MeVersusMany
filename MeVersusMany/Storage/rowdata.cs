using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeVersusMany
{
    class rowdata
    {
        public double timestamp { get; set; }
        public double distance { get; set; }
        public uint spm { get; set; }
        public double pace { get; set; }
        public double avgpace { get; set; }
        public double calhr { get; set; }
        public uint power { get; set; }
        public uint calories { get; set; }
        public uint heartrate { get; set; }
    }
}
