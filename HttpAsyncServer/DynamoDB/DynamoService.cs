using System.Collections.Generic;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace HttpAsyncServer.DynamoDB
{
    public class DynamoService : ITableDataService
    {
        public AmazonDynamoDBClient DynamoClient { get; private set; }
        private AmazonDynamoDBConfig dynamoConfig;
        private AWSCredentials awsCredentials;
        private DynamoDBContext dbContext;
        public string DynamoServiceUrl { get; private set; }

        public DynamoService(string dynamoServiceUrl, string awsAccessKey, string awsSecretKey)
        {
            DynamoServiceUrl = dynamoServiceUrl;

            System.Console.WriteLine("Starting dynamoDB cliente...");

            awsCredentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
            dynamoConfig = new AmazonDynamoDBConfig() { ServiceURL = DynamoServiceUrl };

            DynamoClient = new AmazonDynamoDBClient(awsCredentials, dynamoConfig);

            System.Console.WriteLine("DynamoDB client started!");
        }

        public void RestartDynamoDBContext()
        {
            dbContext = CreateDynamoDBContext();
        }

        private DynamoDBContext CreateDynamoDBContext()
        {
            DynamoClient = new AmazonDynamoDBClient(awsCredentials, dynamoConfig);

            return new DynamoDBContext(DynamoClient, new DynamoDBContextConfig
            {
                ConsistentRead = true,
                SkipVersionCheck = true
            });
        }
        
        public void SaveItem<T>(T item) where T : ITable
        {
            if (item != null && string.IsNullOrEmpty(item.Id))
            {
                item.Id = KeyGenerator.GenerateHashId();
            }

            dbContext = CreateDynamoDBContext();
            dbContext.Save(item);
        }

        public void BatchStore<T>(IEnumerable<T> items) where T : class
        {
            dbContext = CreateDynamoDBContext();
            var itemBatch = dbContext.CreateBatchWrite<T>();

            foreach (var item in items)
            {
                itemBatch.AddPutItem(item);
            }

            itemBatch.Execute();
        }

        public IEnumerable<T> GetAll<T>(string propertyName, string value) where T : class
        {
            ScanCondition scan = new ScanCondition(propertyName, Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, value);

            dbContext = CreateDynamoDBContext();
            IEnumerable<T> items = dbContext.Scan<T>(scan);
            return items;
        }

        public T GetItem<T>(string hashKey) where T : class
        {
            if(string.IsNullOrEmpty(hashKey))
            {
                return null;
            }

            dbContext = CreateDynamoDBContext();
            return dbContext.Load<T>(hashKey);
        }

        public T GetItem<T>(string hashKey, string rangeKey) where T : class
        {
            if (string.IsNullOrEmpty(hashKey))
            {
                return null;
            }

            dbContext = CreateDynamoDBContext();
            return dbContext.Load<T>(hashKey, rangeKey);
        }

        public void UpdateItem<T>(T item) where T : class
        {
            T savedItem = dbContext.Load(item);

            if (savedItem == null)
            {
                throw new AmazonDynamoDBException("Item does not exist in the Table");
            }

            dbContext = CreateDynamoDBContext();
            dbContext.Save(item);
        }

        public void DeleteItem<T>(T item)
        {
            dbContext = CreateDynamoDBContext();
            T savedItem = dbContext.Load(item);

            if (savedItem == null)
            {
                throw new AmazonDynamoDBException("Item does not exist in the Table");
            }

            dbContext.Delete(savedItem);
        }
    }
}