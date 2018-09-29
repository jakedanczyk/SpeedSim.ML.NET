using System;

namespace SpeedSimML
{
    public class FleetParticle
    {
        static Random rand = new Random(); // do not want seed here, want each particle to have different random order

        public int[] defenseComposition; //the defense this fleet must attack

        public int[] fleetComposition = new int[OgameData.FleetDims];
        public double[] velocity = new double[OgameData.FleetDims];
        public long attackCost = 0;
        public int[] bestFleetComposition = new int[OgameData.FleetDims];
        public long minimalAttackCost = long.MaxValue;

        public FleetParticle(int[] i_defenseComposition)
        {
            defenseComposition = i_defenseComposition;
            for (int shipIdx = 0; shipIdx < OgameData.FleetDims; ++shipIdx)
            {
                int maxShipNumber = 10 * (OgameData.DefenseValue / OgameData.FleetUnitsTotalCosts[shipIdx]);
                fleetComposition[shipIdx] = rand.Next(0, maxShipNumber + 1);
                velocity[shipIdx] = rand.Next(-maxShipNumber, maxShipNumber);
            }
            EnsureSufficientCargoSpace();
            FindAttackCost();
            minimalAttackCost = attackCost;
            Array.Copy(fleetComposition, bestFleetComposition, OgameData.FleetDims);
        }

        public void EnsureSufficientCargoSpace()
        {
            int cargoSpace = 0;
            for (int shipIdx = 0; shipIdx < OgameData.FleetDims; ++shipIdx)
            {
                cargoSpace += fleetComposition[shipIdx] * OgameData.ShipCargoSpace[shipIdx];
            }
            //add more ships if more cargo space is needed
            if (cargoSpace < OgameData.ResourcesAtRisk)
            {
                int additionalSpaceNeeded = OgameData.ResourcesAtRisk - cargoSpace;
                int shipToAdd = rand.Next(0, OgameData.FleetDims);
                fleetComposition[shipToAdd] += additionalSpaceNeeded / OgameData.ShipCargoSpace[shipToAdd];
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
                attackCost = (int)(resultArr[0] * OgameData.ResourceRatios[0]
                            + resultArr[1] * OgameData.ResourceRatios[1]
                            + resultArr[2] * OgameData.ResourceRatios[2]);

                attackCost += (int)(FuelCost() * OgameData.ResourceRatios[2]);
            }
        }

        int FuelCost()
        {
            int fuelCost = 0;
            for (int shipIdx = 0; shipIdx < OgameData.FleetDims; ++shipIdx)
            {
                fuelCost += fleetComposition[shipIdx] * OgameData.ShipFuelUsage[shipIdx];
            }
            return fuelCost;
        }
    }
}