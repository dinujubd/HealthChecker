using HealthChecker.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthChecker.Contracts.Models;
using System.Diagnostics;
using System.Threading;

namespace HealthChecker.Imps.Implementations
{
    public class CPUHealthChecker : IHealthCheck
    {
        public double TotalCpuUsage { get; private set; }
        public int TotalProcessorCount { get; private set; }

        public HealthCheckExecutionResult Execute()
        {
            try
            {
                this.GetCPUStatus();
                return new HealthCheckExecutionResult();
            }
            catch (Exception e)
            {
                return new HealthCheckExecutionResult().AddError(e);
            }
        }

        private void GetCPUStatus()
        {
            PerformanceCounter theCPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            CounterSample cs1 = theCPUCounter.NextSample();
            Thread.Sleep(100);
            CounterSample cs2 = theCPUCounter.NextSample();

            this.TotalCpuUsage = CounterSample.Calculate(cs1, cs2);
            this.TotalProcessorCount = Environment.ProcessorCount;

        }
    }
}
