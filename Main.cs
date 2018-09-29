using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedSimML
{
    public class Program
    {
        //*********************************************************************************************************************************************************
        //Static Ogame parameters
        public static int DefenseDims = 6; //rl, ll, hl, gc, ic, pt = 6 defense structures to choose from
        public static int AttackDims = 9; //sc, lc, lf, hf, c, bs, b, d, bc = 9 fleet structure to choose from

        public static UnitType[] StdAttackUnits = {
                                                        UnitType.SC,
                                                        UnitType.LC,
                                                        UnitType.LF,
                                                        UnitType.HF,
                                                        UnitType.C,
                                                        UnitType.BS,
                                                        UnitType.B,
                                                        UnitType.D,
                                                        UnitType.BC,
                                                    };

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

        public static double[] ResourceRatios = { 1, 1.66667, 2.5 }; //Value of Metal, Crystal, and Deuterium, relative to 1 Metal
        public static double[] ResourceRatiosPadded = { 1, 1.66667, 2.5, 0, 0 }; //Value of Metal, Crystal, and Deuterium, relative to 1 Metal

        ///<Summary>
        /// Metal Equivalent Value of the defense
        ///</Summary>
        public static int DefenseValue = 10000000;
        public static int ResourcesAtRisk = 5000000; // raw total of resources attacker can loot, generated fleets will need enough cargo space for this value
        public static int FlightDistance = 3650; 
        //Ingame distance units, default of 3650 is 10 SS flight
        //^Static Ogame Parameters
        //**************************************************************************************************************************************************

        //Static MSO parameters    
        public static int NumSwarms = 4;
        public static int NumParticles = 5; //per swarm
        public static double Inertia = 0.3645;
        public static double GravityGlobal = 0.729; //how much particles velocity are drawn towards the global best
        public static double GravitySwarm = 1.49445; //how much particles velocity are drawn towards the swarm best
        public static double GravityLocal = 1.49445; //how much particles velocity are drawn towards the local best
        public static double ProbDeath = 0.1; //odds a particle dies each iteration
        public static double ProbImmigrate = 0.1; //odds a particle swaps swarm each iteration
        public static int MaxEpochs = 2000;

        //Calculated parameters
        ///
        public static int[] DefenseUnitsTotalCosts = new int[6];
        public static int[] DefenseUnitsMaximums = new int[6];
        public static int[] FleetUnitsTotalCosts = new int[9];

        static void Main(string[] args)
        {
            Console.WriteLine("Initializing SpeedSim ML");

            //data initizaliation
            DefenseUnitsTotalCosts[0] = (int)CostRL.Zip(ResourceRatios, (CostRL, ResourceValueRatios) => CostRL * ResourceValueRatios).Sum();
            DefenseUnitsTotalCosts[1] = (int)CostLL.Zip(ResourceRatios, (CostLL, ResourceValueRatios) => CostLL * ResourceValueRatios).Sum();
            DefenseUnitsTotalCosts[2] = (int)CostHL.Zip(ResourceRatios, (CostHL, ResourceValueRatios) => CostHL * ResourceValueRatios).Sum();
            DefenseUnitsTotalCosts[3] = (int)CostGC.Zip(ResourceRatios, (CostGC, ResourceValueRatios) => CostGC * ResourceValueRatios).Sum();
            DefenseUnitsTotalCosts[4] = (int)CostIC.Zip(ResourceRatios, (CostIC, ResourceValueRatios) => CostIC * ResourceValueRatios).Sum();
            DefenseUnitsTotalCosts[5] = (int)CostPT.Zip(ResourceRatios, (CostPT, ResourceValueRatios) => CostPT * ResourceValueRatios).Sum();

            for (int defenseIdx = 0; defenseIdx < DefenseDims; ++defenseIdx)
            {
                DefenseUnitsMaximums[defenseIdx] = DefenseValue / DefenseUnitsTotalCosts[defenseIdx];
            }

            FleetUnitsTotalCosts[0] = (int)DataSC.Zip(ResourceRatiosPadded, (DataSC, ResourceValueRatiosFleet) => DataSC * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[1] = (int)DataLC.Zip(ResourceRatiosPadded, (DataLC, ResourceValueRatiosFleet) => DataLC * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[2] = (int)DataLF.Zip(ResourceRatiosPadded, (DataLF, ResourceValueRatiosFleet) => DataLF * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[3] = (int)DataHF.Zip(ResourceRatiosPadded, (DataHF, ResourceValueRatiosFleet) => DataHF * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[4] = (int)DataC.Zip(ResourceRatiosPadded, (DataC, ResourceValueRatiosFleet) => DataC * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[5] = (int)DataBS.Zip(ResourceRatiosPadded, (DataBS, ResourceValueRatiosFleet) => DataBS * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[6] = (int)DataB.Zip(ResourceRatiosPadded, (DataB, ResourceValueRatiosFleet) => DataB * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[7] = (int)DataD.Zip(ResourceRatiosPadded, (DataD, ResourceValueRatiosFleet) => DataD * ResourceValueRatiosFleet).Sum();
            FleetUnitsTotalCosts[8] = (int)DataBC.Zip(ResourceRatiosPadded, (DataBC, ResourceValueRatiosFleet) => DataBC * ResourceValueRatiosFleet).Sum();

            Console.WriteLine("Select program:\nf for Optimal Fleet Against Specific Target\nd for global optimal defense.");

            while(true)
            {
                char selectKey = Console.ReadKey().KeyChar;
                if(selectKey == 'f')
                {
                    new FleetMSO();
                }
                else if(selectKey == 'd')
                {
                }
            }


            Console.WriteLine("Using Multi-Swarm Optimization to solve for optimum anti-raid defense composition");


            int[] bestDefenseComposition = SolveDefense();
            Console.WriteLine("\nDone");

            Console.WriteLine("\nBest solution found: ");
            ShowVector(bestDefenseComposition, true);

            Console.WriteLine("\nEnd multi-swarm optimization demo\n");

        }

        static void ShowVector(int[] vector, bool newLine)
        {
            for (int i = 0; i < vector.Length; ++i)
                Console.Write(vector[i].ToString() + " ");
            Console.WriteLine("");
            if (newLine == true) Console.WriteLine("");
        }

        static UnitType[] CustomAttackUnits()
        {
            Console.WriteLine("Enter y to include each unit, n to exclude");
            List<UnitType> attackUnits = new List<UnitType>();

            var unitTypeArray = Enum.GetValues(typeof(UnitType));

            foreach (UnitType unitType in unitTypeArray)
            {
                if (Units.GetUnit(unitType).CanAttack)
                {
                    Console.WriteLine("Include " + Units.GetUnit(unitType).Name + "?");
                    while (true)
                    {
                        char selectKey = Console.ReadKey().KeyChar;
                        if (selectKey == 'y')
                        {
                            attackUnits.Add(unitType);
                            break;
                        }
                        else if (selectKey == 'n')
                        {
                            break;
                        }
                    }
                }
            }
            return attackUnits.ToArray();
        }

        static int[] SolveFleet(int[] targetComposition, UnitType[] unitsToUse)
        {
            Console.WriteLine("Begin multi swarm optimization solving for ideal fleet to attack target");
            //foreach( composition " + String.Join(",", targetComposition));

            Random rand = new Random();
            FleetSwarm[] fleetSwarms = new FleetSwarm[Program.NumSwarms];
            for (int i = 0; i < Program.NumSwarms; ++i)
            {
                fleetSwarms[i] = new FleetSwarm(targetComposition);
            }
            int[] bestGlobalFleetComposition = new int[Program.AttackDims];
            long bestGlobalMinAttackCost = int.MaxValue;
            for (int i = 0; i < Program.NumSwarms; ++i)
            {
                if (fleetSwarms[i].bestSwarmMinAttackCost < bestGlobalMinAttackCost)
                {
                    bestGlobalMinAttackCost = fleetSwarms[i].bestSwarmMinAttackCost;
                    Array.Copy(fleetSwarms[i].bestSwarmFleetComposition, bestGlobalFleetComposition, Program.AttackDims);
                    //PrintCurrentNewBestFleetComposition(bestGlobalFleetComposition);
                }
            }


            int epoch = 0;

            while (epoch < (Program.MaxEpochs))
            {
                ++epoch;

                if (epoch % 10 == 0 && epoch < Program.MaxEpochs)
                {
                    Console.WriteLine(" Epoch " + epoch
                                        + ", Global Best Minimal Attack Cost = " + bestGlobalMinAttackCost.ToString("F4"));
                }

                for (int i = 0; i < Program.NumSwarms; ++i) // each swarm
                {
                    // Shuffle(sequence, rand); // move particles in random sequence
                    for (int j = 0; j < Program.NumParticles; ++j) // each particle
                    {
                        //int j = sequence[pj];
                        double p1 = rand.NextDouble();
                        if (p1 < Program.ProbDeath)
                        {
                            fleetSwarms[i].fleetParticles[j] = new FleetParticle(targetComposition); // new random position
                            if (fleetSwarms[i].fleetParticles[j].minimalAttackCost < fleetSwarms[i].bestSwarmMinAttackCost) // new swarm best by luck?
                            {
                                fleetSwarms[i].bestSwarmMinAttackCost = fleetSwarms[i].fleetParticles[j].minimalAttackCost;
                                Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, fleetSwarms[i].bestSwarmFleetComposition, Program.AttackDims);
                                if (fleetSwarms[i].fleetParticles[j].minimalAttackCost < bestGlobalMinAttackCost) // if a new swarm best, maybe also a new global best?
                                {
                                    bestGlobalMinAttackCost = fleetSwarms[i].fleetParticles[j].minimalAttackCost;
                                    Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, bestGlobalFleetComposition, Program.AttackDims);
                                    //PrintCurrentNewBestFleetComposition(bestGlobalFleetComposition);
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

                            if (fleetSwarms[i].fleetParticles[j].minimalAttackCost < fleetSwarms[otherSwarm].bestSwarmMinAttackCost) // new (other) swarm best?
                            {
                                fleetSwarms[otherSwarm].bestSwarmMinAttackCost = fleetSwarms[i].fleetParticles[j].minimalAttackCost;
                                Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, fleetSwarms[otherSwarm].bestSwarmFleetComposition, Program.AttackDims);
                            }
                            if (fleetSwarms[otherSwarm].fleetParticles[otherParticle].minimalAttackCost < fleetSwarms[i].bestSwarmMinAttackCost) // new (curr) swarm best?
                            {
                                fleetSwarms[i].bestSwarmMinAttackCost = fleetSwarms[otherSwarm].fleetParticles[otherParticle].minimalAttackCost;
                                Array.Copy(fleetSwarms[otherSwarm].fleetParticles[otherParticle].fleetComposition, fleetSwarms[i].bestSwarmFleetComposition, Program.AttackDims);
                            }
                            // not possible for a new global best
                        }

                        for (int k = 0; k < Program.AttackDims; ++k) // update velocity. each x position component
                        {
                            double r1 = rand.NextDouble();
                            double r2 = rand.NextDouble();
                            double r3 = rand.NextDouble();

                            fleetSwarms[i].fleetParticles[j].velocity[k] = (Program.Inertia * fleetSwarms[i].fleetParticles[j].velocity[k]) +
                              (Program.GravityLocal * r1 * (fleetSwarms[i].fleetParticles[j].bestFleetComposition[k] - fleetSwarms[i].fleetParticles[j].fleetComposition[k])) +
                              (Program.GravitySwarm * r2 * (fleetSwarms[i].bestSwarmFleetComposition[k] - fleetSwarms[i].fleetParticles[j].fleetComposition[k])) +
                              (Program.GravityGlobal * r3 * (bestGlobalFleetComposition[k] - fleetSwarms[i].fleetParticles[j].fleetComposition[k]));

                            //if (fleetSwarms[i].fleetParticles[j].velocity[k] < minX) // constrain velocities
                            //    fleetSwarms[i].fleetParticles[j].velocity[k] = minX;
                            //else if (fleetSwarms[i].fleetParticles[j].velocity[k] > maxX)
                            //    fleetSwarms[i].fleetParticles[j].velocity[k] = maxX;
                        }

                        for (int k = 0; k < Program.AttackDims; ++k) // update position
                        {
                            fleetSwarms[i].fleetParticles[j].fleetComposition[k] += (int)fleetSwarms[i].fleetParticles[j].velocity[k];
                            // constrain all xi
                            if (fleetSwarms[i].fleetParticles[j].fleetComposition[k] < 0)
                                //fleetSwarms[i].fleetParticles[j].fleetComposition[k] = (maxX - minX) * rand.NextDouble() + minX;
                                fleetSwarms[i].fleetParticles[j].fleetComposition[k] = 0;
                            else if (fleetSwarms[i].fleetParticles[j].fleetComposition[k] > ((10 * Program.DefenseValue) / Program.FleetUnitsTotalCosts[k]))
                                //fleetSwarms[i].fleetParticles[j].fleetComposition[k] = (maxX - minX) * rand.NextDouble() + minX;
                                fleetSwarms[i].fleetParticles[j].fleetComposition[k] = ((10 * Program.DefenseValue) / Program.FleetUnitsTotalCosts[k]);
                        }

                        fleetSwarms[i].fleetParticles[j].EnsureSufficientCargoSpace();

                        // update error
                        fleetSwarms[i].fleetParticles[j].FindAttackCost();

                        // check if new best error for this particle
                        if (fleetSwarms[i].fleetParticles[j].attackCost < fleetSwarms[i].fleetParticles[j].minimalAttackCost)
                        {
                            fleetSwarms[i].fleetParticles[j].minimalAttackCost = fleetSwarms[i].fleetParticles[j].attackCost;
                            Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition,
                                        fleetSwarms[i].fleetParticles[j].bestFleetComposition,
                                        Program.AttackDims);
                        }

                        if (fleetSwarms[i].fleetParticles[j].minimalAttackCost < fleetSwarms[i].bestSwarmMinAttackCost) // new swarm best?
                        {
                            fleetSwarms[i].bestSwarmMinAttackCost = fleetSwarms[i].fleetParticles[j].minimalAttackCost;
                            Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, fleetSwarms[i].bestSwarmFleetComposition, Program.AttackDims);
                        }

                        if (fleetSwarms[i].fleetParticles[j].minimalAttackCost < bestGlobalMinAttackCost) // new global best?
                        {
                            bestGlobalMinAttackCost = fleetSwarms[i].fleetParticles[j].minimalAttackCost;
                            Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, bestGlobalFleetComposition, Program.AttackDims);
                            //PrintCurrentNewBestFleetComposition(bestGlobalFleetComposition);
                        }
                    } // each particle
                } // each swarm
            } // while
            Console.Write("Minimal attack cost found: " + bestGlobalMinAttackCost);
            return bestGlobalFleetComposition;
        }

        static int[] SolveDefense()
        {
            Random rand = new Random(0);
            DefenseSwarm[] defenseSwarms = new DefenseSwarm[NumSwarms];
            for (int i = 0; i < NumSwarms; i++)
            {
                defenseSwarms[i] = new DefenseSwarm();
            }
            int[] bestGlobalDefense = new int[DefenseDims];
            long bestGlobalMinAttackerCost = 0;

            for (int i = 0; i < NumSwarms; ++i)
            {
                if (defenseSwarms[i].bestSwarmMaximinAttackerCost > bestGlobalMinAttackerCost)
                {
                    bestGlobalMinAttackerCost = defenseSwarms[i].bestSwarmMaximinAttackerCost;
                    Array.Copy(defenseSwarms[i].bestSwarmDefense, bestGlobalDefense, DefenseDims);
                }
            }

            int epoch = 0;

            while (epoch < MaxEpochs)
            {
                ++epoch;

                if (epoch < MaxEpochs)
                {
                    Console.WriteLine("Epoch = " + epoch + " bestGlobalMinAttackerCost = " + bestGlobalMinAttackerCost.ToString("F4"));
                }

                for (int i = 0; i < NumSwarms; ++i) // each swarm
                {
                    // Shuffle(sequence, rand); // move particles in random sequence
                    for (int j = 0; j < NumParticles; ++j) // each particle
                    {
                        //int j = sequence[pj];
                        double p1 = rand.NextDouble();
                        if (p1 < ProbDeath)
                        {
                            defenseSwarms[i].defenseParticles[j] = new DefenseParticle(); // new random position
                            if (defenseSwarms[i].defenseParticles[j].minAttackerCost > defenseSwarms[i].bestSwarmMaximinAttackerCost) // new swarm best by luck?
                            {
                                defenseSwarms[i].bestSwarmMaximinAttackerCost = defenseSwarms[i].defenseParticles[j].minAttackerCost;
                                Array.Copy(defenseSwarms[i].defenseParticles[j].defense, defenseSwarms[i].bestSwarmDefense, DefenseDims);
                                if (defenseSwarms[i].defenseParticles[j].minAttackerCost > bestGlobalMinAttackerCost) // if a new swarm best, maybe also a new global best?
                                {
                                    bestGlobalMinAttackerCost = defenseSwarms[i].defenseParticles[j].minAttackerCost;
                                    Array.Copy(defenseSwarms[i].defenseParticles[j].defense, bestGlobalDefense, DefenseDims);
                                }
                            }
                        }
                        // an alternative is to maintain a particle age and die with high prob after a certain age reached
                        // another option is to maintain particle health/weakness (related to either ratio of times improved / loop count
                        // or number consecutive improves or consecutive non-improves) and die with high prob when health is low 

                        double p2 = rand.NextDouble();
                        if (p2 < ProbImmigrate)
                        {
                            int otherSwarm = rand.Next(0, NumSwarms);
                            int otherParticle = rand.Next(0, NumParticles);
                            DefenseParticle tmp = defenseSwarms[i].defenseParticles[j];
                            defenseSwarms[i].defenseParticles[j] = defenseSwarms[otherSwarm].defenseParticles[otherParticle];
                            defenseSwarms[otherSwarm].defenseParticles[otherParticle] = tmp;

                            if (defenseSwarms[i].defenseParticles[j].minAttackerCost > defenseSwarms[otherSwarm].bestSwarmMaximinAttackerCost) // new (other) swarm best?
                            {
                                defenseSwarms[otherSwarm].bestSwarmMaximinAttackerCost = defenseSwarms[i].defenseParticles[j].minAttackerCost;
                                Array.Copy(defenseSwarms[i].defenseParticles[j].defense, defenseSwarms[otherSwarm].bestSwarmDefense, DefenseDims);
                            }
                            if (defenseSwarms[otherSwarm].defenseParticles[otherParticle].minAttackerCost > defenseSwarms[i].bestSwarmMaximinAttackerCost) // new (curr) swarm best?
                            {
                                defenseSwarms[i].bestSwarmMaximinAttackerCost = defenseSwarms[otherSwarm].defenseParticles[otherParticle].minAttackerCost;
                                Array.Copy(defenseSwarms[otherSwarm].defenseParticles[otherParticle].defense, defenseSwarms[i].bestSwarmDefense, DefenseDims);
                            }
                            // not possible for a new global best
                        }

                        for (int k = 0; k < DefenseDims; ++k) // update velocity. each x position component
                        {
                            double r1 = rand.NextDouble();
                            double r2 = rand.NextDouble();
                            double r3 = rand.NextDouble();

                            defenseSwarms[i].defenseParticles[j].velocity[k] = (Inertia * defenseSwarms[i].defenseParticles[j].velocity[k]) +
                              (GravityLocal * r1 * (defenseSwarms[i].defenseParticles[j].bestPartDefense[k] - defenseSwarms[i].defenseParticles[j].defense[k])) +
                              (GravitySwarm * r2 * (defenseSwarms[i].bestSwarmDefense[k] - defenseSwarms[i].defenseParticles[j].defense[k])) +
                              (GravityGlobal * r3 * (bestGlobalDefense[k] - defenseSwarms[i].defenseParticles[j].defense[k]));

                            //constrain velocities
                            //if (defenseSwarms[i].defenseParticles[j].velocity[k] < minX)
                            //    defenseSwarms[i].defenseParticles[j].velocity[k] = minX;
                            //else if (defenseSwarms[i].defenseParticles[j].velocity[k] > maxX)
                            //    defenseSwarms[i].defenseParticles[j].velocity[k] = maxX;
                        }

                        for (int k = 0; k < DefenseDims; ++k) // update position
                        {
                            defenseSwarms[i].defenseParticles[j].defense[k] += (int)defenseSwarms[i].defenseParticles[j].velocity[k];
                            // constrain all xi
                            if (defenseSwarms[i].defenseParticles[j].defense[k] < 0)
                                //defenseSwarms[i].defenseParticles[j].position[k] = (maxX - minX) * rand.NextDouble() + minX;
                                defenseSwarms[i].defenseParticles[j].defense[k] = 0;
                            else if (defenseSwarms[i].defenseParticles[j].defense[k] > DefenseUnitsMaximums[k])
                                //defenseSwarms[i].defenseParticles[j].position[k] = (maxX - minX) * rand.NextDouble() + minX;
                                defenseSwarms[i].defenseParticles[j].defense[k] = DefenseUnitsMaximums[k];
                        }

                        // update error
                        defenseSwarms[i].defenseParticles[j].minAttackerCost = defenseSwarms[i].defenseParticles[j].SolveAttackerMinimumCost();
                        Console.WriteLine("Epoch " + epoch
                                            + ", swarm " + i
                                            + ", particle " + j
                                            + ": new cost = " + defenseSwarms[i].defenseParticles[j].minAttackerCost);

                        // check if new best error for this particle
                        if (defenseSwarms[i].defenseParticles[j].minAttackerCost > defenseSwarms[i].defenseParticles[j].bestPartMinAttackerCost)
                        {
                            defenseSwarms[i].defenseParticles[j].bestPartMinAttackerCost = defenseSwarms[i].defenseParticles[j].minAttackerCost;
                            Array.Copy(defenseSwarms[i].defenseParticles[j].defense, defenseSwarms[i].defenseParticles[j].bestPartDefense, DefenseDims);
                        }

                        if (defenseSwarms[i].defenseParticles[j].minAttackerCost > defenseSwarms[i].bestSwarmMaximinAttackerCost) // new swarm best?
                        {
                            defenseSwarms[i].bestSwarmMaximinAttackerCost = defenseSwarms[i].defenseParticles[j].bestPartMinAttackerCost;
                            Array.Copy(defenseSwarms[i].defenseParticles[j].defense, defenseSwarms[i].bestSwarmDefense, DefenseDims);
                        }

                        if (defenseSwarms[i].defenseParticles[j].bestPartMinAttackerCost > bestGlobalMinAttackerCost) // new global best?
                        {
                            bestGlobalMinAttackerCost = defenseSwarms[i].defenseParticles[j].bestPartMinAttackerCost;
                            Array.Copy(defenseSwarms[i].defenseParticles[j].defense, bestGlobalDefense, DefenseDims);
                        }
                    } // each particle
                } // each swarm
            } // while
            return bestGlobalDefense;
        }
    }
}