using System;
using System.Collections.Generic;
using HttpAsyncServer.DynamoDB;

namespace HttpAsyncServer
{
    public class HttpServerRunner
    {
        public static void RunAsyncService(int port, string dbEndPoint, string awsAccessKey, string awsSecretKey)
        {
            try
            {
                HttpServer.Run(port, dbEndPoint, awsAccessKey, awsSecretKey);
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Concat("HttpServerRunner->HttpServerRunner->RunAsyncService ", e.Message));
                Console.ReadLine();
            }
        }
    }
}