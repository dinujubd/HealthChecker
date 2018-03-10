using HealthChecker.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthChecker.Contracts.Models;
using StackExchange.Redis;
using MongoDB.Driver;
using MongoDB.Bson;

namespace HealthChecker.Imps.Implementations
{
    public class MongoHealthCheck : IHealthCheck
    {
        private readonly IMongoDatabase database;
        public bool IsRunning { get; private set; }
        public MongoHealthCheck(IMongoDatabase database)
        {
            this.database = database;
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
            database.RunCommand((Command<BsonDocument>)"{ping:1}");             
            return true;
        }
    }
}
