using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedSimML
{
    public static class OgameLogic
    {
        /// <summary>
        /// Limits count of Small and Large Shield Domes to 1
        /// </summary>
        /// <param name="i_composition"></param>
        /// <returns></returns>
        public static int[] LimitShieldCount(int[] composition)
        {
            composition[composition.Length - 2] = Math.Min(composition[composition.Length - 2], 1);
            composition[composition.Length - 1] = Math.Min(composition[composition.Length - 1], 1);
            return composition;
        }

        public static int[] FuelCost(Dictionary<UnitType, int> fleet, int[] driveLevels, int speedPercent)
        {

        }

        public static int FleetSpeed(Dictionary<UnitType, int> fleet, int[] driveLevels, int speedPercent)
        {

        }
    }
}
