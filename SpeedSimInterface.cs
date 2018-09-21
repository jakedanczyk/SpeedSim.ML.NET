using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MSOSpeedSim
{
    public static class SpeedSimInterface
    {
        static string DataFile = "C:/Users/admin/source/repos/SpeedSim.ML.NET/data.txt";
        static string ResultFile = "C:/Users/admin/source/repos/SpeedSim.ML.NET/result.txt";
        static int LineNumberOfResultsInOutput = 14; //1-indexed
        static int IndexOfMetalLossInResultsString = 2; //0-indexed
        static int IndexOfCrystallLossInResultsString = 4; //0-indexed
        static int IndexOfDeuteriumLossInResultsString = 6; //0-indexed

        static Process cmdProcess = new Process();
        static StreamWriter cmdInput;
        private static StringBuilder cmdOutput = null;

        static SpeedSimInterface()
        {
            cmdOutput = new StringBuilder("");
            cmdProcess = new Process();

            cmdProcess.StartInfo.FileName = "cmd.exe";
            cmdProcess.StartInfo.UseShellExecute = false;
            cmdProcess.StartInfo.CreateNoWindow = true;
            cmdProcess.StartInfo.RedirectStandardOutput = true;

            cmdProcess.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
            cmdProcess.StartInfo.RedirectStandardInput = true;
            cmdProcess.Start();

            cmdInput = cmdProcess.StandardInput;
            cmdProcess.BeginOutputReadLine();

            cmdInput.WriteLine("cd C:\\Users\\admin\\source\\repos\\SpeedSim.ML.NET");

            Console.WriteLine(cmdOutput.ToString());
        }

        private static void SortOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                cmdOutput.Append(Environment.NewLine + outLine.Data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>[Metal Loss, Crystal Loss, Deut Loss]</returns>
        public static void RunSpeedSim()
        {
            string output = string.Empty;
            string error = string.Empty;

            cmdInput.WriteLine("C:\\Users\\admin\\source\\repos\\SpeedSim.ML.NET\\speedsim.exe");

            bool isSimulating = true;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (isSimulating)
            {
                Process[] runningProcess = Process.GetProcessesByName("speedsim");
                if (runningProcess.Length == 0)
                {
                    isSimulating = false;
                }
                //else if (stopwatch.ElapsedMilliseconds > 2000)
                //{
                //    runningProcess[0].Kill();
                //}
            }
        }


        public static void FormatDataFile(int[] fleetComposition, int[] defenseComposition, int numSimulations = 5)
        {
            //there are several ships and dense structures not used as paremeters in the optimization
            List<int> fullFleetComposition = new List<int>(fleetComposition);
            fullFleetComposition.Insert(6, 0); //colony ship is 7th in fleet string
            fullFleetComposition.Insert(7, 0); //recycler is 8th in fleet string
            fullFleetComposition.Insert(8, 0); //espionage probe is 9th in fleet string
            fullFleetComposition.Insert(10, 0); //satellite is 11th in fleet string
            fullFleetComposition.Insert(12, 0); //death star is 13th in fleet string
            string fleetString = String.Join(",", fullFleetComposition);

            List<int> fullDefenseComposition = new List<int>(defenseComposition);
            int[] defenseShipCounts = new int[14]; //14 types of ship, 0 of each in the defense
            fullDefenseComposition.InsertRange(0, defenseShipCounts);
            fullDefenseComposition.Add(1); //Small shield dome
            fullDefenseComposition.Add(1); //Large shield dome
            string defenseString = String.Join(",", fullDefenseComposition);

            string numSimulationsString = numSimulations.ToString();

            string[] dataStrings = { fleetString, defenseString, numSimulationsString };

            //clear file
            File.WriteAllText(DataFile, "");
            File.WriteAllLines(DataFile, dataStrings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns>[Metal Loss, Crystal Loss, Deut Loss]</returns>
        public static int[] GetResults()
        {
            string[] strings = File.ReadAllLines(ResultFile);

            int[] results = new int[4];

            for(int i = 0; i < 4; ++i)
            {
                results[i] = Convert.ToInt32(strings[i]);
            }

            return results;
        }
    }
}