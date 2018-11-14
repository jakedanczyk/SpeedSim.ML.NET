//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Runtime.InteropServices;
//using OgameDefenseMSO;
//using OxyPlot;
//using OxyPlot.Axes;
//using OxyPlot.Series;

//namespace OgameDefenseMSO
//{
//    public class FleetOptimizer
//    {

//        //**************************************************************************************************************
//        //Static Ogame parameters
//        public static int DefenseDims = 6; //rl = 0, ll, hl, gc, ic, pt = 5. 6 defense structures to choose from
//        public static int FleetDims = 7; //sc = 0, lc, lf, hf, c, bs, b, d, bc = 8. 9 fleet structure to choose from

//        //defense cost: metal, crystal, deut
//        public static int[] CostRL = { 2000, 0, 0 };
//        public static int[] CostLL = { 1500, 500, 0 };
//        public static int[] CostHL = { 6000, 2000, 0 };
//        public static int[] CostGC = { 20000, 15000, 2000 };
//        public static int[] CostIC = { 2000, 6000, 0 };
//        public static int[] CostPT = { 50000, 50000, 30000 };

//        //ship cost and data: metal, crystal, deut, fuel usage, and cargo space
//        public static int[] DataSC = { 2000, 2000, 0, 20, 5000 };
//        public static int[] DataLC = { 6000, 6000, 0, 50, 25000 };
//        public static int[] DataLF = { 3000, 1000, 0, 20, 50 };
//        public static int[] DataHF = { 6000, 4000, 0, 75, 100 };
//        public static int[] DataC = { 20000, 7000, 2000, 300, 800 };
//        public static int[] DataBS = { 45000, 15000, 0, 500, 1500 };
//        public static int[] DataB = { 50000, 25000, 15000, 1000, 500 };
//        public static int[] DataD = { 60000, 50000, 15000, 1000, 2000 };
//        public static int[] DataBC = { 30000, 40000, 15000, 250, 750 };

//        public static int[] ShipFuelUsage = { 20, 50, 20, 75, 300, 500, 1000, 1000, 250 };
//        public static int[] ShipCargoSpace = { 5000, 25000, 50, 100, 800, 1500, 500, 2000, 750 };

//        public static double[] ResourceValueRatios = { 1, 1.66667, 2.5 }; //Value of Metal, Crystal, and Deuterium, relative to 1 Metal
//        public static double[] ResourceValueRatiosFleet = { 1, 1.66667, 2.5, 0, 0 }; //Value of Metal, Crystal, and Deuterium, relative to 1 Metal

//        ///<Summary>
//        /// Metal Equivalent Value of the defense
//        ///</Summary>
//        public static ulong DefenseValue = 150000000;
//        public static int ResourcesAtRisk = 0; // raw total of resources attacker can loot, generated fleets will need enough cargo space for this value
//        public static int FlightDistance = 3650;    //Ingame distance units, default of 3650 is 10 SS flight                                           
//        //^Static Ogame Parameters
//        //**************************************************************************************************************

//        //Static MSO parameters    
//        public static int NumSwarms = 4;
//        public static int NumParticles = 5; //per swarm
//        public static double Inertia = .729;
//        public static double GravityGlobal = .1; //how much particles velocity are drawn towards the global best
//        public static double GravitySwarm = 0.0;//1.49445; //how much particles velocity are drawn towards the swarm best
//        public static double GravityLocal = 1.49445; //how much particles velocity are drawn towards the local best
//        public static double ProbDeath = 0.005; //odds a particle dies each iteration
//        public static double ProbImmigrate = 0.005; //odds a particle swaps swarm each iteration
//        public static int MaxEpochs = 2000;
//        public static int MinFailsBeforeDeath = 1;

//        public static int DefaultTrials = 4;
//        public static int HighTrials = 40;


//        //Calculated parameters
//        ///
//        public static int[] DefenseUnitsTotalCosts = new int[6];
//        public static int[] DefenseUnitsMaximums = new int[6];
//        public static int[] FleetUnitsTotalCosts = new int[9];
//        public static int[] UnitTotalCosts = new int[22];
//        public static int InitializationCount = 0;
//        public static List<bool[]> FleetSeeds = new List<bool[]>();
//        static void GenFleetSeeds()
//        {
//            for (int i = 0; i < 9; i++)
//            {
//                int currCount = FleetSeeds.Count;
//                for (int j = 0; j < currCount; j++)
//                {
//                    bool[] modSeed = new bool[9];
//                    FleetSeeds[j].CopyTo(modSeed, 0);
//                    modSeed[i] = true;
//                    FleetSeeds.Add(modSeed);
//                }
//                bool[] newSeed = new bool[9];
//                newSeed[i] = true;
//                FleetSeeds.Add(newSeed);
//            }
//        }

//        public static Defense defense = new Defense();

//        static void Main(string[] args)
//        {
//            Console.WriteLine("Using Multi-Swarm Optimization to find optimum attack fleet against set target");

//            //set up SpeedSim


//            //Calculate cost of defense units in Metal Equivalent Value terms
//            DefenseUnitsTotalCosts[0] = CalculateUnitCost(CostRL);
//            DefenseUnitsTotalCosts[1] = CalculateUnitCost(CostLL);
//            DefenseUnitsTotalCosts[2] = CalculateUnitCost(CostHL);
//            DefenseUnitsTotalCosts[3] = CalculateUnitCost(CostGC);
//            DefenseUnitsTotalCosts[4] = CalculateUnitCost(CostIC);
//            DefenseUnitsTotalCosts[5] = CalculateUnitCost(CostPT);

//            GenFleetSeeds();

//            //set maximum number of each type of defense unit
//            for (int defenseIdx = 0; defenseIdx < DefenseDims; ++defenseIdx)
//            {
//                DefenseUnitsMaximums[defenseIdx] = (int)(DefenseValue / (ulong)DefenseUnitsTotalCosts[defenseIdx]);
//            }

//            //Calculate cost of ship units in Metal Equivalent Value terms
//            FleetUnitsTotalCosts[0] = CalculateUnitCost(DataSC);
//            FleetUnitsTotalCosts[1] = CalculateUnitCost(DataLC);
//            FleetUnitsTotalCosts[2] = CalculateUnitCost(DataLF);
//            FleetUnitsTotalCosts[3] = CalculateUnitCost(DataHF);
//            FleetUnitsTotalCosts[4] = CalculateUnitCost(DataC);
//            FleetUnitsTotalCosts[5] = CalculateUnitCost(DataBS);
//            FleetUnitsTotalCosts[6] = CalculateUnitCost(DataB);
//            FleetUnitsTotalCosts[7] = CalculateUnitCost(DataD);
//            FleetUnitsTotalCosts[8] = CalculateUnitCost(DataBC);

//            Array.Copy(FleetUnitsTotalCosts, 0, UnitTotalCosts, 0, 6);
//            UnitTotalCosts[6] = (int)(10000 * 1 + 20000 * 1.66667 + 10000 * 2.5);
//            UnitTotalCosts[7] = (int)(10000 * 1 + 6000 * 1.66667 + 2000 * 2.5);
//            UnitTotalCosts[8] = (int)(0 * 1 + 1000 * 1.66667 + 0 * 2.5);
//            UnitTotalCosts[9] = FleetUnitsTotalCosts[6];
//            UnitTotalCosts[10] = (int)(0 * 1 + 2000 * 1.66667 + 500 * 2.5);
//            UnitTotalCosts[11] = FleetUnitsTotalCosts[7];
//            UnitTotalCosts[12] = (int)(5000000 * 1 + 4000000 * 1.66667 + 1000000 * 2.5);
//            UnitTotalCosts[13] = FleetUnitsTotalCosts[8];
//            Array.Copy(DefenseUnitsTotalCosts, 0, UnitTotalCosts, 14, 6);
//            UnitTotalCosts[20] = (int)(10000 * 1 + 10000 * 1.66667 + 0 * 2.5);
//            UnitTotalCosts[21] = (int)(50000 * 1 + 50000 * 1.66667 + 0 * 2.5);

//            //defense.UnitCounts = new int[] { 1088, 1442, 1000, 772, 0, 201, 0, 352, 0, 0, 0, 0, 0, 139, 12384, 4347, 42, 28, 40, 17, 1, 1 };
//            //defense.UnitCounts = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 12384, 4347, 42, 28, 40, 17, 1, 1 };
//            //defense.UnitCounts = new int[] { 0, 200, 0, 0, 0, 0, 0, 0, 0, 0, 400, 0, 0, 0, 0, 3463, 640, 49, 0, 0, 1, 1 };
//            defense.UnitCounts = new int[] { 0, 200, 0, 0, 0, 0, 0, 0, 0, 0, 400, 0, 0, 0, 1854, 404, 234, 135, 0, 14, 1, 1 };
//            //defense.UnitCounts = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10000, 8570, 2850, 400, 1667, 96, 1, 1 };
//            //defense.UnitCounts = new int[] { 221, 469, 0, 437, 511, 453, 0, 25, 0, 1, 145, 31, 1, 97, 300, 0, 0, 0, 170, 0, 1, 1 };

//            DefenseValue = defense.CalculateCost();

//            double maxAttackerProfitRate = SolveAttackerMaximumProfitRate();
//            Console.WriteLine("\nDone");


//            Console.WriteLine("\nEnd Fleet Optimizer");
//            Console.WriteLine(InitializationCount);

//            Console.ReadKey();
//            Console.ReadLine();
//        }

//        public static int CalculateUnitCost(int[] unitCosts)
//        {
//            double costD = 0;
//            costD += unitCosts[0] * ResourceValueRatios[0];
//            costD += unitCosts[1] * ResourceValueRatios[1];
//            costD += unitCosts[2] * ResourceValueRatios[2];
//            int costI = Convert.ToInt32(costD);
//            return costI;
//        }

//        static void ShowVector(int[] vector, bool newLine)
//        {
//            for (int i = 0; i < vector.Length; ++i)
//                Console.Write(vector[i].ToString() + " ");
//            Console.WriteLine("");
//            if (newLine == true) Console.WriteLine("");
//        }

//        ///<summary>
//        /// Create and run a Fleet MSO that attempts to find the optimum fleet composition to raid this defense
//        ///</summary>
//        public static double SolveAttackerMaximumProfitRate()
//        {
//            Console.WriteLine("Using Multi-Swarm Optimization to solve for optimum fleet to raid defense composition ["
//                                + String.Join(",", defense.UnitCounts)
//                                + "]");

//            Random rand = new Random();
//            FleetSwarm[] fleetSwarms = new FleetSwarm[Program.NumSwarms];
//            for (int i = 0; i < Program.NumSwarms; ++i)
//            {
//                fleetSwarms[i] = new FleetSwarm(defense);
//            }
//            Fleet gBestFleet = new Fleet();
//            double gBestProfits = double.MinValue;
//            for (int i = 0; i < Program.NumSwarms; ++i)
//            {
//                if (fleetSwarms[i].lBestProfits > gBestProfits)
//                {
//                    gBestProfits = fleetSwarms[i].lBestProfits;
//                    gBestFleet.CopyFleet(fleetSwarms[i].lBestFleet);
//                    PrintCurrentNewBestFleetComposition(gBestFleet.ShipCounts, gBestProfits, -1);
//                }
//            }

//            var globalBestList = new List<(int, double)>
//            {
//                (-1, gBestProfits)
//            };

//            int epoch = 0;

//            while (epoch < (Program.MaxEpochs))
//            {
//                ++epoch;

//                if (epoch % 10 == 0)
//                {
//                    Console.WriteLine("Defense[" + String.Join(",", defense) + "], "
//                                        + " Epoch " + epoch
//                                        + ", Global Best Minimal Fleet Cost = " + gBestProfits.ToString("F4"));
//                }

//                for (int i = 0; i < Program.NumSwarms; ++i) // each swarm
//                {
//                    // Shuffle(sequence, rand); // move particles in random sequence
//                    for (int j = 0; j < Program.NumParticles; ++j) // each particle
//                    {
//                        //int j = sequence[pj];
//                        double p1 = rand.NextDouble();
//                        if (fleetSwarms[i].fleetParticles[j].consecutiveNonImproves * p1 > MinFailsBeforeDeath)
//                        {
//                            fleetSwarms[i].fleetParticles[j] = new FleetParticle(defense); // new random position
//                            if (fleetSwarms[i].fleetParticles[j].pBestProfits > fleetSwarms[i].lBestProfits) // new swarm best by luck?
//                            {
//                                double moreAccurateProfits = fleetSwarms[i].fleetParticles[j].EvaluateFleet(5);
//                                if (moreAccurateProfits > fleetSwarms[i].lBestProfits)
//                                {
//                                    fleetSwarms[i].lBestProfits = moreAccurateProfits;
//                                    fleetSwarms[i].lBestFleet.CopyFleet(fleetSwarms[i].fleetParticles[j].bestLocalFleet);
//                                }
//                                if (fleetSwarms[i].fleetParticles[j].pBestProfits > gBestProfits) // if a new swarm best, maybe also a new global best?
//                                {
//                                    //Simulate again with higher number of trials before confirming new global best
//                                    moreAccurateProfits = fleetSwarms[i].fleetParticles[j].EvaluateFleet(20);
//                                    if (moreAccurateProfits > gBestProfits)
//                                    {
//                                        gBestProfits = moreAccurateProfits;
//                                        gBestFleet.CopyFleet(fleetSwarms[i].fleetParticles[j].fleet);
//                                        PrintCurrentNewBestFleetComposition(gBestFleet.ShipCounts, gBestProfits, epoch);
//                                        globalBestList.Add((epoch, gBestProfits));
//                                    }
//                                }
//                            }
//                        }
//                        // an alternative is to maintain a particle age and die with high prob after a certain age reached
//                        // another option is to maintain particle health/weakness (related to either ratio of times improved / loop count
//                        // or number consecutive improves or consecutive non-improves) and die with high prob when health is low 

//                        double p2 = rand.NextDouble();
//                        if (p2 < Program.ProbImmigrate)
//                        {
//                            int otherSwarm = rand.Next(0, Program.NumSwarms);
//                            int otherParticle = rand.Next(0, Program.NumParticles);
//                            FleetParticle tmp = fleetSwarms[i].fleetParticles[j];
//                            fleetSwarms[i].fleetParticles[j] = fleetSwarms[otherSwarm].fleetParticles[otherParticle];
//                            fleetSwarms[otherSwarm].fleetParticles[otherParticle] = tmp;

//                            if (fleetSwarms[i].fleetParticles[j].pBestProfits > fleetSwarms[otherSwarm].lBestProfits) // new (other) swarm best?
//                            {
//                                double moreAccurateProfits = fleetSwarms[i].fleetParticles[j].EvaluateFleet(5);
//                                if (moreAccurateProfits > fleetSwarms[otherSwarm].lBestProfits)
//                                {
//                                    fleetSwarms[otherSwarm].lBestProfits = moreAccurateProfits;
//                                    fleetSwarms[otherSwarm].lBestFleet.CopyFleet(fleetSwarms[i].fleetParticles[j].bestLocalFleet);
//                                }
//                            }
//                            if (fleetSwarms[otherSwarm].fleetParticles[otherParticle].pBestProfits > fleetSwarms[i].lBestProfits) // new (curr) swarm best?
//                            {
//                                double moreAccurateProfits = fleetSwarms[otherSwarm].fleetParticles[otherParticle].EvaluateFleet(5);
//                                if (moreAccurateProfits > fleetSwarms[i].lBestProfits)
//                                {
//                                    fleetSwarms[i].lBestProfits = moreAccurateProfits;
//                                    fleetSwarms[i].lBestFleet.CopyFleet(fleetSwarms[otherSwarm].fleetParticles[otherParticle].fleet);
//                                }
//                            }
//                            // not possible for a new global best
//                        }

//                        for (int k = 0; k < Program.FleetDims; ++k) // update velocity. each x position component
//                        {
//                            double r1 = rand.NextDouble();
//                            double r2 = rand.NextDouble();
//                            double r3 = rand.NextDouble();

//                            fleetSwarms[i].fleetParticles[j].velocity[k] = (Program.Inertia * fleetSwarms[i].fleetParticles[j].velocity[k])
//                                                                            + (Program.GravityLocal * r1 * (fleetSwarms[i].fleetParticles[j].bestLocalFleet.ShipCounts[k] - fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k]))
//                                                                            + (Program.GravitySwarm * r2 * (fleetSwarms[i].lBestFleet.ShipCounts[k]
//                                                                                - fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k]))
//                                                                            + (Program.GravityGlobal * r3 * (gBestFleet.ShipCounts[k]
//                                                                                - fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k]));

//                            //if (fleetSwarms[i].fleetParticles[j].velocity[k] < minX) // constrain velocities
//                            //    fleetSwarms[i].fleetParticles[j].velocity[k] = minX;
//                            //else if (fleetSwarms[i].fleetParticles[j].velocity[k] > maxX)
//                            //    fleetSwarms[i].fleetParticles[j].velocity[k] = maxX;
//                        }

//                        for (int k = 0; k < Program.FleetDims; ++k) // update position
//                        {
//                            fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k] += (int)fleetSwarms[i].fleetParticles[j].velocity[k];
//                            // constrain all xi
//                            if (fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k] < 0)
//                            {
//                                fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k] = 0;
//                            }
//                            else if (fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k]
//                                        > (int)((20 * Program.DefenseValue) / (ulong)Program.FleetUnitsTotalCosts[k]))
//                            {
//                                //fleetSwarms[i].fleetParticles[j].fleetComposition[k] = (maxX - minX) * rand.NextDouble() + minX;
//                                //fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k] = (int)(rand.NextDouble()
//                                //                                                                * (10 * Program.DefenseValue) / Program.FleetUnitsTotalCosts[k]);
//                                fleetSwarms[i].fleetParticles[j].fleet.ShipCounts[k] = 0;
//                            }
//                        }

//                        //fleetSwarms[i].fleetParticles[j].EnsureSufficientCargoSpace();

//                        // update error
//                        fleetSwarms[i].fleetParticles[j].EvaluateFleet();
//                        fleetSwarms[i].fleetParticles[j].consecutiveNonImproves++;
//                        // check if new best error for this particle
//                        if (fleetSwarms[i].fleetParticles[j].profits > fleetSwarms[i].fleetParticles[j].pBestProfits)
//                        {
//                            fleetSwarms[i].fleetParticles[j].consecutiveNonImproves = 0;
//                            fleetSwarms[i].fleetParticles[j].pBestProfits = fleetSwarms[i].fleetParticles[j].profits;
//                            fleetSwarms[i].fleetParticles[j].bestLocalFleet.CopyFleet(fleetSwarms[i].fleetParticles[j].fleet);

//                            if (fleetSwarms[i].fleetParticles[j].profits > fleetSwarms[i].lBestProfits) // new swarm best?
//                            {
//                                double moreAccurateProfits = fleetSwarms[i].fleetParticles[j].EvaluateFleet(5);
//                                if (moreAccurateProfits > fleetSwarms[i].lBestProfits)
//                                {
//                                    fleetSwarms[i].lBestProfits = moreAccurateProfits;
//                                    fleetSwarms[i].lBestFleet.CopyFleet(fleetSwarms[i].fleetParticles[j].fleet);
//                                }

//                                if (fleetSwarms[i].fleetParticles[j].profits > gBestProfits) // new global best?
//                                {
//                                    //Simulate again with more trials to avoid outliers messing with results
//                                    moreAccurateProfits = fleetSwarms[i].fleetParticles[j].EvaluateFleet(20);
//                                    if (moreAccurateProfits > gBestProfits)
//                                    {
//                                        gBestProfits = moreAccurateProfits;
//                                        gBestFleet.CopyFleet(fleetSwarms[i].fleetParticles[j].fleet);
//                                        PrintCurrentNewBestFleetComposition(gBestFleet.ShipCounts, gBestProfits, epoch);
//                                        globalBestList.Add((epoch, gBestProfits));
//                                    }
//                                }
//                            }
//                        }
//                    } // each particle
//                } // each swarm
//            } // while
//            string fleetStr = String.Join(", ", gBestFleet.ShipCounts);
//            Console.WriteLine("\nBest solution found: " + fleetStr);
//            var pm = new PlotModel { Title = "FleetMSO: " + String.Join(", ", defense.UnitCounts), PlotAreaBorderThickness = new OxyThickness(0) };
//            var categoryAxis = new OxyPlot.Axes.CategoryAxis { AxislineStyle = LineStyle.Solid, TickStyle = TickStyle.None };
//            var value = new List<DataPoint>();
//            for (int i = 0; i < globalBestList.Count; i++)
//            {
//                value.Add(new DataPoint(globalBestList[i].Item1, globalBestList[i].Item2));
//            }


//            pm.Axes.Add(

//                new OxyPlot.Axes.LinearAxis

//                {

//                    Position = AxisPosition.Left,

//                    Minimum = globalBestList[1].Item2,

//                    Maximum = globalBestList[globalBestList.Count - 1].Item2,

//                    MajorStep = Math.Abs(globalBestList[globalBestList.Count - 1].Item2) / 10,

//                    MinorStep = Math.Abs(globalBestList[globalBestList.Count - 1].Item2) / 50,

//                    AxislineStyle = LineStyle.Solid,

//                    TickStyle = TickStyle.Crossing,

//                    StringFormat = "0,0"

//                });

//            pm.Axes.Add(new OxyPlot.Axes.LinearAxis

//            {

//                Position = AxisPosition.Bottom,

//                Minimum = -1,

//                Maximum = MaxEpochs,

//                MajorStep = MaxEpochs / 5,

//                MinorStep = MaxEpochs / 20,

//                AxislineStyle = LineStyle.Solid,

//                TickStyle = TickStyle.Outside

//            });


//            pm.Series.Add(

//                new OxyPlot.Series.ScatterSeries

//                {

//                    ItemsSource = value,

//                    MarkerType = MarkerType.Circle,

//                    MarkerSize = 3.0,

//                    MarkerFill = OxyColors.White,

//                    MarkerStroke = OxyColors.Black,

//                    DataFieldX = "X",

//                    DataFieldY = "Y"

//                });

//            Stream stream = File.Create("C:\\Users\\admin\\source\\repos\\FleetMSO\\FleetMSO\\plot" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".pdf");
//            var pdf = new PdfExporter();
//            PdfExporter.Export(pm, stream, 400.0, 400);

//            return gBestProfits;
//        }

//        static void PrintCurrentNewBestFleetComposition(int[] fleetComposition, double cost, int epoch)
//        {
//            Console.WriteLine("Epoch " + epoch);
//            Console.WriteLine("New best fleet composition found: [" + String.Join(",", fleetComposition) + "]");
//            Console.WriteLine("Fleet cost: " + cost);
//        }
//    }
//}