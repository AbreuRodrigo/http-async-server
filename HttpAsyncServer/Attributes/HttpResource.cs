using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpAsyncServer
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HttpResource : Attribute
    {
        public string ResourceName { get; private set; }

        public HttpResource(string resourceName)
        {
            ResourceName = resourceName;
        }
    }
}
