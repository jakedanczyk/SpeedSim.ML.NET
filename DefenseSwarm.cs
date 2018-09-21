using System;

namespace MSOSpeedSim
{
    /// <summary>
    /// Summary description for Class1
    /// </summary>
    public class DefenseSwarm
    {
        public int[] bestSwarmDefense;
        public long bestSwarmMaximinAttackerCost;
        public DefenseParticle[] defenseParticles;

        public DefenseSwarm()
        {
            bestSwarmDefense = new int[Program.DefenseDims];
            bestSwarmMaximinAttackerCost = 0;
            defenseParticles = new DefenseParticle[Program.NumParticles];
            for (int defenseParticleIdx = 0; defenseParticleIdx < Program.NumParticles; ++defenseParticleIdx)
            {
                defenseParticles[defenseParticleIdx] = new DefenseParticle();
                if (defenseParticles[defenseParticleIdx].minAttackerCost > bestSwarmMaximinAttackerCost)
                {
                    bestSwarmMaximinAttackerCost = defenseParticles[defenseParticleIdx].minAttackerCost;
                    Array.Copy(defenseParticles[defenseParticleIdx].defense, bestSwarmDefense, Program.DefenseDims);
                }
            }
        }
    }
}