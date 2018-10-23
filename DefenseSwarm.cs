using System;

namespace OgameDefenseMSO
{
    /// <summary>
    /// Summary description for Class1
    /// </summary>
    public class DefenseSwarm
    {
        public Defense lBestDefense;
        public double lBestError = double.MaxValue;
        public DefenseParticle[] defenseParticles;

        public DefenseSwarm()
        {
            lBestDefense = new Defense();
            defenseParticles = new DefenseParticle[Program.NumParticles];
            for (int defenseParticleIdx = 0; defenseParticleIdx < Program.NumParticles; ++defenseParticleIdx)
            {
                defenseParticles[defenseParticleIdx] = new DefenseParticle();
                //penalize initial errors since we aren't doing triple checking of new gBests
                defenseParticles[defenseParticleIdx].error = defenseParticles[defenseParticleIdx].pBestError *= 1.5;
                if (defenseParticles[defenseParticleIdx].error < lBestError)
                {
                    lBestError = defenseParticles[defenseParticleIdx].error;
                    lBestDefense.CopyDefense(defenseParticles[defenseParticleIdx].defense);
                }
            }
        }
    }
}