using HealthChecker.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthChecker.Contracts.Models;
using System.ServiceProcess;

namespace HealthChecker.Imps.Implementations
{
    public class IISHealthChecker : IHealthCheck
    {
        public bool IsRunning { get; private set; }
        public HealthCheckExecutionResult Execute()
        {
            try
            {
                this.IsRunning = this.CheckIISStatus();
                return new HealthCheckExecutionResult();
            }
            catch (Exception e)
            {
                return new HealthCheckExecutionResult().AddError(e);
            }
        }


        private bool CheckIISStatus()
        {
            ServiceController sc = new ServiceController("World Wide Web Publishing Service");
            return sc.Status.Equals(ServiceControllerStatus.Running);
        }
    }
}
