using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpAsyncServer
{
    public class HttpRequest
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }
        public HttpRoute Route { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Dictionary<string, string> Parameters { get; set; }

        public HttpRequest()
        {
            Headers = new Dictionary<string, string>();
            Parameters = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Content))
            {
                if (!Headers.ContainsKey(Consts.CONTENT_LENGTH_TEXT))
                {
                    Headers.Add(Consts.CONTENT_LENGTH_TEXT, Content.Length.ToString());
                }
            }

            return string.Format("{0} {1} HTTP/1.1\r\n{2}\r\n\r\n{3}", Method, Url, string.Join("\r\n", Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))), Content);
        }
    }
}