using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpAsyncServer
{
    public class HttpResponseStatus
    {
        public string Code { get; set; }
        public string Reason { get; set; }

        public HttpResponseStatus(string code, string reason)
        {
            Code = code;
            Reason = reason;
        }
    }
}