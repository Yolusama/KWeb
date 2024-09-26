using KWeb.HttpOption;
using KWeb.HttpOption.RequestHandle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interceptor
{
    public class TestInterceptor : RequestInterceptor
    {
        public override bool PreHandle(HttpRequest request, HttpResponse response)
        {
            return base.PreHandle(request, response);
        }
        public override void AfterHandling(HttpRequest request, HttpResponse response)
        {
            base.AfterHandling(request, response);
        }
    }
}
