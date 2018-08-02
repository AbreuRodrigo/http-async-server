using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace HttpAsyncServer
{
    public class HttpProcessor
    {
        private List<HttpRoute> Routes = new List<HttpRoute>();
        //private static readonly ILog log = LogManager.GetLogger(typeof(HttpProcessor));

        public void HandleClient(Stream clientStream)
        {
            Stream inputStream = clientStream;
            NetworkStream outputStream = (NetworkStream) clientStream;
            HttpRequest request = GetHttpRequest(inputStream);
            HttpResponse response = RouteHttpRequest(request);

            request.Url = string.IsNullOrEmpty(request.Url) ? "/" : request.Url;
            Console.WriteLine("Response: {0} Url: {1}", response.HttpResponseStatus.Code, request.Url);

            if (response.Content == null)
            {
                if (HttpStatus.Ok.Code.Equals(response.HttpResponseStatus.Code))
                {
                    response.ContentAsUTF8 = string.Format("{0} {1} <p> {2}", response.HttpResponseStatus.Code, request.Url, response.HttpResponseStatus.Reason);
                }
            }

            WriteResponse(outputStream, response);

            outputStream.Flush();
            outputStream.Close();
            outputStream = null;

            inputStream.Close();
            inputStream = null;
        }

        private static void WriteResponse(Stream stream, HttpResponse response)
        {
            try
            {
                if (response.Content == null)
                {
                    response.Content = new byte[] { };
                }

                if (!response.Headers.ContainsKey(Consts.CONTENT_TYPE_TEXT))
                {
                    response.Headers[Consts.CONTENT_TYPE_TEXT] = Consts.APPLICATION_JSON_TYPE;
                }

                response.Headers[Consts.CONTENT_LENGTH_TEXT] = response.Content.Length.ToString();

                Write(stream, string.Format("HTTP/1.1 {0} {1}", response.HttpResponseStatus.Code, response.HttpResponseStatus.Reason));
                Write(stream, string.Join("\r\n", response.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
                Write(stream, "\r\n\r\n");

                stream.Write(response.Content, 0, response.Content.Length);
            }
            catch
            {
                Console.WriteLine("Could not write to stream, the client is no longer available.");
            }
        }

        public void AddRoute(HttpRoute route)
        {
            this.Routes.Add(route);
        }

        protected virtual HttpResponse RouteHttpRequest(HttpRequest request)
        {
            HandleParamsBasedRequest(request);

            if (string.Empty.Equals(request.Url) || "/".Equals(request.Url) || request == null || string.IsNullOrEmpty(request.Url))
            {
                return HttpBuilder.Ok();
            }

            List<HttpRoute> routes = this.Routes.Where(x => Regex.Match(request.Url, x.UrlRegex).Success).ToList();

            if (!routes.Any())
            {
                return HttpBuilder.NotFound();
            }

            HttpRoute route = routes.SingleOrDefault(x => x.Method == request.Method);

            if (route == null)
            {
                return new HttpResponse()
                {
                    HttpResponseStatus = HttpStatus.MethodNotAllowed
                };
            }

            var match = Regex.Match(request.Url, route.UrlRegex);
            if (match.Groups.Count > 1)
            {
                request.Path = match.Groups[1].Value;
            }
            else
            {
                request.Path = request.Url;
            }

            request.Route = route;

            try
            {
                return route.Callable(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error -> " + ex.Message);
                return HttpBuilder.InternalServerError();
            }
        }

        private void HandleParamsBasedRequest(HttpRequest request)
        {
            if (request != null && !string.IsNullOrEmpty(request.Url) &&
                (EHttpMethod.GET.ToString().Equals(request.Method)) ||
                 EHttpMethod.DELETE.ToString().Equals(request.Method))
            {
                string[] uriParts = request.Url.Split('?');

                if (uriParts.Length > 1)
                {
                    request.Url = uriParts[0];
                    string[] parameters = uriParts[1].Split('&');
                    string[] keyVal = null;

                    foreach (string p in parameters)
                    {
                        keyVal = p.Split('=');

                        if (keyVal.Length == 2)
                        {
                            request.Parameters.Add(keyVal[0], keyVal[1]);
                        }
                    }
                }
            }
        }

        private HttpRequest GetHttpRequest(Stream inputStream)
        {
            string method = string.Empty;
            string url = string.Empty;
            string protocolVersion = string.Empty;
            string content = string.Empty;
            string request = ReadLineFromStream(inputStream);

            Dictionary<string, string> headers = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(request))
            {
                string[] tokens = request.Split(' ');

                if (tokens.Length != 3)
                {
                    Console.WriteLine("HttpProcessor->GetHttpRequest: Invalid http request line\n");

                    foreach (string token in tokens)
                    {
                        Console.Write(token + " ");
                    }
                }
                else
                {
                    method = tokens[0].ToUpper();
                    url = tokens[1];
                    protocolVersion = tokens[2];

                    headers = ReadHeaders(inputStream);
                    content = ReadContent(inputStream, headers);
                }
            }

            return new HttpRequest()
            {
                Method = method,
                Url = url,
                Headers = headers,
                Content = content
            };
        }

        private static string ReadLineFromStream(Stream stream)
        {
            int nextChar;
            string data = string.Empty;

            try
            {
                while (true)
                {
                    nextChar = stream.ReadByte();

                    if (nextChar == '\n') break;
                    if (nextChar == '\r') continue;
                    if (nextChar == -1) break;

                    data += Convert.ToChar(nextChar);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("HttpProcessor->ReadLine: " + e.Message);
            }

            return data;
        }

        private static void Write(Stream stream, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        private Dictionary<string, string> ReadHeaders(Stream inputStream)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();

            string line;
            while ((line = ReadLineFromStream(inputStream)) != null)
            {
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator != -1)
                {

                    string name = line.Substring(0, separator);
                    int pos = separator + 1;
                    while ((pos < line.Length) && (line[pos] == ' '))
                    {
                        pos++;
                    }

                    string value = line.Substring(pos, line.Length - pos);

                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                    {
                        headers.Add(name, value);
                    }
                }
            }

            return headers;
        }

        private string ReadContent(Stream inputStream, Dictionary<string, string> headers)
        {
            string bytesStr = null;
            string content = null;

            if (headers.TryGetValue(Consts.CONTENT_LENGTH_TEXT, out bytesStr))
            {
                int totalBytes = Convert.ToInt32(bytesStr);
                int bytesLeft = totalBytes;
                byte[] bytes = new byte[totalBytes];

                while (bytesLeft > 0)
                {
                    byte[] buffer = new byte[bytesLeft > 1024 ? 1024 : bytesLeft];
                    int n = inputStream.Read(buffer, 0, buffer.Length);
                    buffer.CopyTo(bytes, totalBytes - bytesLeft);

                    bytesLeft -= n;
                }

                content = Encoding.UTF8.GetString(bytes);
                content = Uri.UnescapeDataString(content);
            }

            return content;
        }
    }
}