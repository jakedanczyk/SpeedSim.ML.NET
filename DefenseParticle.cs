using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.IO;

namespace OgameDefenseMSO
{
    /// <summary>
    /// Particle representing defense composition
    /// </summary>
    public class DefenseParticle
    {
        static Random rand = new Random(); // do not want seed here, want each particle to have different random order

        ulong costOfDefense = 0;

        public Defense defense = new Defense();
        public double[] velocity = new double[Program.DefenseDims];

        /// <summary>
        /// The maximum profit per hour an attacker can make by attacking
        /// </summary>
        public double error = 0;
        public Defense pBestDefense = new Defense();
        public double pBestError = 0;

        public int consecutiveNonImproves = 0;

        public DefenseParticle()
        {
            for (int unitIdx = 0; unitIdx < Program.DefenseDims; ++unitIdx)
            {
                defense.DefenseCounts[unitIdx] = rand.Next(0, Program.DefenseUnitsMaximums[unitIdx] + 1);
                velocity[unitIdx] = rand.Next(-Program.DefenseUnitsMaximums[unitIdx], Program.DefenseUnitsMaximums[unitIdx] + 1);
            }
            error = EvaluateDefense();
            pBestError = error;
            pBestDefense.CopyDefense(defense);
        }
        //int sc, int lc, int lf, int hf, int c, int bs, int colony, int rec, int probe, int b, int sat, int d, int rip, int bc, int rl, int ll, int hl, int gc, int ic, int pt, int ssd, int lsd

        public DefenseParticle(int[] targetComposition)
        {
            Array.Copy(targetComposition, defense.DefenseCounts, Program.DefenseDims);
            error = EvaluateDefense();
            pBestError = error;
            pBestDefense.CopyDefense(defense);
        }

        /// <summary>
        /// Balances the defense to have the correct total cost by picking a random structure and increasing or decreasing number of that structure
        /// Repeats processs as necessary, until defense cost is less than the value of 1 plasma turret away from the desired valued
        /// </summary>
        void BalanceDefense()
        {
            CalculateCost();
            int plasmaTurretCost = Program.DefenseUnitsTotalCosts[5];
            bool isTooExpensive = (costOfDefense - (ulong)plasmaTurretCost) > Program.DefenseValue;
            bool isTooCheap = (costOfDefense + (ulong)plasmaTurretCost) < Program.DefenseValue;
            bool isBalanced = !(isTooExpensive || isTooCheap);
            while (!isBalanced)
            {
                int unitToBalanceIdx = rand.Next(0, Program.DefenseDims);
                if (isTooExpensive)
                {
                    int numUnitsToRemove = (int)((costOfDefense - Program.DefenseValue) / (double)Program.DefenseUnitsTotalCosts[unitToBalanceIdx]);
                    defense.DefenseCounts[unitToBalanceIdx] = Math.Max(0, defense.DefenseCounts[unitToBalanceIdx] - numUnitsToRemove);
                }
                else if (isTooCheap)
                {
                    int numUnitsToAdd = (int)((Program.DefenseValue - costOfDefense) / (double)Program.DefenseUnitsTotalCosts[unitToBalanceIdx]);
                    defense.DefenseCounts[unitToBalanceIdx] = Math.Min(Program.DefenseUnitsMaximums[unitToBalanceIdx],
                                                                    numUnitsToAdd + defense.DefenseCounts[unitToBalanceIdx]);
                }
                CalculateCost();
                isTooExpensive = (costOfDefense - (ulong)plasmaTurretCost) > Program.DefenseValue;
                isTooCheap = (costOfDefense + (ulong)plasmaTurretCost) < Program.DefenseValue;
                isBalanced = !(isTooExpensive || isTooCheap);
            }
        }

        ulong CalculateCost()
        {
            costOfDefense = 0;
            for (int i = 0; i < Program.DefenseDims; ++i)
            {
                costOfDefense += (ulong)(Program.DefenseUnitsTotalCosts[i] * defense.DefenseCounts[i]);
            }
            return costOfDefense;
        }

        ///<summary>
        /// Create and run a Fleet MSO that attempts to find the optimum fleet composition to raid this defense
        ///</summary>
        public double EvaluateDefense()
        {
            BalanceDefense();
            Console.WriteLine("\tSolving for optimum fleet to raid defense composition ["
                                + String.Join(",", defense.DefenseCounts)
                                + "]");

            Random rand = new Random();
            FleetSwarm[] fleetSwarms = new FleetSwarm[Program.NumSwarms];
            for (int i = 0; i < Program.NumSwarms; ++i)
            {
                fleetSwarms[i] = new FleetSwarm(defense);
            }
            Fleet gBestFleet = new Fleet();
            double gBestProfits = double.MinValue;
            for (int i = 0; i < Program.NumSwarms; ++i)
            {
                if (fleetSwarms[i].lBestProfits > gBestProfits)
                {
                    gBestProfits = fleetSwarms[i].lBestProfits;
                    gBestFleet.CopyFleet(fleetSwarms[i].lBestFleet);
                    PrintCurrentNewBestFleetComposition(gBestFleet.ShipCounts, gBestProfits, -1);
                }
            }


            var gBestList = new List<(int, double)>
            {
                (-1, gBestProfits)
            };


            int epoch = 0;
            int epochsSinceImprovement = 0;

            while (epoch < (Program.MaxEpochsInner) && epochsSinceImprovement < 500)
            {
                ++epoch;
                ++epochsSinceImprovement;

                //if (epoch < Program.MaxEpochs && epoch % 5 == 0)
                //{
                //    Console.WriteLine("Defense[" + String.Join(",", defense) + "], "
                //                        + " Epoch " + epoch
                //                        + ", Global Best Minimal Attack Cost = " + bestGlobalMinAttackCost.ToString("F4"));
                //}

                for (int i = 0; i < Program.NumSwarms; ++i) // each swarm
                {
                    // Shuffle(sequence, rand); // move particles in random sequence
                    for (int j = 0; j < Program.NumParticles; ++j) // each particle
                    {
                        //Check if Particle dies
                        double p1 = rand.NextDouble();
                        if (fleetSwarms[i].fleetParticles[j].consecutiveNonImproves * p1 > 20)
                        {
                            fleetSwarms[i].fleetParticles[j] = new FleetParticle(defense); // new random position

                            if (fleetSwarms[i].fleetParticles[j].profits > fleetSwarms[i].lBestProfits) // new swarm best by luck?
                            {
                                fleetSwarms[i].lBestProfits = fleetSwarms[i].fleetParticles[j].pBestProfits;
                                fleetSwarms[i].lBestFleet.CopyFleet(fleetSwarms[i].fleetParticles[j].fleet);

                                if (fleetSwarms[i].fleetParticles[j].profits > gBestProfits) // if a new swarm best, maybe also a new global best?
                                {
                                    //need to repeat Defense evaluation to avoid outliers skewing results
                                    double moreAccurateProfits = fleetSwarms[i].fleetParticles[j].EvaluateFleet(20);
                                    if (moreAccurateProfits > gBestProfits)
                                    {
                                        epochsSinceImprovement = 0;

                                        gBestProfits = moreAccurateProfits;
                                        gBestFleet.CopyFleet(fleetSwarms[i].fleetParticles[j].fleet);
                                        PrintCurrentNewBestFleetComposition(gBestFleet.ShipCounts, gBestProfits, epoch);
                                        gBestList.Add((epoch, gBestProfits));
                                    }
                                }
                            }
                        }
                        // an alternative is to maintain a particle age and die with high prob after a certain age reached
                        // another option is to maintain particle health/weakness (related to either ratio of times improved / loop count
                        // or number consecutive improves or consecutive non-improves) and die with high prob when health is low 

                        double p2 = rand.NextDouble();
                        if (p2 < Program.ProbImmigrate)
                        {
                            int otherSwarm = rand.Next(0, Program.NumSwarms);
                            int otherParticle = rand.Next(0, Program.NumParticles);
                            FleetParticle tmp = fleetSwarms[i].fleetParticles[j];
                            fleetSwarms[i].fleetParticles[j] = fleetSwarms[otherSwarm].fleetParticles[otherParticle];
                            fleetSwarms[otherSwarm].fleetParticles[otherParticle] = tmp;

                            if (fleetSwarms[i].fleetParticles[j].pBestProfits > fleetSwarms[otherSwarm].lBestProfits) // new (other) swarm best?
                            {
                                fleetSwarms[otherSwarm].lBestProfits = fleetSwarms[i].fleetParticles[j].pBestProfits;
                                fleetSwarms[otherSwarm].lBestFleet.CopyFleet(fleetSwarms[i].fleetParticles[j].bestLocalFleet);
                            }
                            if (fleetSwarms[otherSwarm].fleetParticles[otherParticle].pBestProfits > fleetSwarms[i].lBestProfits) // new (curr) swarm best?
                            {
                                fleetSwarms[i].lBestProfits = fleetSwarms[otherSwarm].fleetParticles[otherParticle].pBestProfits;
                                fleetSwarms[i].lBestFleet.CopyFleet(fleetSwarms[otherSwarm].fleetParticles[otherParticle].fleet);
                            }
                            // not possible for a new global best
                        }

                        for (int k = 0; k < Program.FleetDims; ++k) // update velocity. each x position component
                        {
                            double r1 = rand.NextDouble();
                            double r2 = rand.NextDouble();
                            double r3 = rand.NextDouble();

                            fleetSwarms[i].fleetParticles[j].velocity[k] = (
                                                                            (Program.Inertia * fleetSwarms[i].fleetParticles[j].velocity[k])
                                                                            + (Program.GravityLocal * r1 * (fleetSwarms[i].fleetParticles[j].bestLocalFleet.ShipCounts[k] 
                                                                                - fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k])
                                                                                ) 
                                                                            + (Program.GravitySwarm * r2 * (fleetSwarms[i].lBestFleet.ShipCounts[k] 
                                                                                - fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k])
                                                                                ) 
                                                                            + (Program.GravityGlobal * r3 * (gBestFleet.ShipCounts[k] 
                                                                                - fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k])
                                                                                )
                                                                            );

                            //if (fleetSwarms[i].fleetParticles[j].velocity[k] < minX) // constrain velocities
                            //    fleetSwarms[i].fleetParticles[j].velocity[k] = minX;
                            //else if (fleetSwarms[i].fleetParticles[j].velocity[k] > maxX)
                            //    fleetSwarms[i].fleetParticles[j].velocity[k] = maxX;
                        }

                        for (int k = 0; k < Program.FleetDims; ++k) // update position
                        {
                            fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k] += (int)fleetSwarms[i].fleetParticles[j].velocity[k];
                            // constrain all xi
                            if (fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k] < 0)
                            {
                                fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k] = 0;
                            }
                            else if (fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k]
                                        > (int)((10 * Program.DefenseValue) / (ulong)Program.FleetUnitsTotalCosts[k]))
                            {
                                //fleetSwarms[i].fleetParticles[j].fleetComposition[k] = (maxX - minX) * rand.NextDouble() + minX;
                                fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k] = (int)(rand.NextDouble()
                                                                                                * (10 * Program.DefenseValue) / Program.FleetUnitsTotalCosts[k]);
                            }
                        }

                        // update error
                        fleetSwarms[i].fleetParticles[j].EvaluateFleet();

                        // check if new best error for this fleet
                        if (fleetSwarms[i].fleetParticles[j].profits > fleetSwarms[i].fleetParticles[j].pBestProfits)
                        {
                            fleetSwarms[i].fleetParticles[j].consecutiveNonImproves = 0;
                            fleetSwarms[i].fleetParticles[j].pBestProfits = fleetSwarms[i].fleetParticles[j].profits;
                            fleetSwarms[i].fleetParticles[j].bestLocalFleet.CopyFleet(fleetSwarms[i].fleetParticles[j].fleet);

                            if (fleetSwarms[i].fleetParticles[j].profits > fleetSwarms[i].lBestProfits) // new swarm best?
                            {
                                fleetSwarms[i].lBestProfits = fleetSwarms[i].fleetParticles[j].profits;
                                fleetSwarms[i].fleetParticles[j].bestLocalFleet.CopyFleet(fleetSwarms[i].fleetParticles[j].fleet);

                                if (fleetSwarms[i].fleetParticles[j].profits > gBestProfits) // new global best?
                                {
                                    //Simulate again with more trials to avoid outliers messing with results
                                    double moreAccurateProfits = fleetSwarms[i].fleetParticles[j].EvaluateFleet(20);
                                    if (moreAccurateProfits > gBestProfits)
                                    {
                                        epochsSinceImprovement = 0;

                                        fleetSwarms[i].fleetParticles[j].pBestProfits = moreAccurateProfits;
                                        fleetSwarms[i].lBestProfits = moreAccurateProfits;
                                        gBestProfits = moreAccurateProfits;
                                        gBestFleet.CopyFleet(fleetSwarms[i].fleetParticles[j].fleet);
                                        PrintCurrentNewBestFleetComposition(gBestFleet.ShipCounts, gBestProfits, epoch);
                                        gBestList.Add((epoch, gBestProfits));
                                    }
                                }
                            }
                        }
                    } // each particle
                } // each swarm
            } // while
            string fleetStr = String.Join(", ", gBestFleet.ShipCounts);
            Console.WriteLine("\tBest fleet found: " + fleetStr);
            Console.WriteLine("\t\tProfits per hour: " + gBestProfits);
            var pm = new PlotModel { Title = "Defense: " + String.Join(", ", defense.DefenseCounts),
                                    PlotAreaBorderThickness = new OxyThickness(0),
                                    Subtitle = "\nBest Fleet: " + String.Join(", ", gBestFleet.ShipCounts),
            };
            var categoryAxis = new OxyPlot.Axes.CategoryAxis { AxislineStyle = LineStyle.Solid, TickStyle = TickStyle.None };
            var value = new List<DataPoint>();
            for (int i = 0; i < gBestList.Count; i++)
            {
                value.Add(new DataPoint(gBestList[i].Item1, gBestList[i].Item2));
            }


            pm.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 1.05 * Math.Abs(gBestList[gBestList.Count - 1].Item2),
                MajorStep = 4_000_000,
                MinorStep = 1_000_000,
                AxislineStyle = LineStyle.Solid,
                TickStyle = TickStyle.Crossing,
                StringFormat = "0,0"
            });

            pm.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = -1,
                Maximum = Program.MaxEpochsInner,
                MajorStep = Program.MaxEpochsInner / 5,
                MinorStep = Program.MaxEpochsInner / 20,
                AxislineStyle = LineStyle.Solid,
                TickStyle = TickStyle.Outside
            });


            pm.Series.Add(new OxyPlot.Series.ScatterSeries
            {
                ItemsSource = value,
                MarkerType = MarkerType.Circle,
                MarkerSize = 3.0,
                MarkerFill = OxyColors.White,
                MarkerStroke = OxyColors.Black,
                DataFieldX = "X",
                DataFieldY = "Y",
            });

            Stream stream = File.Create("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\plots\\" + String.Join(", ", defense.DefenseCounts) + "plot.pdf");
            var pdf = new PdfExporter();
            PdfExporter.Export(pm, stream, 400.0, 400);

            error = gBestProfits;
            return gBestProfits;
        }

        void PrintCurrentNewBestFleetComposition(int[] fleetComposition, double profit, int epoch)
        {
            Console.WriteLine("\t\tEpoch " + epoch);
            Console.WriteLine("\t\tNew best fleet found: [" + String.Join(",", fleetComposition) + "]");
            Console.WriteLine("\t\tProfits per hour: " + profit);
        }
    }
}