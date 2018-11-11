using System;
using System.Collections.Generic;
using System.Text;

namespace OgameDefenseMSO
{
    public class Defense
    {
        public Defense()
        {
        }

        public Defense(int[] counts)
        {
            Array.Copy(counts, DefenseCounts, 6);
        }

        public int[] DefenseCounts = new int[6];

        public int RocketLauncherCount
        {
            get { return DefenseCounts[0]; }
            set { DefenseCounts[0] = value; }
        }
        public int LightLaserCount
        {
            get { return DefenseCounts[1]; }
            set { DefenseCounts[1] = value; }
        }
        public int HeavyLaserCount
        {
            get { return DefenseCounts[2]; }
            set { DefenseCounts[2] = value; }
        }
        public int GaussCannonCount
        {
            get { return DefenseCounts[3]; }
            set { DefenseCounts[3] = value; }
        }
        public int IonCannonCount
        {
            get { return DefenseCounts[4]; }
            set { DefenseCounts[4] = value; }
        }
        public int PlasmaTurretCount
        {
            get { return DefenseCounts[5]; }
            set { DefenseCounts[5] = value; }
        }

        public void CopyDefense(Defense sourceDefense)
        {
            Array.Copy(sourceDefense.DefenseCounts, DefenseCounts, Program.DefenseDims);
        }
    }
}
