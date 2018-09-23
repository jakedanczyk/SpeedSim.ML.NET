using System;

namespace MSOSpeedSim
{
    public class FleetSwarm
    {
        public FleetParticle[] fleetParticles = new FleetParticle[Program.NumParticles];
        public int[] bestSwarmFleetComposition = new int[Program.FleetDims];
        public long bestSwarmFleetCost = long.MaxValue;

        public FleetSwarm(int[] defenseComposition)
        {
            for (int fleetParticleIdx = 0; fleetParticleIdx < Program.NumParticles; ++fleetParticleIdx)
            {
                fleetParticles[fleetParticleIdx] = new FleetParticle(defenseComposition);
                if (fleetParticles[fleetParticleIdx].fleetCost < bestSwarmFleetCost)
                {
                    bestSwarmFleetCost = fleetParticles[fleetParticleIdx].fleetCost;
                    Array.Copy(fleetParticles[fleetParticleIdx].fleetComposition, bestSwarmFleetComposition, Program.FleetDims);
                }
            }
        }
    }
}
