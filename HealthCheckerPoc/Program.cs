using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Management;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Owin.Hosting;
using HealthChecker.Web;
using System.Net.Http;

namespace HealthCheckerPoc
{
    class Program
    {

        static void Main(string[] args)
        {


            string baseAddress = "http://localhost:9000/";

            // Start OWIN host 
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                // Create HttpCient and make a request to api/values 
                HttpClient client = new HttpClient();

                var response = client.GetAsync(baseAddress + "api/values").Result;

                Console.WriteLine(response);
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                Console.ReadLine();
            }

            //CpuUsage cpuUsage = new CpuUsage();
            //MemUsage memUsage = new MemUsage();

            //while (true)
            //{
            //    cpuUsage.Display();
            //    memUsage.Display();


            //    Thread.Sleep(1000);
            //    Console.Clear();
            //}
            //foreach (var item in cpuUsage.DetailDisplay())
            //{
            //    Console.WriteLine(item);
            //}

            //cpuUsage.DisplayProcessState();
            //Console.ReadLine();

        }
    }


    class MemUsage
    {

        public void Display()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
            {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();

            if (memoryValues != null)
            {
                var percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
                Console.WriteLine("Total memory usage :" + percent);
            }

        }

    }

    class CpuUsage
    {
        public void Display()
        {
            PerformanceCounter theCPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            CounterSample cs1 = theCPUCounter.NextSample();
            Thread.Sleep(100);
            CounterSample cs2 = theCPUCounter.NextSample();
            float finalCpuCounter = CounterSample.Calculate(cs1, cs2);
            Console.WriteLine("Total Cpu Usage:" + finalCpuCounter);
        }


        public void DisplayProcessState()
        {

            var processesC = Process.GetProcesses();
            foreach (var item in processesC)
            {
                Console.WriteLine(item.ProcessName + " " + item.IsRunning());
            }
        }

        public void DetailDisplay()
        {
            var processesC = Process.GetProcesses();
            var processDict = new Dictionary<string, float>();

            foreach (var item in processesC)
            {
                var perfCounter = new PerformanceCounter("Process", "% Processor Time", item.ProcessName);


                // Initialize to start capturing
                perfCounter.NextValue();
                perfCounter.NextValue();
                perfCounter.NextValue();
                //Thread.Sleep(1000);

                float cpu = perfCounter.NextValue() / Environment.ProcessorCount;
                try
                {
                    var dictItem = processDict[item.ProcessName];
                    dictItem = dictItem + cpu;
                }
                catch
                {

                    processDict.Add(item.ProcessName, cpu);
                }







                //CounterSample cs1 = perfCounter.NextSample();
                ////    System.Threading.Thread.Sleep(100);
                //CounterSample cs2 = perfCounter.NextSample();
                //float finalCpuCounter = CounterSample.Calculate(cs1, cs2);
                //if (finalCpuCounter > 0)
                //    Console.WriteLine(item.ProcessName + " Using " + finalCpuCounter + "% cpu");

                //  yield return finalCpuCounter;

            }

            foreach (var item in processDict)
            {
                Console.WriteLine(item.Key + " CPU: " + item.Value);
            }
        }


    }


    /// <summary>
    /// Class to retrieve the CPU values.
    /// </summary>
    /// <remarks></remarks>
    public class CPUStatus
    {

        #region "Members"
        private ProcessTimes _ProcessTimes = new ProcessTimes();
        private long _OldUserTime;
        private long _OldKernelTime;
        private DateTime _OldUpdate;
        private Int32 _RawUsage;
        private object _Lock = new object();
        private IntPtr _processHandle;
        #endregion

        #region "Constructor"
        /// <summary>
        /// Initializes the CPUStatus instance
        /// </summary>
        /// <param name="process">The process to monitor</param>
        public CPUStatus(System.Diagnostics.Process process)
        {
            _OldUpdate = DateTime.MinValue;
            _processHandle = process.Handle;

            InitValues();
        }
        #endregion

        #region Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetProcessTimes(IntPtr hProcess, out long lpCreationTime, out long lpExitTime, out long lpKernelTime, out long lpUserTime);
        #endregion

        #region "Private methods"
        /// <summary>
        /// Retrieve the initial values
        /// </summary>
        /// <remarks></remarks>
        private void InitValues()
        {
            try
            {

                if ((GetProcessTimes(_processHandle, out _ProcessTimes.RawCreationTime, out _ProcessTimes.RawExitTime, out _ProcessTimes.RawKernelTime, out _ProcessTimes.RawUserTime)))
                {
                    // convert the values to DateTime values
                    _ProcessTimes.ConvertTime();

                    _OldUserTime = _ProcessTimes.UserTime.Ticks;
                    _OldKernelTime = _ProcessTimes.KernelTime.Ticks;
                    _OldUpdate = DateTime.Now;
                }
            }
            catch (Exception)
            {
                _OldUpdate = DateTime.MinValue;
            }
        }

        /// <summary>
        /// Refreshes the usage values
        /// </summary>
        /// <remarks></remarks>
        private void Refresh()
        {
            lock (_Lock)
            {

                if ((GetProcessTimes(_processHandle, out _ProcessTimes.RawCreationTime, out _ProcessTimes.RawExitTime, out _ProcessTimes.RawKernelTime, out _ProcessTimes.RawUserTime)))
                {
                    // convert the values to DateTime values
                    _ProcessTimes.ConvertTime();

                    UpdateCPUUsage(_ProcessTimes.UserTime.Ticks, _ProcessTimes.KernelTime.Ticks);
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not retrieve process times");
                }
            }
        }

        /// <summary>
        /// updates the cpu usage (cpu usgae = UserTime + KernelTime)
        /// </summary>
        /// <param name="newUserTime"></param>
        /// <param name="newKernelTime"></param>
        /// <remarks></remarks>
        private void UpdateCPUUsage(long newUserTime, long newKernelTime)
        {
            long UpdateDelay = 0;
            long UserTime = newUserTime - _OldUserTime;
            long KernelTime = newKernelTime - _OldKernelTime;

            if (_OldUpdate == DateTime.MinValue)
            {
                _RawUsage = Convert.ToInt32((UserTime + KernelTime) * 100);
            }
            else
            {
                // eliminates "divided by zero"
                if (DateTime.Now.Ticks == _OldUpdate.Ticks)
                    Thread.Sleep(100);

                UpdateDelay = DateTime.Now.Ticks - _OldUpdate.Ticks;

                _RawUsage = Convert.ToInt32(((UserTime + KernelTime) * 100) / UpdateDelay);
            }

            _OldUserTime = newUserTime;
            _OldKernelTime = newKernelTime;
            _OldUpdate = DateTime.Now;
        }
        #endregion

        #region "Properties"
        /// <summary>
        /// Gets the CPU usage
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public Int32 RawUsage
        {
            get
            {
                lock (_Lock)
                {
                    Refresh();
                    return _RawUsage;
                }
            }
        }
        #endregion

        #region "internal classes"
        private struct ProcessTimes
        {
            public DateTime CreationTime;
            public DateTime ExitTime;
            public DateTime KernelTime;
            public DateTime UserTime;

            public long RawCreationTime;
            public long RawExitTime;
            public long RawKernelTime;
            public long RawUserTime;

            public void ConvertTime()
            {
                CreationTime = FiletimeToDateTime(RawCreationTime);
                ExitTime = FiletimeToDateTime(RawExitTime);
                KernelTime = FiletimeToDateTime(RawKernelTime);
                UserTime = FiletimeToDateTime(RawUserTime);
            }

            private DateTime FiletimeToDateTime(long FileTime)
            {
                try
                {
                    return DateTime.FromFileTimeUtc(FileTime);
                }
                catch (Exception)
                {
                    return new DateTime();
                }
            }
        }
        #endregion

    }


    public static class ProcessExtensions
    {
        public static bool IsRunning(this Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");

            try
            {
                Process.GetProcessById(process.Id);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }
    }
}
