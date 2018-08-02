using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpAsyncServer
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class HttpPath : Attribute
    {
        public string Path { get; private set; }
        public EHttpMethod Method { get; private set; }

        public HttpPath(string path, EHttpMethod method)
        {
            Path = path;
            Method = method;
        }
    }
}