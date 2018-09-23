using System;

namespace MSOSpeedSim
{
    public class FleetParticle
    {
        static Random rand = new Random(); // do not want seed here, want each particle to have different random order

        public int[] defenseComposition; //the defense this fleet must attack

        public int[] fleetComposition = new int[Program.FleetDims];
        public double[] velocity = new double[Program.FleetDims];
        public long fleetCost = 0;
        public int[] bestFleetComposition = new int[Program.FleetDims];
        /// <summary>
        /// The cheapest fleet found in this particle that is able to win against the target Defense with losses below MAX_LOSS_PERCENT
        /// </summary>
        public long minimalFleetCost = long.MaxValue;

        public FleetParticle(int[] i_defenseComposition)
        {
            defenseComposition = i_defenseComposition;
            for (int shipIdx = 0; shipIdx < Program.FleetDims; ++shipIdx)
            {
                fleetComposition[shipIdx] = rand.Next(0, Program.FleetUnitsmaximums[shipIdx] + 1);
                velocity[shipIdx] = rand.Next(-Program.FleetUnitsmaximums[shipIdx], Program.FleetUnitsmaximums[shipIdx]);
            }
            Assessment();
            minimalFleetCost = fleetCost;
            Array.Copy(fleetComposition, bestFleetComposition, Program.FleetDims);
        }

        /// <summary>
        /// Calculate fleet cost, check if it wins against defense with less than MAX_LOSS_FRACTION losses
        /// </summary>
        public void Assessment()
        {
            CalcFleetCost();
            SpeedSimInterface.FormatDataFile(fleetComposition, defenseComposition);
            SpeedSimInterface.RunSpeedSim();
            int[] resultArr = SpeedSimInterface.GetResults();
            
            //set fleet cost to max if attacker didn't win
            if (resultArr[resultArr.Length - 1] < 100)
            {
                fleetCost = long.MaxValue;
            }
            else
            {
                int losses = (int)(resultArr[0] * Program.ResourceValueRatios[0]
                            + resultArr[1] * Program.ResourceValueRatios[1]
                            + resultArr[2] * Program.ResourceValueRatios[2]);

                fleetCost += losses * 2;
            }
        }

        void CalcFleetCost()
        {
            fleetCost = 0;
            for (int shipIdx = 0; shipIdx < Program.FleetDims; ++shipIdx)
            {
                fleetCost += fleetComposition[shipIdx] * Program.FleetUnitsTotalCosts[shipIdx];
            }
        }
    }
}