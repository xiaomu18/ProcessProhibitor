using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessProhibitor
{
    internal class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        static int millisecond = 1;

        static void Main(string[] args)
        {
            if (args.Length < 1) {
                Console.WriteLine("Usage: ProcessProhibitor [Process Name (without suffix)] [name 2] [name 3] ...");
                Console.WriteLine("\n       Additional Options:");
                Console.WriteLine("       -t:[value: int] Set the number of threads ending each process (Default to 1)");
                Console.WriteLine("       -m:[value: int] Set delay milliseconds ending process (Default to 1)");
                return;
            }

            int thread_num = 1;
            List<string> ProcessesList = new List<string>();

            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    if (arg.StartsWith("-t:"))
                    {
                        string value = arg.Substring(3);
                        int intValue;

                        if (int.TryParse(value, out intValue))
                        {
                            Console.WriteLine($"[ INFO ] Set the number of threads to {intValue}.");
                            thread_num = intValue;
                        }
                    } else if (arg.StartsWith("-m:"))
                    {
                        string value = arg.Substring(3);
                        int intValue;

                        if (int.TryParse(value, out intValue))
                        {
                            Console.WriteLine($"[ INFO ] Set delay milliseconds to {intValue}.");
                            millisecond = intValue;
                        }
                    } else
                    {
                        Console.WriteLine($"[ INFO ] Unknown Parameter {arg}.");
                    }
                }
                else
                {
                    ProcessesList.Add(arg);
                }
            }

            if (ProcessesList.Count < 1)
            {
                Console.WriteLine("[ INFO ] No Process Executing Operation.");
                return;
            }

            int started_threads = 0;

            foreach (string ProcessesName in ProcessesList)
            {
                for (int i = 1; i <= thread_num; i++)
                {
                    // 启动一个新任务
                    Task.Run(() => Killer(ProcessesName));
                    started_threads++;
                }
            }

            Console.WriteLine($"[ INFO ] {started_threads} started done.");
            Console.WriteLine("[ INFO ] Press Enter to stop the operation.");
            Console.WriteLine();
            Console.ReadLine();

            Console.WriteLine("[ INFO ] Exit.");
            Environment.Exit(0);
        }

        private static void Killer(string ProcessesName)
        {
            while (true)
            {
                Process[] processes = Process.GetProcessesByName(ProcessesName);
                foreach (Process process in processes)
                {
                    try
                    {
                        TerminateProcess(process.Handle, 0);
                        Console.WriteLine("[ SUCCESS - {0} ] {1}: {2}", ProcessesName, process.Id, "Killed.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[ INFO - {0} ] {1}: {2}", ProcessesName, process.Id, ex.Message);
                    }
                }

                Thread.Sleep(millisecond);
            }
        }
    }
}
