using System;

namespace OgameDefenseMSO
{
    public class FleetSwarm
    {
        public FleetParticle[] fleetParticles = new FleetParticle[Program.NumParticles];
        public Fleet lBestFleet = new Fleet();
        public double lBestProfits = double.MinValue;

        public FleetSwarm(Defense targetDefense)
        {
            for (int fleetParticleIdx = 0; fleetParticleIdx < Program.NumParticles; ++fleetParticleIdx)
            {
                fleetParticles[fleetParticleIdx] = new FleetParticle(targetDefense);
                if (fleetParticles[fleetParticleIdx].profits > lBestProfits)
                {
                    lBestProfits = fleetParticles[fleetParticleIdx].profits;
                    lBestFleet.CopyFleet(fleetParticles[fleetParticleIdx].fleet);
                }
            }
        }
    }
}
