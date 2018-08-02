using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HttpAsyncServer
{
    public class HttpResponse
    {
        private const string DATA_STR = "data";
        public HttpResponseStatus HttpResponseStatus { get; set; }
        public byte[] Content { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string ContentAsUTF8 {
            set
            {
                Content = Encoding.UTF8.GetBytes(value);
            }
        }

        public HttpResponse()
        {
            Headers = new Dictionary<string, string>();
        }

        public HttpResponse(object content, HttpResponseStatus responseStatus)
        {
            Headers = new Dictionary<string, string>();

            HttpResponseStatus = responseStatus;

            HttpResult result = new HttpResult();
            result.code = responseStatus.Code;
            result.reason = responseStatus.Reason;
            result.data = content;
            
            ContentAsUTF8 = JsonConvert.SerializeObject(result);
        }

        public override string ToString()
        {
            return string.Format("HTTP status {0} {1}", HttpResponseStatus.Code, HttpResponseStatus.Reason);
        }
    }
}