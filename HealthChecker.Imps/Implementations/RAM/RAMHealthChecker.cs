using HealthChecker.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthChecker.Contracts.Models;
using System.Management;

namespace HealthChecker.Imps.Implementations
{
    public class RAMHealthChecker : IHealthCheck
    {
        public double TotalPhysicalMemory { get; private set; }
        public double FreeMemorySize { get; private set; }
        public double PercentAvailable { get; private set; }


        public HealthCheckExecutionResult Execute()
        {
            try
            {
                this.GetRamStatus();
                return new HealthCheckExecutionResult();
            }
            catch (Exception e)
            {
                return new HealthCheckExecutionResult().AddError(e);
            }
        }


        private void GetRamStatus()
        {

            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
            {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();

            if (memoryValues != null)
            {
                this.TotalPhysicalMemory = memoryValues.TotalVisibleMemorySize;
                this.FreeMemorySize = memoryValues.FreePhysicalMemory;
                var percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
                this.PercentAvailable = percent;
            }

        }
    }
}
