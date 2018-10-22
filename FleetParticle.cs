using System;
using System.Collections.Generic;
using System.Linq;

namespace OgameDefenseMSO
{
    public class FleetParticle
    {
        static Random rand = new Random(); // do not want seed here, want each particle to have different random order

        //the defense this fleet must attack
        public Defense targetDefense;

        public Fleet fleet = new Fleet();
        public double[] velocity = new double[Program.FleetDims];


        public double profits = 0;
        public Fleet bestLocalFleet = new Fleet();
        public double pBestProfits = double.MinValue;

        public int consecutiveNonImproves = 0;

        public FleetParticle(Defense defense)
        {
            targetDefense = defense;
            for (int shipIdx = 0; shipIdx < Program.FleetDims; ++shipIdx)
            {
                int maxShipNumber = 10 * ((int)Program.DefenseValue / Program.FleetUnitsTotalCosts[shipIdx]);
                int zeroDie = rand.Next(0, 2);
                fleet.ShipCounts[shipIdx] = zeroDie * rand.Next(0, maxShipNumber + 1);
                velocity[shipIdx] = rand.Next(-maxShipNumber, maxShipNumber);
            }
            //EnsureSufficientCargoSpace();
            pBestProfits = EvaluateFleet(5);
            bestLocalFleet.CopyFleet(fleet);
        }

        //Measures the fleet error (should change term, since we are now maximizing error(profit))
        public double EvaluateFleet(int numTrials = 1)
        {
            SpeedSimInterface.Reset();

            //This was not working, was unable to find a solution, currently these values are hardcoded into speedsimlib.dll
            //SpeedSimInterface.SetLoot(new int[] { 8767680, 2598864, 1507632 });

            SpeedSimInterface.SetSystemsApart(20);

            //convert the fleet ship counts from the 9-element array of viable ships to the 14-element array needed by SpeedSimLib
            List<int> fleetList = fleet.ShipCounts.ToList();
            //insert Deathstar slot
            fleetList.Insert(8, 0);
            //insert Solar Sat slot
            fleetList.Insert(7, 0);
            //insert colony ship, recycler, and probe slots
            fleetList.InsertRange(6, new int[] { 0, 0, 0 });

            //convert the defense unit counts to the 21-element array needed by SpeedSim
            List<int> defList = targetDefense.DefenseCounts.ToList();
            //add the 14 ship slots
            defList.InsertRange(0, new int[14]);
            defList[1] = 200;
            defList[10] = 400;
            defList.Add(1); //Small shield dome
            defList.Add(1); //Large shield dome
            SpeedSimInterface.SetFleetInt(fleetList.ToArray(), defList.ToArray());

            SpeedSimInterface.SetTechs(15, 15, 13, 17, 17, 17, 14, 14, 14);

            SpeedSimInterface.Simulate(numTrials);

            //calculate fleet profits
            profits = 0;
            //subtract losses
            profits -= (SpeedSimInterface.GetAttackMetalLoss() * Program.ResourceValueRatios[0]);
            profits -= (SpeedSimInterface.GetAttackCrystalLoss() * Program.ResourceValueRatios[1]);
            profits -= (SpeedSimInterface.GetAttackDeuteriumLoss() * Program.ResourceValueRatios[2]);

            //subtract fuel
            profits -= (SpeedSimInterface.GetFuelConsumption() * Program.ResourceValueRatios[2]);

            //add loot (must multiply by odds of winning)
            double loot = (SpeedSimInterface.GetLootMetal() * Program.ResourceValueRatios[0]);
            loot += (SpeedSimInterface.GetLootCrystal() * Program.ResourceValueRatios[1]);
            loot += (SpeedSimInterface.GetLootDeuterium() * Program.ResourceValueRatios[2]);
            float winPercentage = SpeedSimInterface.GetAttackWinPercent();
            profits += (loot * winPercentage);

            //add df
            profits += (SpeedSimInterface.GetDebrisMetal() * Program.ResourceValueRatios[0]);
            profits += (SpeedSimInterface.GetDebrisCrystal() * Program.ResourceValueRatios[1]);
            profits += (SpeedSimInterface.GetDebrisDeuterium() * Program.ResourceValueRatios[2]);

            //divide by flight time to get profits per hour
            long flightTime = SpeedSimInterface.GetFlightTime();
            double flightHours = flightTime / 3600.0;

            profits = (profits / flightHours);
            return profits;
        }

        ulong CalculateCost()
        {
            ulong fleetCost = 0;
            for (int i = 0; i < Program.FleetDims; ++i)
            {
                fleetCost += (ulong)(Program.FleetUnitsTotalCosts[i] * fleet.ShipCounts[i]);
            }
            return fleetCost;
        }

    }
}