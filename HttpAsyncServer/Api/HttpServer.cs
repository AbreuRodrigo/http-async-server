using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Newtonsoft.Json;
using HttpAsyncServer.DynamoDB;
using System.Threading;

namespace HttpAsyncServer
{
    public class HttpServer
    {
        private static int Port;
        private static HttpProcessor Processor = null;
        private static DynamoService DynamoService = null;

        //private CancellationToken ct;
        //private static CancellationTokenSource cts = new CancellationTokenSource();
        //private static readonly ILog log = LogManager.GetLogger(typeof(HttpServer));

        //private bool disposed = false;
        private static TcpListener listener = null;

        public static void Run(int port, string dbEndPoint, string awsAccessKey, string awsSecretKey)
        {
            Console.WriteLine(string.Format("Starting server on port {0}", port));
            
            //DynamoService = new DynamoService(dbEndPoint, awsAccessKey, awsSecretKey);

            if(DynamoService != null)
            {
                Console.WriteLine(string.Format("\nStarting dynamoDB - {0}", dbEndPoint));
            }

            Port = port;
            Processor = new HttpProcessor();
            BuildRoutesFromServices();
            //CreateDynamoDBTables();

            Console.WriteLine(string.Format("\nServer listening on port {0}\n", port));

            listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();

            WaitForClients();
        }

        private static void WaitForClients()
        {
            if(listener != null)
            {
                listener.BeginAcceptTcpClient(new AsyncCallback(ProcessRequest), null);
            }
        }

        private static void ProcessRequest(IAsyncResult result)
        {
            try
            {
                if (listener != null)
                {
                    TcpClient tcpClient = listener.EndAcceptTcpClient(result);

                    if (tcpClient != null)
                    {
                        Console.WriteLine("\nRequest from: " + tcpClient.Client.RemoteEndPoint.ToString());
                        Processor.HandleClient(tcpClient.GetStream());
                    }
                }

                WaitForClients();
            }
            catch(Exception e)
            {
                Console.WriteLine("\nError: " + e.Message);
            }
        }
        
        private static void BuildRoutesFromServices()
        {
            try
            {
                var type = typeof(IController);
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p));

                foreach (var t in types)
                {
                    if (t != null)
                    {
                        Type serviceType = GetTypeFromAssemblies(t.FullName);

                        if (!serviceType.IsInterface && !serviceType.IsAbstract)
                        {
                            IController service = (IController) Activator.CreateInstance(serviceType);
                            
                            if (service != null)
                            {
                                var r = service.GetType().GetCustomAttribute<HttpResource>();
                                service.DynamoService = DynamoService;

                                foreach (MethodInfo method in service.GetType().GetMethods())
                                {
                                    var paths = method.GetCustomAttributes<HttpPath>();

                                    foreach (HttpPath m in paths)
                                    { 
                                        HttpRoute route = new HttpRoute();
                                        route.Callable = (HttpRequest request) =>
                                        {
                                            object response = null;
                                            List<object> args = GetParamsFromRequest(method, request);

                                            if (args != null)
                                            {
                                                response = method.Invoke(service, args.ToArray());
                                            }

                                            return (HttpResponse)response;
                                        };

                                        route.Name = method.Name;
                                        route.UrlRegex = string.Format(Consts.REGEX_PATH, r.ResourceName + m.Path);
                                        route.Method = m.Method.ToString();

                                        Processor.AddRoute(route);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static List<object> GetParamsFromRequest(MethodInfo method, HttpRequest request)
        {
            if(request == null)
            {
                return null;
            }

            List<object> args = new List<object>();

            if (EHttpMethod.GET.ToString().Equals(request.Method))
            {
                args.Add(request);
            }
            else if (EHttpMethod.POST.ToString().Equals(request.Method))
            {
                string content = string.Empty;
                string data = string.Empty;
                object arg = null;

                foreach (ParameterInfo p in method.GetParameters())
                {
                    content = request.Content;

                    if (content.Contains("="))
                    {
                        string[] dataArray = content.Split('=');
                        data = dataArray[1];
                    }
                    else
                    {
                        data = content;
                    }

                    arg = JsonConvert.DeserializeObject(data, p.ParameterType);
                    args.Add(arg);
                }
            }
            else if(EHttpMethod.PUT.ToString().Equals(request.Method))
            {
                string data = request.Content;
                object arg = JsonConvert.DeserializeObject(data);

                //Get only the first param
                ParameterInfo[] pmts = method.GetParameters();
                ParameterInfo parameter = null;

                if (pmts != null && pmts.Length > 0)
                {
                    parameter = pmts[0];
                    arg = JsonConvert.DeserializeObject(data, parameter.ParameterType);
                    args.Add(arg);
                }
            }
            else if(EHttpMethod.DELETE.ToString().Equals(request.Method))
            {
                string content = string.Empty;
                string data = string.Empty;

                if (request.Parameters != null && request.Parameters.Count > 0)
                {
                    foreach (ParameterInfo p in method.GetParameters())
                    {
                        args.Add(request.Parameters[p.Name]);
                    }
                }
            }

            return args;
        }

        private static void CreateDynamoDBTables()
        {
            try
            {
                Console.WriteLine("\nStarting dynamoDB table validation...");

                DynamoTableManager dynamoDBTableCreator = new DynamoTableManager(DynamoService);

                var type = typeof(ITable);
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p));

                foreach (var t in types)
                {
                    if (t != null)
                    {
                        Type tableType = GetTypeFromAssemblies(t.FullName);

                        if (!tableType.IsInterface && !tableType.IsAbstract)
                        {
                            dynamoDBTableCreator.CreateNewTable(tableType, typeof(ITable));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static Type GetTypeFromAssemblies(string typeName)
        {
            var type = Type.GetType(typeName);

            if (type != null)
            {
                return type;
            }

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);

                if (type != null)
                {
                    return type;
                }
            }

            return type;
        }

        /*public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                Console.WriteLine("Server is shutting down...");
                Console.ReadLine();
            }
        }*/
    }
}