using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ubisoft.Interview.SessionFeedback.DA
{
    public interface IDynamoDBConnectionFactory
    {
        (IAmazonDynamoDB amazonDynamoDB, IDynamoDBContext context) Create();
    }
    public class DynamoDBConnectionFactory : IDynamoDBConnectionFactory
    {
        DatabaseConfig _databaseConfig;
        public DynamoDBConnectionFactory(IOptions<DatabaseConfig> optionsAccessor )
        {
            this._databaseConfig = optionsAccessor.Value;
        }

        public (IAmazonDynamoDB amazonDynamoDB, IDynamoDBContext context) Create()
        {
            var config = new AmazonDynamoDBConfig();
            config.ServiceURL = "http://localhost:"+ _databaseConfig.Port;
            var client = new AmazonDynamoDBClient(config);
            return (client, new DynamoDBContext(client));
        }
    }
}
