using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedSimML
{
    public class FleetMSO
    {
        public int NumSwarms = 4;
        public int NumParticle = 5;

        static void Main(string[] args)
        {
            Console.WriteLine("Initializing");
            Console.WriteLine("Begin SpeedSim ML: Fleet Multi Swarm Optimizer");

            SetupFleetMSO();
        }


        static void SetupFleetMSO()
        {
            Console.WriteLine("\nEnter target composition\n");

            //Get the target composition from user
            int[] targetComposition = new int[Units.UnitDict.Count];
            var unitTypeArray = Enum.GetValues(typeof(UnitType));
            int unitTypeIdx = 0;
            foreach (UnitType unitType in unitTypeArray)
            {
                bool isWaitingValidInput = true;
                while (isWaitingValidInput)
                {
                    Console.WriteLine(Units.UnitDict[unitType].Name);
                    string input = Console.ReadLine();
                    try
                    {
                        int unitCount = Math.Abs(Int32.Parse(input));
                        targetComposition[unitTypeIdx] = unitCount;
                        isWaitingValidInput = false;
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine(String.Format("\n{0}\n", e.Message));
                    }
                }
                unitTypeIdx++;
            }
            targetComposition = OgameLogic.LimitShieldCount(targetComposition);

            //Ask the user which types of units to include in attack
            List<UnitType> attackUnitTypes = new List<UnitType>();
            Console.WriteLine("Select unit types for attack fleet composition...");
            foreach (UnitType unitType in unitTypeArray)
            {
                if (Units.GetUnit(unitType) is Ship)
                {
                    while (true)
                    {
                        Console.WriteLine(Units.UnitDict[unitType].Name + "? (y or n)");
                        char selectKey = Console.ReadKey().KeyChar;
                        if (selectKey == 'y')
                        {
                            attackUnitTypes.Add(unitType);
                            break;
                        }
                        else if (selectKey == 'n')
                        {
                            break;
                        }
                    }
                }
            }

            //Get user engine tech levels
            Console.WriteLine("Enter Combustion Drive level");
            int[] driveLevels = new int[3];
            var driveTypeArr = Enum.GetValues(typeof(DriveType));

            for (int driveIdx = 0; driveIdx < driveTypeArr.Length; driveIdx++)
            {
                while (true)
                {
                    string input = Console.ReadLine();
                    try
                    {
                        int driveLevel = Math.Abs(Int32.Parse(input));
                        driveLevels[driveIdx] = driveLevel;
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine(String.Format("\n{0}\n", e.Message));
                    }
                }
            }

            //Solve with MSO
            Dictionary<UnitType, int> optimumAttackFleet = Solve(targetComposition, attackUnitTypes);

            Console.WriteLine("Best attack fleet found: ");
            foreach(UnitType in attackUnitTypes)

            //int indexOfLastShip = Dictionary.IndexOf(UnitNames, "Battlecruiser");
            //for (int outputIdx = 0; outputIdx < indexOfLastShip)
        }

        IDictionary<UnitType, int> Solve(int[] target, List<UnitType> attackUnits)
        {
            Dictionary<UnitType, int> globalBestAttackFleet = new Dictionary<UnitType, int>();
            foreach(UnitType unitType in attackUnits)
            {
                globalBestAttackFleet.Add(unitType, 0);
            }

            return globalBestAttackFleet;
        }

        void AttackCost()
        {

        }

        void FuelCost
    }
}
