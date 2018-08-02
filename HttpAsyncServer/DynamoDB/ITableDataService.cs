using System.Collections.Generic;

namespace HttpAsyncServer.DynamoDB
{
    public interface ITableDataService
    {        
        void SaveItem<T>(T item) where T : ITable;
        void BatchStore<T>(IEnumerable<T> items) where T : class;
        IEnumerable<T> GetAll<T>(string propertyName, string value) where T : class;
        T GetItem<T>(string hashKey) where T : class;
    }
}