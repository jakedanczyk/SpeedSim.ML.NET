using System;

namespace SpeedSimML
{
    public class FleetSwarm
    {
        public FleetParticle[] fleetParticles = new FleetParticle[OgameData.NumParticles];
        public int[] bestSwarmFleetComposition = new int[OgameData.FleetDims];
        public long bestSwarmMinAttackCost = long.MaxValue;

        public FleetSwarm(int[] defenseComposition)
        {
            for (int fleetParticleIdx = 0; fleetParticleIdx < OgameData.NumParticles; ++fleetParticleIdx)
            {
                fleetParticles[fleetParticleIdx] = new FleetParticle(defenseComposition);
                if (fleetParticles[fleetParticleIdx].attackCost < bestSwarmMinAttackCost)
                {
                    bestSwarmMinAttackCost = fleetParticles[fleetParticleIdx].attackCost;
                    Array.Copy(fleetParticles[fleetParticleIdx].fleetComposition, bestSwarmFleetComposition, OgameData.FleetDims);
                }
            }
        }
    }
}
