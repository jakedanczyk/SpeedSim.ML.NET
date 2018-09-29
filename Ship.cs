using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedSimML
{
    class Ship : Unit
    {
        public int FuelUsage { get; set; } = 0;

        public int Speed { get; set; } = 0;

        public DriveType Drive { get; set; }
    }
}
