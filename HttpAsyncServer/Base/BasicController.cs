using System;
using log4net;
using HttpAsyncServer.DynamoDB;
using Newtonsoft.Json;

namespace HttpAsyncServer
{
    public abstract class BasicController : IController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BasicController));                
        public DynamoService DynamoService { get; set; }

        protected void Log(string message)
        {
            if (log != null)
            {
                log.Debug(message);
            }
        }

        protected void LogError(Exception exception)
        {
            if (exception != null)
            {
                LogError(exception.Message, exception);
            }
        }

        protected void LogError(string message, Exception exception)
        {
            if(log != null)
            {
                if (exception == null)
                {
                    log.Error(message);
                }
                else
                {
                    log.Error(message, exception);
                }
            }
        }

        protected void LogInfo(string message)
        {
            LogInfo(message, null);
        }

        protected void LogInfo(string message, Exception exception)
        {
            if (log != null)
            {
                if (exception == null)
                {
                    log.Info(message);
                }
                else
                {
                    log.Info(message, exception);
                }
            }
        }

        protected string ObjectToJson<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}