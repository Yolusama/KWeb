using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWeb.HttpOption
{
    public abstract class RestController
    {
        public HttpRequest Request { get; set; }
        public HttpResponse Response { get; set; }
        public RestController() { }

    }
}
