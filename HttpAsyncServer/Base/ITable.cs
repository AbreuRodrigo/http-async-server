using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;

namespace HttpAsyncServer
{
    public interface ITable
    {
        string Id { get; set; }
        string Type { get; set; }
        string Data { get; set; }
    }
}
