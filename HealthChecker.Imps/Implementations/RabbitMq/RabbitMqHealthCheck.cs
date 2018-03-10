using HealthChecker.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthChecker.Contracts.Models;
using RabbitMQ.Client;

namespace HealthChecker.Imps.Implementations
{
    public class RabbitMqHealthCheck : IHealthCheck
    {
        public bool IsRunning { get; private set; }

        public HealthCheckExecutionResult Execute()
        {
            try
            {
                this.IsRunning = this.IsUp("");
                return new HealthCheckExecutionResult();
            }
            catch (Exception e)
            {
                return new HealthCheckExecutionResult().AddError(e);
            }
        }

        private bool IsUp(string host)
        {
            var factory = new ConnectionFactory() { HostName = host };
            try
            {
                var connection = factory.CreateConnection();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
