using KLogger;
using KWeb.HttpOption;
using KWeb.HttpOption.RequestHandle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interceptor
{
    [ServiceInjection]
    public class TestInterceptor : RequestInterceptor
    {
        public TestInterceptor()
        {
            ExcludedPatterns = new string[] { "/Common","/Test/Test1" };
            Order = 2;
        }
        [ConfigInjection]
        private readonly ExampleConfig1 config;
        [ServiceInjection]
        private readonly IKLogger logger;
        public override bool PreHandle(HttpRequest request, HttpResponse response)
        {
            logger.Debug(config.a + ":" + config.b);
  
            return base.PreHandle(request, response);
        }
        public override void AfterHandling(HttpRequest request, HttpResponse response)
        {
            base.AfterHandling(request, response);
        }
    }
}
