using HttpAsyncServer.DynamoDB;

namespace HttpAsyncServer
{
    public interface IController
    {
        DynamoService DynamoService { get; set; }
    }
}