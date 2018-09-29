using System;

namespace SpeedSimML
{
    /// <summary>
    /// Particle representing defense composition
    /// </summary>
    public class DefenseParticle
    {
        static Random rand = new Random(); // do not want seed here, want each particle to have different random order

        int costOfDefense = 0;

        public int[] defense = new int[OgameData.DefenseDims];
        public double[] velocity = new double[OgameData.DefenseDims];
        public long minAttackerCost = 0;
        public int[] bestPartDefense = new int[OgameData.DefenseDims];
        public long bestPartMinAttackerCost = 0;

        public DefenseParticle()
        {
            for (int unitIdx = 0; unitIdx < OgameData.DefenseDims; ++unitIdx)
            {
                defense[unitIdx] = rand.Next(0, OgameData.DefenseUnitsMaximums[unitIdx] + 1);
                velocity[unitIdx] = rand.Next(-OgameData.DefenseUnitsMaximums[unitIdx], OgameData.DefenseUnitsMaximums[unitIdx] + 1);
            }
            BalanceDefense();
            minAttackerCost = SolveAttackerMinimumCost();
            bestPartMinAttackerCost = minAttackerCost;
            Array.Copy(defense, bestPartDefense, OgameData.DefenseDims);
        }

        /// <summary>
        /// Balances the defense to have the correct total cost by picking a random structure and increasing or decreasing number of that structure
        /// Repeats processs as necessary, until defense cost is less than the value of 1 plasma turret away from the desired valued
        /// </summary>
        void BalanceDefense()
        {
            CalculateCost();
            int plasmaTurretCost = OgameData.DefenseUnitsTotalCosts[5];
            bool isTooExpensive = (costOfDefense - plasmaTurretCost) > OgameData.DefenseValue;
            bool isTooCheap = (costOfDefense + plasmaTurretCost) < OgameData.DefenseValue;
            bool isBalanced = !(isTooExpensive || isTooCheap);
            while (!isBalanced)
            {
                int unitToBalanceIdx = rand.Next(0, 6);
                if (isTooExpensive)
                {
                    int numUnitsToRemove = (costOfDefense - OgameData.DefenseValue) / OgameData.DefenseUnitsTotalCosts[unitToBalanceIdx];
                    defense[unitToBalanceIdx] = Math.Max(0, defense[unitToBalanceIdx] - numUnitsToRemove);
                }
                else if (isTooCheap)
                {
                    int numUnitsToAdd = (OgameData.DefenseValue - costOfDefense) / OgameData.DefenseUnitsTotalCosts[unitToBalanceIdx];
                    defense[unitToBalanceIdx] = Math.Min(OgameData.DefenseUnitsMaximums[unitToBalanceIdx],
                                                                    numUnitsToAdd + defense[unitToBalanceIdx]);
                }
                CalculateCost();
                isTooExpensive = (costOfDefense - plasmaTurretCost) > OgameData.DefenseValue;
                isTooCheap = (costOfDefense + plasmaTurretCost) < OgameData.DefenseValue;
                isBalanced = !(isTooExpensive || isTooCheap);
            }
        }

        int CalculateCost()
        {
            costOfDefense = 0;
            for (int i = 0; i < OgameData.DefenseDims; ++i)
            {
                costOfDefense += OgameData.DefenseUnitsTotalCosts[i] * defense[i];
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
            FleetSwarm[] fleetSwarms = new FleetSwarm[OgameData.NumSwarms];
            for (int i = 0; i < OgameData.NumSwarms; ++i)
            {
                fleetSwarms[i] = new FleetSwarm(defense);
            }
            int[] bestGlobalFleetComposition = new int[OgameData.FleetDims];
            long bestGlobalMinAttackCost = int.MaxValue;
            for (int i = 0; i < OgameData.NumSwarms; ++i)
            {
                if (fleetSwarms[i].bestSwarmMinAttackCost < bestGlobalMinAttackCost)
                {
                    bestGlobalMinAttackCost = fleetSwarms[i].bestSwarmMinAttackCost;
                    Array.Copy(fleetSwarms[i].bestSwarmFleetComposition, bestGlobalFleetComposition, OgameData.FleetDims);
                    PrintCurrentNewBestFleetComposition(bestGlobalFleetComposition);
                }
            }


            int epoch = 0;

            while (epoch < (OgameData.MaxEpochs / 10))
            {
                ++epoch;

                if (epoch < OgameData.MaxEpochs)
                {
                    Console.WriteLine("Defense[" + String.Join(",", defense) + "], "
                                        + " Epoch " + epoch
                                        + ", Global Best Minimal Attack Cost = " + bestGlobalMinAttackCost.ToString("F4"));
                }

                for (int i = 0; i < OgameData.NumSwarms; ++i) // each swarm
                {
                    // Shuffle(sequence, rand); // move particles in random sequence
                    for (int j = 0; j < OgameData.NumParticles; ++j) // each particle
                    {
                        //int j = sequence[pj];
                        double p1 = rand.NextDouble();
                        if (p1 < OgameData.ProbDeath)
                        {
                            fleetSwarms[i].fleetParticles[j] = new FleetParticle(defense); // new random position
                            if (fleetSwarms[i].fleetParticles[j].minimalAttackCost < fleetSwarms[i].bestSwarmMinAttackCost) // new swarm best by luck?
                            {
                                fleetSwarms[i].bestSwarmMinAttackCost = fleetSwarms[i].fleetParticles[j].minimalAttackCost;
                                Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, fleetSwarms[i].bestSwarmFleetComposition, OgameData.FleetDims);
                                if (fleetSwarms[i].fleetParticles[j].minimalAttackCost < bestGlobalMinAttackCost) // if a new swarm best, maybe also a new global best?
                                {
                                    bestGlobalMinAttackCost = fleetSwarms[i].fleetParticles[j].minimalAttackCost;
                                    Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, bestGlobalFleetComposition, OgameData.FleetDims);
                                    PrintCurrentNewBestFleetComposition(bestGlobalFleetComposition);
                                }
                            }
                        }
                        // an alternative is to maintain a particle age and die with high prob after a certain age reached
                        // another option is to maintain particle health/weakness (related to either ratio of times improved / loop count
                        // or number consecutive improves or consecutive non-improves) and die with high prob when health is low 

                        double p2 = rand.NextDouble();
                        if (p2 < OgameData.ProbImmigrate)
                        {
                            int otherSwarm = rand.Next(0, OgameData.NumSwarms);
                            int otherParticle = rand.Next(0, OgameData.NumParticles);
                            FleetParticle tmp = fleetSwarms[i].fleetParticles[j];
                            fleetSwarms[i].fleetParticles[j] = fleetSwarms[otherSwarm].fleetParticles[otherParticle];
                            fleetSwarms[otherSwarm].fleetParticles[otherParticle] = tmp;

                            if (fleetSwarms[i].fleetParticles[j].minimalAttackCost < fleetSwarms[otherSwarm].bestSwarmMinAttackCost) // new (other) swarm best?
                            {
                                fleetSwarms[otherSwarm].bestSwarmMinAttackCost = fleetSwarms[i].fleetParticles[j].minimalAttackCost;
                                Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, fleetSwarms[otherSwarm].bestSwarmFleetComposition, OgameData.FleetDims);
                            }
                            if (fleetSwarms[otherSwarm].fleetParticles[otherParticle].minimalAttackCost < fleetSwarms[i].bestSwarmMinAttackCost) // new (curr) swarm best?
                            {
                                fleetSwarms[i].bestSwarmMinAttackCost = fleetSwarms[otherSwarm].fleetParticles[otherParticle].minimalAttackCost;
                                Array.Copy(fleetSwarms[otherSwarm].fleetParticles[otherParticle].fleetComposition, fleetSwarms[i].bestSwarmFleetComposition, OgameData.FleetDims);
                            }
                            // not possible for a new global best
                        }

                        for (int k = 0; k < OgameData.FleetDims; ++k) // update velocity. each x position component
                        {
                            double r1 = rand.NextDouble();
                            double r2 = rand.NextDouble();
                            double r3 = rand.NextDouble();

                            fleetSwarms[i].fleetParticles[j].velocity[k] = (OgameData.Inertia * fleetSwarms[i].fleetParticles[j].velocity[k]) +
                              (OgameData.GravityLocal * r1 * (fleetSwarms[i].fleetParticles[j].bestFleetComposition[k] - fleetSwarms[i].fleetParticles[j].fleetComposition[k])) +
                              (OgameData.GravitySwarm * r2 * (fleetSwarms[i].bestSwarmFleetComposition[k] - fleetSwarms[i].fleetParticles[j].fleetComposition[k])) +
                              (OgameData.GravityGlobal * r3 * (bestGlobalFleetComposition[k] - fleetSwarms[i].fleetParticles[j].fleetComposition[k]));

                            //if (fleetSwarms[i].fleetParticles[j].velocity[k] < minX) // constrain velocities
                            //    fleetSwarms[i].fleetParticles[j].velocity[k] = minX;
                            //else if (fleetSwarms[i].fleetParticles[j].velocity[k] > maxX)
                            //    fleetSwarms[i].fleetParticles[j].velocity[k] = maxX;
                        }

                        for (int k = 0; k < OgameData.FleetDims; ++k) // update position
                        {
                            fleetSwarms[i].fleetParticles[j].fleetComposition[k] += (int)fleetSwarms[i].fleetParticles[j].velocity[k];
                            // constrain all xi
                            if (fleetSwarms[i].fleetParticles[j].fleetComposition[k] < 0)
                                //fleetSwarms[i].fleetParticles[j].fleetComposition[k] = (maxX - minX) * rand.NextDouble() + minX;
                                fleetSwarms[i].fleetParticles[j].fleetComposition[k] = 0;
                            else if (fleetSwarms[i].fleetParticles[j].fleetComposition[k] > ((10 * OgameData.DefenseValue) / OgameData.FleetUnitsTotalCosts[k]))
                                //fleetSwarms[i].fleetParticles[j].fleetComposition[k] = (maxX - minX) * rand.NextDouble() + minX;
                                fleetSwarms[i].fleetParticles[j].fleetComposition[k] = ((10 * OgameData.DefenseValue) / OgameData.FleetUnitsTotalCosts[k]);
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
                                        OgameData.FleetDims);
                        }

                        if (fleetSwarms[i].fleetParticles[j].minimalAttackCost < fleetSwarms[i].bestSwarmMinAttackCost) // new swarm best?
                        {
                            fleetSwarms[i].bestSwarmMinAttackCost = fleetSwarms[i].fleetParticles[j].minimalAttackCost;
                            Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, fleetSwarms[i].bestSwarmFleetComposition, OgameData.FleetDims);
                        }

                        if (fleetSwarms[i].fleetParticles[j].minimalAttackCost < bestGlobalMinAttackCost) // new global best?
                        {
                            bestGlobalMinAttackCost = fleetSwarms[i].fleetParticles[j].minimalAttackCost;
                            Array.Copy(fleetSwarms[i].fleetParticles[j].fleetComposition, bestGlobalFleetComposition, OgameData.FleetDims);
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