using HealthChecker.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthChecker.Contracts.Models;
using StackExchange.Redis;

namespace HealthChecker.Imps.Implementations
{
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly IConnectionMultiplexer multiplexer;
        public bool IsRunning { get; private set; }
        public RedisHealthCheck(IConnectionMultiplexer multiplexer)
        {
            this.multiplexer = multiplexer;
        }
        public HealthCheckExecutionResult Execute()
        {
            try
            {
                this.IsRunning = this.IsUp();
                return new HealthCheckExecutionResult();
            }
            catch (Exception e)
            {
                return new HealthCheckExecutionResult().AddError(e);
            }
        }

        private bool IsUp()
        {
            return multiplexer.IsConnected;
        }
    }
}
