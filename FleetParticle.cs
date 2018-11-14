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
        public double[] velocity = new double[Program.MilFleetDims];

        public double milValue = Program.DefenseValue * 30.0;
        public double cargoValue = 0.4155 * Program.DefenseValue;


        public double profits = 0;
        public Fleet bestLocalFleet = new Fleet();
        public double pBestProfits = double.MinValue;

        public int consecutiveNonImproves = 0;

        public FleetParticle(Defense defense)
        {
            targetDefense = defense;
            int seedIdx = Program.InitializationCount % Program.FleetSeeds.Count;
            Program.InitializationCount++;

            int numTypes = Program.FleetSeeds[seedIdx].Count(isTypePresent => isTypePresent == true);
            List<double> initialComposition = new List<double>();
            for (int i = 0; i < numTypes - 1; i++)
            {
                initialComposition.Add(rand.NextDouble());
            }
            initialComposition.Sort();
            initialComposition.Insert(0, 0.0);
            initialComposition.Add(1.0);
            int numTypesAdded = 0;
            for (int shipIdx = 0; shipIdx < Program.MilFleetDims; ++shipIdx)
            {
                if (Program.FleetSeeds[seedIdx][shipIdx])
                {
                    fleet.ShipFractions[shipIdx] = initialComposition[numTypesAdded + 1] - initialComposition[numTypesAdded];
                    numTypesAdded++;
                    velocity[shipIdx] = rand.Next(-1, 1);
                }
                else
                {
                    fleet.ShipFractions[shipIdx] = 0.0;
                    velocity[shipIdx] = 0.0;
                }
            }
            FindOptimumScale();
            //EnsureSufficientCargoSpace();
            pBestProfits = EvaluateFleet(5);
            bestLocalFleet.CopyFleet(fleet);
        }

        double h = 1.05;
        public void FindOptimumScale()
        {
            double currentScore = EvaluateFleet(Program.DefaultTrials);
            double newScore = currentScore;
            //take initial measurment (size carried over from last time, composition modified from PSO portion)
            while(true)
            {
                //take measurement with +5% combat ship value
                milValue = milValue * h;
                double milScore = EvaluateFleet(Program.DefaultTrials);
                milValue = milValue / h;

                //take measurement with +5 % cargo ship value
                cargoValue = cargoValue * h;
                double cargoScore = EvaluateFleet(Program.DefaultTrials);
                cargoValue = cargoValue / h;

                double prevMilValue = milValue;
                milValue = Math.Clamp(
                                    milValue 
                                    + Math.Clamp(
                                                Program.LearningRate * (milScore - newScore) / ((h - 1.0) * milValue),
                                                -1 * Math.Abs((h - 1) * milValue),
                                                Math.Abs((h - 1) * milValue)
                                                ),
                                    Program.DefenseValue,
                                    50.0 * Program.DefenseValue);

                double prevCargoValue = cargoValue;
                cargoValue = Math.Clamp(
                                        cargoValue
                                        + Math.Clamp(
                                                Program.LearningRate * (cargoValue / milValue) 
                                                    * (cargoScore - newScore) / ((h - 1.0) * cargoValue), 
                                                -1 * Math.Abs((h - 1) * cargoValue),
                                                Math.Abs((h - 1) * cargoValue)
                                                ),
                                    1,
                                    5.0 * Program.DefenseValue);

                currentScore = newScore;
                newScore = EvaluateFleet(Program.DefaultTrials);
                //if new position is not an increase, but milScore and/or cargoScore was an increase, 
                //      move position to the higher of milScore or cargoScore positions
                if(newScore < (1.005 * currentScore))
                {
                    if (milScore > (1.005 * currentScore)
                        || cargoScore > (1.005 * currentScore))
                    {
                        if (milScore >= cargoScore)
                        {
                            milValue = prevMilValue * h;
                            cargoValue = prevCargoValue;
                        }
                        else
                        {
                            milValue = prevMilValue;
                            cargoValue = prevCargoValue * h;
                        }
                    }
                    else
                    {
                      break;
                    }
                }
            }
        }

        //Measures the fleet error (should change term, since we are now maximizing error(profit))
        public double EvaluateFleet(int numTrials = 1)
        {
            SpeedSimInterface.Reset();

            //This was not working, was unable to find a solution, currently these values are hardcoded into speedsimlib.dll
            //SpeedSimInterface.SetLoot(new int[] { 8767680, 2598864, 1507632 });

            SpeedSimInterface.SetSystemsApart(20);

            List<int> fleetList = new List<int>(new int[14]);
            fleetList[0] = (int)Math.Round(cargoValue / Program.FleetUnitsTotalCosts[0]); //sc
            fleetList[2] = (int)Math.Round(fleet.ShipFractions[0] * milValue / Program.FleetUnitsTotalCosts[2]); //lf
            fleetList[3] = (int)Math.Round(fleet.ShipFractions[1] * milValue / Program.FleetUnitsTotalCosts[3]); //hf
            fleetList[4] = (int)Math.Round(fleet.ShipFractions[2] * milValue / Program.FleetUnitsTotalCosts[4]); //c
            fleetList[5] = (int)Math.Round(fleet.ShipFractions[3] * milValue / Program.FleetUnitsTotalCosts[5]); //bs
            fleetList[9] = (int)Math.Round(fleet.ShipFractions[4] * milValue / Program.FleetUnitsTotalCosts[6]); //b
            fleetList[11] = (int)Math.Round(fleet.ShipFractions[5] * milValue / Program.FleetUnitsTotalCosts[7]); //d
            fleetList[13] = (int)Math.Round(fleet.ShipFractions[6] * milValue / Program.FleetUnitsTotalCosts[8]); //bc


            //convert the defense unit counts to the 21-element array needed by SpeedSim
            List<int> defList = targetDefense.DefenseCounts.ToList();
            //add the 14 ship slots
            defList.InsertRange(0, new int[14]);
            defList[1] = 6400;
            defList[10] = 12800;
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
    }
}