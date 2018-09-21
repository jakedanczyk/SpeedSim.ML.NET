using System;

namespace MSOSpeedSim
{
    public class FleetParticle
    {
        static Random rand = new Random(); // do not want seed here, want each particle to have different random order

        public int[] defenseComposition; //the defense this fleet must attack

        public int[] fleetComposition = new int[Program.FleetDims];
        public double[] velocity = new double[Program.FleetDims];
        public long attackCost = 0;
        public int[] bestFleetComposition = new int[Program.FleetDims];
        public long minimalAttackCost = long.MaxValue;

        public FleetParticle(int[] i_defenseComposition)
        {
            defenseComposition = i_defenseComposition;
            for (int shipIdx = 0; shipIdx < Program.FleetDims; ++shipIdx)
            {
                int maxShipNumber = 10 * (Program.DefenseValue / Program.FleetUnitsTotalCosts[shipIdx]);
                fleetComposition[shipIdx] = rand.Next(0, maxShipNumber + 1);
                velocity[shipIdx] = rand.Next(0, maxShipNumber);
            }
            EnsureSufficientCargoSpace();
            FindAttackCost();
            minimalAttackCost = attackCost;
            Array.Copy(fleetComposition, bestFleetComposition, Program.FleetDims);
        }

        public void EnsureSufficientCargoSpace()
        {
            int cargoSpace = 0;
            for (int shipIdx = 0; shipIdx < Program.FleetDims; ++shipIdx)
            {
                cargoSpace += fleetComposition[shipIdx] * Program.ShipCargoSpace[shipIdx];
            }
            //add more ships if more cargo space is needed
            if (cargoSpace < Program.ResourcesAtRisk)
            {
                int additionalSpaceNeeded = Program.ResourcesAtRisk - cargoSpace;
                int shipToAdd = rand.Next(0, Program.FleetDims);
                fleetComposition[shipToAdd] += additionalSpaceNeeded / Program.ShipCargoSpace[shipToAdd];
            }
        }

        public void FindAttackCost()
        {
            attackCost = 0;
            SpeedSimInterface.FormatDataFile(fleetComposition, defenseComposition);
            SpeedSimInterface.RunSpeedSim();
            int[] resultArr = SpeedSimInterface.GetResults();
            //set attack cost to max if attacker didn't win
            if (resultArr[resultArr.Length - 1] < 100)
            {
                attackCost = long.MaxValue;
            }
            else
            {
                attackCost = (int)(resultArr[0] * Program.ResourceValueRatios[0]
                            + resultArr[1] * Program.ResourceValueRatios[1]
                            + resultArr[2] * Program.ResourceValueRatios[2]);

                attackCost += (int)(FuelCost() * Program.ResourceValueRatios[2]);
            }
        }

        int FuelCost()
        {
            int fuelCost = 0;
            for (int shipIdx = 0; shipIdx < Program.FleetDims; ++shipIdx)
            {
                fuelCost += fleetComposition[shipIdx] * Program.ShipFuelUsage[shipIdx];
            }
            return fuelCost;
        }
    }
}