using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedSimML
{
    public class Unit
    {
        public string Name { get; set; }

        public int MetalCost { get; set; }

        public int CrysCost { get; set; }

        public int DeutCost { get; set; }

        /// <summary>
        /// Metal Equivalent Value, using resource ratios in OgameData.cs
        /// </summary>
        public float MEV { get; set; }

        public Unit()
        {
            MEV = MetalCost * OgameData.MetalValue
                    + CrysCost * OgameData.CrysValue
                    + DeutCost * OgameData.DeutValue;
        }
    }
}
