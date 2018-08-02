using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;

namespace HttpAsyncServer.DynamoDB
{
    public class DynamoTableManager
    {
        private const string ACTIVE_STATUS = "active";
        private DynamoService dynamoService;
        private List<string> tables;

        public DynamoTableManager(DynamoService dynamoService)
        {
            this.dynamoService = dynamoService;
            tables = dynamoService.DynamoClient.ListTables().TableNames;
        }

        private bool CheckTableExists(string tableName)
        {
            if(tables != null && !string.IsNullOrEmpty(tableName))
            {
                return tables.Contains(tableName);
            }

            return false;
        }

        public void CreateNewTable(Type entity, Type entityBaseType)
        {
            if(dynamoService == null || entity == null || !entityBaseType.IsAssignableFrom(entity))
            {
                return;
            }

            try
            {
                DynamoDBTableAttribute attr = entity.GetCustomAttribute<DynamoDBTableAttribute>();

                if(attr == null)
                {
                    return;
                }

                string tableName = attr.TableName.ToLower();

                if (CheckTableExists(tableName))
                {
                    ConsoleHelper.Write("\nTable ");
                    ConsoleHelper.Write(tableName.ToUpper(), ConsoleColor.Yellow);
                    ConsoleHelper.Write(string.Format(" is active in dynamoDB - {0}", dynamoService.DynamoServiceUrl));
                    return;
                }

                dynamoService.RestartDynamoDBContext();

                using (IAmazonDynamoDB client = dynamoService.DynamoClient)
                {
                    CreateTableRequest createTableRequest = new CreateTableRequest();
                    createTableRequest.TableName = tableName;
                    createTableRequest.ProvisionedThroughput = new ProvisionedThroughput() { ReadCapacityUnits = 1, WriteCapacityUnits = 1 };

                    List<KeySchemaElement> keySchemas = new List<KeySchemaElement>();
                    List<AttributeDefinition> attributeDefinitions = new List<AttributeDefinition>();

                    Attribute hashAttr = null;
                    Attribute rangeAttr = null;

                    foreach (PropertyInfo property in entity.GetProperties())
                    {
                        hashAttr = property.GetCustomAttribute(typeof(DynamoDBHashKeyAttribute), false);
                        rangeAttr = property.GetCustomAttribute(typeof(DynamoDBRangeKeyAttribute), false);

                        if (hashAttr != null)
                        {
                            keySchemas.Add(new KeySchemaElement(property.Name, KeyType.HASH));
                            attributeDefinitions.Add(new AttributeDefinition(property.Name, TypeToScalarAttributeType(property.GetType())));
                        }
                        else if (rangeAttr != null)
                        {
                            keySchemas.Add(new KeySchemaElement(property.Name, KeyType.RANGE));
                            attributeDefinitions.Add(new AttributeDefinition(property.Name, TypeToScalarAttributeType(property.GetType())));
                        }
                    }

                    createTableRequest.KeySchema = keySchemas;
                    createTableRequest.AttributeDefinitions = attributeDefinitions;

                    CreateTableResponse createTableResponse = client.CreateTable(createTableRequest);

                    TableDescription tableDescription = createTableResponse.TableDescription;

                    ConsoleHelper.Write("\n\nTable ");
                    ConsoleHelper.Write(tableName.ToUpper(), ConsoleColor.Yellow);
                    ConsoleHelper.Write(" creation command sent to Amazon...");

                    string tableStatus = tableDescription.TableStatus.Value.ToLower();

                    while (!ACTIVE_STATUS.Equals(tableStatus))
                    {
                        ConsoleHelper.Write("\nTable ");
                        ConsoleHelper.Write(tableName.ToUpper(), ConsoleColor.Yellow);
                        ConsoleHelper.WriteLine(" is not yet active, waiting...");

                        //Waiting for the table to be created in amazon or locally, check again each x milliseconds
                        Thread.Sleep(2000);

                        DescribeTableRequest describeTableRequest = new DescribeTableRequest(tableName);
                        DescribeTableResponse describeTableResponse = client.DescribeTable(describeTableRequest);

                        tableDescription = describeTableResponse.Table;
                        tableStatus = tableDescription.TableStatus.Value.ToLower();

                        ConsoleHelper.Write("\nLatest status of table ");
                        ConsoleHelper.Write(string.Format("{0}: ", tableName.ToUpper()), ConsoleColor.Yellow);
                        ConsoleHelper.Write(tableStatus.ToUpper(), ConsoleColor.Green);
                    }

                    ConsoleHelper.Write("\nCreation complete for table ");
                    ConsoleHelper.Write(tableName.ToUpper(), ConsoleColor.Yellow);
                    ConsoleHelper.Write(", final status: ");
                    ConsoleHelper.Write(tableStatus.ToUpper(), ConsoleColor.Green);
                }
            }
            catch (AmazonDynamoDBException exception)
            {
                ConsoleHelper.Write(string.Format("\nException while creating new dynamoDB table: {0}.", exception.Message), ConsoleColor.Red);
                ConsoleHelper.Write(string.Format(" Error code: {0}, error type: {1}", exception.ErrorCode, exception.ErrorType), ConsoleColor.Red);
            }
        }

        public void DeleteTable(string tableName)
        {
            if (dynamoService == null || string.IsNullOrEmpty(tableName))
            {
                return;
            }

            try
            {
                dynamoService.RestartDynamoDBContext();

                using (IAmazonDynamoDB client = dynamoService.DynamoClient)
                {
                    DeleteTableRequest deleteTableRequest = new DeleteTableRequest(tableName.ToLower());
                    DeleteTableResponse deleteTableResponse = client.DeleteTable(deleteTableRequest);
                    TableDescription tableDescription = deleteTableResponse.TableDescription;
                    TableStatus tableStatus = tableDescription.TableStatus;

                    ConsoleHelper.Write("\nDelete table ");
                    ConsoleHelper.Write(tableName.ToUpper(), ConsoleColor.Yellow);
                    ConsoleHelper.Write(" command sent to Amazon, status after deletion: ");
                    ConsoleHelper.Write(tableDescription.TableStatus, ConsoleColor.Green);
                }
            }
            catch (AmazonDynamoDBException exception)
            {
                ConsoleHelper.Write(string.Format("Exception while deleting new dynamoDB table: {0}", exception.Message), ConsoleColor.Red);
                ConsoleHelper.WriteLine(string.Concat("Error code: {0}, error type: {1}", exception.ErrorCode, exception.ErrorType), ConsoleColor.Red);
            }
        }

        private ScalarAttributeType TypeToScalarAttributeType(Type type)
        {
            switch(type.Name.ToLower())
            {
                case "string":
                    return ScalarAttributeType.S;

                case "short":                                    
                case "int":
                case "int16":
                case "int32":
                case "int64":
                case "float":
                case "double":
                case "long":
                    return ScalarAttributeType.N;

                case "bool":
                case "boolean":
                    return ScalarAttributeType.B;

                default:
                    break;
            }

            return ScalarAttributeType.S;
        }
    }
}