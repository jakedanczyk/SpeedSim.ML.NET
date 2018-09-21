using System;

namespace MSOSpeedSim
{
    /// <summary>
    /// Particle representing defense composition
    /// </summary>
    public class DefenseParticle
    {
        static Random rand = new Random(); // do not want seed here, want each particle to have different random order

        int costOfDefense = 0;

        public int[] defense = new int[Program.DefenseDims];
        public double[] velocity = new double[Program.DefenseDims];
        public long minAttackerCost = 0;
        public int[] bestPartDefense = new int[Program.DefenseDims];
        public long bestPartMinAttackerCost = 0;

        public DefenseParticle()
        {
            for (int unitIdx = 0; unitIdx < Program.DefenseDims; ++unitIdx)
            {
                defense[unitIdx] = rand.Next(0, Program.DefenseUnitsMaximums[unitIdx] + 1);
                velocity[unitIdx] = rand.Next(-Program.DefenseUnitsMaximums[unitIdx], Program.DefenseUnitsMaximums[unitIdx] + 1);
            }
            BalanceDefense();
            minAttackerCost = SolveAttackerMinimumCost();
            bestPartMinAttackerCost = minAttackerCost;
            Array.Copy(defense, bestPartDefense, Program.DefenseDims);
        }

        /// <summary>
        /// Balances the defense to have the correct total cost by picking a random structure and increasing or decreasing number of that structure
        /// Repeats processs as necessary, until defense cost is less than the value of 1 plasma turret away from the desired valued
        /// </summary>
        void BalanceDefense()
        {
            CalculateCost();
            int plasmaTurretCost = Program.DefenseUnitsTotalCosts[5];
            bool isTooExpensive = (costOfDefense - plasmaTurretCost) > Program.DefenseValue;
            bool isTooCheap = (costOfDefense + plasmaTurretCost) < Program.DefenseValue;
            bool isBalanced = !(isTooExpensive || isTooCheap);
            while (!isBalanced)
            {
                int unitToBalanceIdx = rand.Next(0, 6);
                if (isTooExpensive)
                {
                    int numUnitsToRemove = (costOfDefense - Program.DefenseValue) / Program.DefenseUnitsTotalCosts[unitToBalanceIdx];
                    defense[unitToBalanceIdx] = Math.Max(0, defense[unitToBalanceIdx] - numUnitsToRemove);
                }
                else if (isTooCheap)
                {
                    int numUnitsToAdd = (Program.DefenseValue - costOfDefense) / Program.DefenseUnitsTotalCosts[unitToBalanceIdx];
                    defense[unitToBalanceIdx] = Math.Min(Program.DefenseUnitsMaximums[unitToBalanceIdx],
                                                                    numUnitsToAdd + defense[unitToBalanceIdx]);
                }
                CalculateCost();
                isTooExpensive = (costOfDefense - plasmaTurretCost) > Program.DefenseValue;
                isTooCheap = (costOfDefense + plasmaTurretCost) < Program.DefenseValue;
                isBalanced = !(isTooExpensive || isTooCheap);
            }
        }

        int CalculateCost()
        {
            costOfDefense = 0;
            for (int i = 0; i < Program.DefenseDims; ++i)
            {
                costOfDefense += Program.DefenseUnitsTotalCosts[i] * defense[i];
            }
            return costOfDefense;
        }

        ///<summary>
        /// Create and run a Fleet MSO that attempts to find the optimum fleet composition to raid this defense
        ///</summary>
        public long SolveAttackerMinimumCost()
        {
            Console.WriteLine("Using Multi-Swarm Optimization to solve for optimum fleet to raid defense composition ["
                                + String.Join(",", defense)
                                + "]");

            Random rand = new Random();
            FleetSwarm[] fleetSwarms = new FleetSwarm[Program.NumSwarms];
            for (int i = 0; i < Program.NumSwarms; ++i)
            {
                fleetSwarms[i] = new FleetSwarm(defense);
            }
            int[] bestGlobalFleetComposition = new int[Program.FleetDims];
            long bestGlobalMinAttackCost = int.MaxValue;
            for (int i = 0; i < Program.NumSwarms; ++i)
            {
                if (fleetSwarms[i].bestSwarmMinAttackCost < bestGlobalMinAttackCost)
                {
                    bestGlobalMinAttackCost = fleetSwarms[i].bestSwarmMinAttackCost;
                    Array.Copy(fleetSwarms[i].bestSwarmFleetComposition, bestGlobalFleetComposition, Program.FleetDims);
                    PrintCurrentNewBestFleetComposition(bestGlobalFleetComposition);
                }
            }


            int epoch = 0;

            while (epoch < (Program.MaxEpochs / 10))
            {
                ++epoch;

                if (epoch < Program.MaxEpochs)
                {
                    Console.WriteLine("Defense[" + String.Join(",", defense) + "], "
                                        + " Epoch " + epoch
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
                            fleetSwarms[i].fleetParticles[j] = new FleetParticle(defense); // new random position
                            if (fleetSwarms[i].fleetParticles[j].minimalAttackCost < fleetSwarms[i].bestSwarmMinAttackCost) // new swarm best by luck?
                            {
                                fleetSwarms[i].bestSwarmMinAttackCost = fleetSwarms[i].fleetParticles[j].minimalAttackCost;
                                Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, fleetSwarms[i].bestSwarmFleetComposition, Program.FleetDims);
                                if (fleetSwarms[i].fleetParticles[j].minimalAttackCost < bestGlobalMinAttackCost) // if a new swarm best, maybe also a new global best?
                                {
                                    bestGlobalMinAttackCost = fleetSwarms[i].fleetParticles[j].minimalAttackCost;
                                    Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, bestGlobalFleetComposition, Program.FleetDims);
                                    PrintCurrentNewBestFleetComposition(bestGlobalFleetComposition);
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
                                Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, fleetSwarms[otherSwarm].bestSwarmFleetComposition, Program.FleetDims);
                            }
                            if (fleetSwarms[otherSwarm].fleetParticles[otherParticle].minimalAttackCost < fleetSwarms[i].bestSwarmMinAttackCost) // new (curr) swarm best?
                            {
                                fleetSwarms[i].bestSwarmMinAttackCost = fleetSwarms[otherSwarm].fleetParticles[otherParticle].minimalAttackCost;
                                Array.Copy(fleetSwarms[otherSwarm].fleetParticles[otherParticle].fleetComposition, fleetSwarms[i].bestSwarmFleetComposition, Program.FleetDims);
                            }
                            // not possible for a new global best
                        }

                        for (int k = 0; k < Program.FleetDims; ++k) // update velocity. each x position component
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

                        for (int k = 0; k < Program.FleetDims; ++k) // update position
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
                                        Program.FleetDims);
                        }

                        if (fleetSwarms[i].fleetParticles[j].minimalAttackCost < fleetSwarms[i].bestSwarmMinAttackCost) // new swarm best?
                        {
                            fleetSwarms[i].bestSwarmMinAttackCost = fleetSwarms[i].fleetParticles[j].minimalAttackCost;
                            Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, fleetSwarms[i].bestSwarmFleetComposition, Program.FleetDims);
                        }

                        if (fleetSwarms[i].fleetParticles[j].minimalAttackCost < bestGlobalMinAttackCost) // new global best?
                        {
                            bestGlobalMinAttackCost = fleetSwarms[i].fleetParticles[j].minimalAttackCost;
                            Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, bestGlobalFleetComposition, Program.FleetDims);
                            PrintCurrentNewBestFleetComposition(bestGlobalFleetComposition);
                        }
                    } // each particle
                } // each swarm
            } // while
            return bestGlobalMinAttackCost;
        }

        void PrintCurrentNewBestFleetComposition(int[] fleetComposition)
        {
            Console.WriteLine("New best fleet composition found: [" + String.Join(",", fleetComposition) + "]");
        }
    }
}