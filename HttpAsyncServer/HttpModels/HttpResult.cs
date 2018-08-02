using System;
using Newtonsoft.Json;

namespace HttpAsyncServer
{
    [Serializable]
    public class HttpResult
    {
        public string code;
        public string reason;
        public object data;
    }
}
