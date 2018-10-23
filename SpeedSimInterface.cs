using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OgameDefenseMSO
{
    public static class SpeedSimInterface
    {
        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern void Init();

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern void Reset();

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern void SetLoot(int[] Loot);

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern void SetSystemsApart(int systemsApart);

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern void SetTechs(int cDrive,
                                            int iDrive,
                                            int hDrive,
                                            int attackWeapon,
                                            int attackShield,
                                            int attackArmor,
                                            int defWeapon,
                                            int defShield,
                                            int defArmor);

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern void SetFleetInt(int[] Attacker, int[] Defender);

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern void Simulate(int numTrials);

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern IntPtr GetAttackLosses();

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern float GetAttackWinPercent();

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern int GetAttackMetalLoss();

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern int GetAttackCrystalLoss();

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern int GetAttackDeuteriumLoss();

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern long GetFuelConsumption();

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern uint GetFlightTime();

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern ulong GetDebrisMetal();

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern ulong GetDebrisCrystal();

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern ulong GetDebrisDeuterium();

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern ulong GetLootMetal();

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern ulong GetLootCrystal();

        [DllImport("C:\\Users\\admin\\source\\repos\\SpeedSimML\\SpeedSimML\\SpeedSimLib.dll")]
        public static extern ulong GetLootDeuterium();
    }
}
