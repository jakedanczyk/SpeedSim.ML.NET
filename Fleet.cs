using System;
using System.Collections.Generic;
using System.Text;

namespace OgameDefenseMSO
{
    public class Fleet
    {
        public double[] ShipFractions = new double[Program.MilFleetDims];

        //public int SmallCargoCount
        //{
        //    get { return ShipCounts[0]; }
        //    set { ShipCounts[0] = value; }
        //}
        //public int LargeCargoCount
        //{
        //    get { return ShipCounts[1]; }
        //    set { ShipCounts[1] = value; }
        //}
        //public int LightFighterCount
        //{
        //    get { return ShipCounts[2]; }
        //    set { ShipCounts[2] = value; }
        //}
        //public int HeavyFighterCount
        //{
        //    get { return ShipCounts[3]; }
        //    set { ShipCounts[3] = value; }
        //}
        //public int CruiserCount
        //{
        //    get { return ShipCounts[4]; }
        //    set { ShipCounts[4] = value; }
        //}
        //public int BattleshipCount
        //{
        //    get { return ShipCounts[5]; }
        //    set { ShipCounts[5] = value; }
        //}
        //public int BomberCount
        //{
        //    get { return ShipCounts[6]; }
        //    set { ShipCounts[6] = value; }
        //}
        //public int DestroyerCount
        //{
        //    get { return ShipCounts[7]; }
        //    set { ShipCounts[7] = value; }
        //}
        //public int BattlecruiserCount
        //{
        //    get { return ShipCounts[8]; }
        //    set { ShipCounts[8] = value; }
        //}

        public void CopyFleet(Fleet sourceFleet)
        {
            Array.Copy(sourceFleet.ShipFractions, ShipFractions, Program.MilFleetDims);
        }
    }
}
