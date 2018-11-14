using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using OxyPlot;
using OxyPlot.Axes;

namespace OgameDefenseMSO
{
    public class Program
    {

        //**************************************************************************************************************
        //Static Ogame parameters
        public static int DefenseDims = 6; //rl, ll, hl, gc, ic, pt = 6 defense structures to choose from
        public static int FleetDims = 9; //sc, lc, lf, hf, c, bs, b, d, bc = 9 fleet structure to choose from
        /// <summary>
        /// The number of military ship types that fleet particles will be composed of. The dimensionality of the search space for fleets
        /// </summary>
        public static int MilFleetDims = 7;

        //defense cost: metal, crystal, deut
        public static int[] CostRL = { 2000, 0, 0 };
        public static int[] CostLL = { 1500, 500, 0 };
        public static int[] CostHL = { 6000, 2000, 0 };
        public static int[] CostGC = { 20000, 15000, 2000 };
        public static int[] CostIC = { 2000, 6000, 0 };
        public static int[] CostPT = { 50000, 50000, 30000 };

        //ship cost and data: metal, crystal, deut, fuel usage, and cargo space
        public static int[] DataSC = { 2000, 2000, 0, 20, 5000 };
        public static int[] DataLC = { 6000, 6000, 0, 50, 25000 };
        public static int[] DataLF = { 3000, 1000, 0, 20, 50 };
        public static int[] DataHF = { 6000, 4000, 0, 75, 100 };
        public static int[] DataC = { 20000, 7000, 2000, 300, 800 };
        public static int[] DataBS = { 45000, 15000, 0, 500, 1500 };
        public static int[] DataB = { 50000, 25000, 15000, 1000, 500 };
        public static int[] DataD = { 60000, 50000, 15000, 1000, 2000 };
        public static int[] DataBC = { 30000, 40000, 15000, 250, 750 };

        public static int[] ShipFuelUsage = { 20, 50, 20, 75, 300, 500, 1000, 1000, 250 };
        public static int[] ShipCargoSpace = { 5000, 25000, 50, 100, 800, 1500, 500, 2000, 750 };

        public static double[] ResourceValueRatios = { 1, 1.66667, 2.5 }; //Value of Metal, Crystal, and Deuterium, relative to 1 Metal
        public static double[] ResourceValueRatiosFleet = { 1, 1.66667, 2.5, 0, 0 }; //Value of Metal, Crystal, and Deuterium, relative to 1 Metal

        ///<Summary>
        /// Metal Equivalent Value of the defense
        ///</Summary>
        public static ulong DefenseValue = 528_000_000;
        public static int ResourcesAtRisk = 5000000; // raw total of resources attacker can loot, generated fleets will need enough cargo space for this value
        public static int FlightDistance = 3650;    //Ingame distance units, default of 3650 is 10 SS flight                                           
        //^Static Ogame Parameters
        //**************************************************************************************************************

        //Static MSO parameters    
        public static int NumSwarms = 4;
        public static int NumParticles = 5; //per swarm
        public static int NumFleetParticles = 5; //per swarm
        public static double Inertia = 0.729;
        public static double GravityGlobal = 0.3645; //how much particles velocity are drawn towards the global best
        public static double GravitySwarm = .75; //how much particles velocity are drawn towards the swarm best
        public static double GravityLocal = 1.49445; //how much particles velocity are drawn towards the local best
        public static double ProbDeath = 0.005; //odds a particle dies each iteration
        public static double ProbImmigrate = 0.005; //odds a particle swaps swarm each iteration
        public static int MaxEpochsInner = 1000;
        public static int MaxEpochsOuter = 2000;
        public static int MinFailsBeforeDeath = 3;

        //Fleet Optimum Scale Finder parameters
        public static int DefaultTrials = 4;
        public static int HighTrials = 40;
        public static double LearningRate = 20_000_000_000;




        //Calculated parameters
        ///
        public static int[] DefenseUnitsTotalCosts = new int[6];
        public static int[] DefenseUnitsMaximums = new int[6];
        public static double[] FleetUnitsTotalCosts = new double[9];
        public static double[] UnitTotalCosts = new double[22];

        public static int InitializationCount = 0;
        public static List<bool[]> FleetSeeds = new List<bool[]>();
        static void GenFleetSeeds()
        {
            for (int i = 0; i < MilFleetDims; i++)
            {
                int currCount = FleetSeeds.Count;
                for (int j = 0; j < currCount; j++)
                {
                    bool[] modSeed = new bool[MilFleetDims];
                    FleetSeeds[j].CopyTo(modSeed, 0);
                    modSeed[i] = true;
                    if (!IsLFOnly(modSeed))
                    {
                        FleetSeeds.Add(modSeed);
                    }
                }
                bool[] newSeed = new bool[MilFleetDims];
                newSeed[i] = true;
                if (!IsLFOnly(newSeed))
                {
                    FleetSeeds.Add(newSeed);
                }
            }
        }

        static bool IsLFOnly(bool[] typesPresent)
        {
            int numTypes = typesPresent.Count(isTypePresent => isTypePresent == true);
                
            if (numTypes == 1)
            {
                if(typesPresent[0])
                {
                    return true;
                }
            }
            return false;
        }

        public Defense defense;

        static void Main(string[] args)
        {
            Console.WriteLine("Using Multi-Swarm Optimization to find optimum anti-Raid defense");

            //set up SpeedSim
            //
            SpeedSimInterface.Init();

            GenFleetSeeds();


            //Calculate cost of defense units in Metal Equivalent Value terms
            DefenseUnitsTotalCosts[0] = (int)CostRL.Zip(ResourceValueRatios, (CostRL, ResourceValueRatios) => CostRL * ResourceValueRatios).Sum();
            DefenseUnitsTotalCosts[1] = (int)CostLL.Zip(ResourceValueRatios, (CostLL, ResourceValueRatios) => CostLL * ResourceValueRatios).Sum();
            DefenseUnitsTotalCosts[2] = (int)CostHL.Zip(ResourceValueRatios, (CostHL, ResourceValueRatios) => CostHL * ResourceValueRatios).Sum();
            DefenseUnitsTotalCosts[3] = (int)CostGC.Zip(ResourceValueRatios, (CostGC, ResourceValueRatios) => CostGC * ResourceValueRatios).Sum();
            DefenseUnitsTotalCosts[4] = (int)CostIC.Zip(ResourceValueRatios, (CostIC, ResourceValueRatios) => CostIC * ResourceValueRatios).Sum();
            DefenseUnitsTotalCosts[5] = (int)CostPT.Zip(ResourceValueRatios, (CostPT, ResourceValueRatios) => CostPT * ResourceValueRatios).Sum();

            //set maximum number of each type of defense unit
            for (int defenseIdx = 0; defenseIdx < DefenseDims; ++defenseIdx)
            {
                DefenseUnitsMaximums[defenseIdx] = (int)(DefenseValue / (ulong)DefenseUnitsTotalCosts[defenseIdx]);
            }

            //Calculate cost of ship units in Metal Equivalent Value terms
            FleetUnitsTotalCosts[0] = (int)DataSC.Zip(ResourceValueRatiosFleet, (DataSC, ResourceValueRatiosFleet) => DataSC * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[1] = (int)DataLC.Zip(ResourceValueRatiosFleet, (DataLC, ResourceValueRatiosFleet) => DataLC * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[2] = (int)DataLF.Zip(ResourceValueRatiosFleet, (DataLF, ResourceValueRatiosFleet) => DataLF * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[3] = (int)DataHF.Zip(ResourceValueRatiosFleet, (DataHF, ResourceValueRatiosFleet) => DataHF * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[4] = (int)DataC.Zip(ResourceValueRatiosFleet, (DataC, ResourceValueRatiosFleet) => DataC * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[5] = (int)DataBS.Zip(ResourceValueRatiosFleet, (DataBS, ResourceValueRatiosFleet) => DataBS * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[6] = (int)DataB.Zip(ResourceValueRatiosFleet, (DataB, ResourceValueRatiosFleet) => DataB * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[7] = (int)DataD.Zip(ResourceValueRatiosFleet, (DataD, ResourceValueRatiosFleet) => DataD * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[8] = (int)DataBC.Zip(ResourceValueRatiosFleet, (DataBC, ResourceValueRatiosFleet) => DataBC * ResourceValueRatiosFleet).Sum();

            Array.Copy(FleetUnitsTotalCosts, 0, UnitTotalCosts, 0, 6);
            UnitTotalCosts[6] = (int)(10000 * 1 + 20000 * 1.66667 + 10000 * 2.5);
            UnitTotalCosts[7] = (int)(10000 * 1 + 6000 * 1.66667 + 2000 * 2.5);
            UnitTotalCosts[8] = (int)(0 * 1 + 1000 * 1.66667 + 0 * 2.5);
            UnitTotalCosts[9] = FleetUnitsTotalCosts[6];
            UnitTotalCosts[10] = (int)(0 * 1 + 2000 * 1.66667 + 500 * 2.5);
            UnitTotalCosts[11] = FleetUnitsTotalCosts[7];
            UnitTotalCosts[12] = (int)(5000000 * 1 + 4000000 * 1.66667 + 1000000 * 2.5);
            UnitTotalCosts[13] = FleetUnitsTotalCosts[8];
            Array.Copy(DefenseUnitsTotalCosts, 0, UnitTotalCosts, 14, 6);
            UnitTotalCosts[20] = (int)(10000 * 1 + 10000 * 1.66667 + 0 * 2.5);
            UnitTotalCosts[21] = (int)(50000 * 1 + 50000 * 1.66667 + 0 * 2.5);


            Defense bestDefense = Solve();
            Console.WriteLine("\nDone");

            Console.WriteLine("\nBest solution found: ");
            ShowVector(bestDefense.DefenseCounts, true);

            Console.WriteLine("\nEnd multi-swarm optimization demo\n");

        }

        static void ShowVector(int[] vector, bool newLine)
        {
            for (int i = 0; i < vector.Length; ++i)
                Console.Write(vector[i].ToString() + " ");
            Console.WriteLine("");
            if (newLine == true) Console.WriteLine("");
        }

        ///<summary>
        /// Create and run a Fleet MSO that attempts to find the optimum fleet composition to raid this defense
        ///</summary>

        static Defense Solve()
        {
            Random rand = new Random(0);
            DefenseSwarm[] defenseSwarms = new DefenseSwarm[NumSwarms];
            for (int i = 0; i < NumSwarms; i++)
            {
                defenseSwarms[i] = new DefenseSwarm();
            }
            Defense gBestDefense = new Defense();
            double gBestError = double.MaxValue;

            for (int i = 0; i < NumSwarms; ++i)
            {
                if (defenseSwarms[i].lBestError < gBestError)
                {
                    gBestError = defenseSwarms[i].lBestError;
                    gBestDefense.CopyDefense(defenseSwarms[i].lBestDefense);
                }
            }


            Defense gBestDefenseCopy = new Defense(gBestDefense.DefenseCounts);
            var gBestList = new List<(int, double, Defense)>
            {
                (-1, gBestError, gBestDefenseCopy)
            };


            int epoch = 0;

            while (epoch < MaxEpochsOuter)
            {
                ++epoch;

                if (epoch < MaxEpochsOuter)
                {
                    Console.WriteLine("Outer Epoch " + epoch + ", gBestError " + gBestError.ToString("F1"));
                }

                for (int i = 0; i < NumSwarms; ++i) // each swarm
                {
                    // Shuffle(sequence, rand); // move particles in random sequence
                    for (int j = 0; j < NumParticles; ++j) // each particle
                    {
                        //Death
                        double p1 = rand.NextDouble();
                        if (defenseSwarms[i].defenseParticles[j].consecutiveNonImproves * p1 > 10)
                        {
                            defenseSwarms[i].defenseParticles[j] = new DefenseParticle(); // new random position
                            if (defenseSwarms[i].defenseParticles[j].error < defenseSwarms[i].lBestError) // new swarm best by luck?
                            {
                                defenseSwarms[i].lBestError = defenseSwarms[i].defenseParticles[j].error;
                                defenseSwarms[i].lBestDefense.CopyDefense(defenseSwarms[i].defenseParticles[j].defense);
                                if (defenseSwarms[i].defenseParticles[j].error < gBestError) // if a new swarm best, maybe also a new global best?
                                {
                                    //must repeat defense evaluation to avoid outlier skewing results
                                    double maxResult = ConfirmNewGBest(defenseSwarms[i].defenseParticles[j]);
                                    if (maxResult < gBestError)
                                    {
                                        gBestError = maxResult;
                                        gBestDefense.CopyDefense(defenseSwarms[i].defenseParticles[j].defense);
                                        Defense gBestDefenseCopy2 = new Defense(defenseSwarms[i].defenseParticles[j].defense.DefenseCounts);
                                        gBestList.Add((epoch, gBestError, gBestDefenseCopy2));
                                    }
                                }
                            }
                        }

                        //Immigration
                        double p2 = rand.NextDouble();
                        if (p2 < ProbImmigrate)
                        {
                            int otherSwarm = rand.Next(0, NumSwarms);
                            int otherParticle = rand.Next(0, NumParticles);
                            DefenseParticle tmp = defenseSwarms[i].defenseParticles[j];
                            defenseSwarms[i].defenseParticles[j] = defenseSwarms[otherSwarm].defenseParticles[otherParticle];
                            defenseSwarms[otherSwarm].defenseParticles[otherParticle] = tmp;

                            if (defenseSwarms[i].defenseParticles[j].pBestError < defenseSwarms[otherSwarm].lBestError) // new (other) swarm best?
                            {
                                defenseSwarms[otherSwarm].lBestError = defenseSwarms[i].defenseParticles[j].pBestError;
                                defenseSwarms[otherSwarm].lBestDefense.CopyDefense(defenseSwarms[i].defenseParticles[j].defense);
                            }
                            if (defenseSwarms[otherSwarm].defenseParticles[otherParticle].pBestError < defenseSwarms[i].lBestError) // new (curr) swarm best?
                            {
                                defenseSwarms[i].lBestError = defenseSwarms[otherSwarm].defenseParticles[otherParticle].pBestError;
                                defenseSwarms[i].lBestDefense.CopyDefense(defenseSwarms[otherSwarm].defenseParticles[otherParticle].defense);
                            }
                            // not possible for a new global best
                        }

                        for (int k = 0; k < DefenseDims; ++k) // update velocity. each x position component
                        {
                            double r1 = rand.NextDouble();
                            double r2 = rand.NextDouble();
                            double r3 = rand.NextDouble();

                            defenseSwarms[i].defenseParticles[j].velocity[k] 
                                = (
                                    (Inertia * defenseSwarms[i].defenseParticles[j].velocity[k]) 
                                    + (GravityLocal * r1 * (defenseSwarms[i].defenseParticles[j].pBestDefense.DefenseCounts[k] 
                                        - defenseSwarms[i].defenseParticles[j].defense.DefenseCounts[k])
                                        ) 
                                    + (GravitySwarm * r2 * (defenseSwarms[i].lBestDefense.DefenseCounts[k]
                                        - defenseSwarms[i].defenseParticles[j].defense.DefenseCounts[k])
                                        )
                                    + (GravityGlobal * r3 * (gBestDefense.DefenseCounts[k]
                                        - defenseSwarms[i].defenseParticles[j].defense.DefenseCounts[k])
                                        )
                                    );

                            //constrain velocities
                            //if (defenseSwarms[i].defenseParticles[j].velocity[k] < minX)
                            //    defenseSwarms[i].defenseParticles[j].velocity[k] = minX;
                            //else if (defenseSwarms[i].defenseParticles[j].velocity[k] > maxX)
                            //    defenseSwarms[i].defenseParticles[j].velocity[k] = maxX;
                        }

                        for (int k = 0; k < DefenseDims; ++k) // update position
                        {
                            defenseSwarms[i].defenseParticles[j].defense.DefenseCounts[k] += (int)defenseSwarms[i].defenseParticles[j].velocity[k];
                            // constrain all xi
                            if (defenseSwarms[i].defenseParticles[j].defense.DefenseCounts[k] < 0
                                || defenseSwarms[i].defenseParticles[j].defense.DefenseCounts[k] > DefenseUnitsMaximums[k])
                            {
                                defenseSwarms[i].defenseParticles[j].defense.DefenseCounts[k] = (int)(rand.NextDouble() * DefenseUnitsMaximums[k]);
                            }
                        }

                        // update error
                        defenseSwarms[i].defenseParticles[j].EvaluateDefense();

                        // check if new best error for this particle
                        if (defenseSwarms[i].defenseParticles[j].error < defenseSwarms[i].defenseParticles[j].pBestError)
                        {
                            defenseSwarms[i].defenseParticles[j].pBestError = defenseSwarms[i].defenseParticles[j].error;
                            defenseSwarms[i].defenseParticles[j].pBestDefense.CopyDefense(defenseSwarms[i].defenseParticles[j].defense);

                            if (defenseSwarms[i].defenseParticles[j].error < defenseSwarms[i].lBestError) // new swarm best?
                            {
                                defenseSwarms[i].lBestError = defenseSwarms[i].defenseParticles[j].error;
                                defenseSwarms[i].lBestDefense.CopyDefense(defenseSwarms[i].defenseParticles[j].defense);

                                if (defenseSwarms[i].defenseParticles[j].error < gBestError) // new global best?
                                {
                                    //must repeat defense evaluation to avoid outlier skewing results
                                    double maxResult = ConfirmNewGBest(defenseSwarms[i].defenseParticles[j]);
                                    if (maxResult < gBestError)
                                    {
                                        gBestError = maxResult;
                                        gBestDefense.CopyDefense(defenseSwarms[i].defenseParticles[j].defense);
                                        Defense gBestDefenseCopy2 = new Defense(defenseSwarms[i].defenseParticles[j].defense.DefenseCounts);
                                        gBestList.Add((epoch, gBestError, gBestDefenseCopy2));
                                    }
                                }
                            }
                        }
                    } // each particle
                } // each swarm
            } // while
            string defenseStr = String.Join(", ", gBestDefense.DefenseCounts);
            Console.WriteLine("\n****Best defense found: " + defenseStr + " ****");
            var pm = new PlotModel { Title = "DefenseMSO", PlotAreaBorderThickness = new OxyThickness(0) };
            var categoryAxis = new OxyPlot.Axes.CategoryAxis { AxislineStyle = LineStyle.Solid, TickStyle = TickStyle.None };
            var value = new List<DataPoint>();
            for (int i = 0; i < gBestList.Count; i++)
            {
                value.Add(new DataPoint(gBestList[i].Item1, gBestList[i].Item2));
            }


            pm.Axes.Add
            (
                new OxyPlot.Axes.LinearAxis
                {
                    Position = AxisPosition.Left,
                    Minimum = -Math.Abs(gBestList[0].Item2),
                    Maximum = 1.05 * Math.Abs(gBestList[gBestList.Count - 1].Item2),
                    MajorStep = Math.Abs(gBestList[0].Item2 / 10),
                    MinorStep = Math.Abs(gBestList[0].Item2 / 50),
                    AxislineStyle = LineStyle.Solid,
                    TickStyle = TickStyle.Crossing,
                    StringFormat = "0,0"
                }
            );

            pm.Axes.Add
            (
                new OxyPlot.Axes.LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    Minimum = -1,
                    Maximum = MaxEpochsOuter,
                    MajorStep = Program.MaxEpochsInner / 5,
                    MinorStep = Program.MaxEpochsInner / 20,
                    AxislineStyle = LineStyle.Solid,
                    TickStyle = TickStyle.Outside
                }
            );


            pm.Series.Add
            (
                new OxyPlot.Series.ScatterSeries
                {
                    ItemsSource = value,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3.0,
                    MarkerFill = OxyColors.White,
                    MarkerStroke = OxyColors.Black,
                    DataFieldX = "X",
                    DataFieldY = "Y"
                }
            );

            Stream stream = File.Create("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\defenseplot.pdf");
            var pdf = new PdfExporter();
            PdfExporter.Export(pm, stream, 400.0, 400);

            return gBestDefense;
        }

        static double ConfirmNewGBest(DefenseParticle defenseParticle)
        {
            double maxResult = defenseParticle.error;
            for (int i = 0; i < 5; i++)
            {
                double result = defenseParticle.EvaluateDefense();
                if(result > maxResult)
                {
                    maxResult = result;
                }
            }
            return maxResult;
        }
    }
}