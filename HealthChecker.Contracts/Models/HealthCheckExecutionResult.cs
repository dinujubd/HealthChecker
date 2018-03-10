using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthChecker.Contracts.Models
{
    public class HealthCheckExecutionResult
    {
        public HealthCheckExecutionResult()
        {
            this.Errors = new List<Exception>();
        }
        public bool IsSuccessful
        {
            get => Errors.Count() == 0;
        }
        public List<Exception> Errors { get; set; }

        public HealthCheckExecutionResult AddError(Exception e)
        {
            if (this.Errors == null)
            {
                this.Errors = new List<Exception>();
            }
            this.Errors.Add(e);
            return this;
        }
    }
}
