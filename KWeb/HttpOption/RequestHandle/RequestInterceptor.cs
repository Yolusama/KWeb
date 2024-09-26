using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWeb.HttpOption.RequestHandle
{
    public abstract class RequestInterceptor
    {
        public int Order { get; set; } = 1;
        public string[]? ExcludedPatterns { get; set; } = null;
        public virtual bool PreHandle(HttpRequest request,HttpResponse response)
        {
            return true;
        }

        public virtual void AfterHandling(HttpRequest request, HttpResponse response) { }
    }

    public class RequestInterceptorRegister
    {
        public List<RequestInterceptor> Interceptors { get; init; }
        public RequestInterceptorRegister()
        {
            Interceptors = new List<RequestInterceptor>();
        }

        public RequestInterceptorRegister AddInterceptor(RequestInterceptor interceptor)
        {
            Interceptors.Add(interceptor);
            return this;
        }

        public RequestInterceptorRegister Order(int order)
        {
            Interceptors[Interceptors.Count - 1].Order = order;
            return this;
        }
        public RequestInterceptorRegister ExcludePatterns(params string[] patterns)
        {
            Interceptors[Interceptors.Count-1].ExcludedPatterns = patterns;
            return this;
        }
    }

    public class RequestInterceptorRegisterBuilder
    {
        private readonly RequestInterceptorRegister register;
        public RequestInterceptorRegisterBuilder()
        {
            register = new RequestInterceptorRegister();
        }
        public RequestInterceptorRegisterBuilder AddInterceptor(RequestInterceptor interceptor)
        {
            register.Interceptors.Add(interceptor);
            return this;
        }

        public RequestInterceptorRegisterBuilder Order(int order)
        {
            register.Interceptors[register.Interceptors.Count - 1].Order = order;
            return this;
        }
        public RequestInterceptorRegisterBuilder ExcludePatterns(params string[] patterns)
        {
            register.Interceptors[register.Interceptors.Count - 1].ExcludedPatterns = patterns;
            return this;
        }
        public RequestInterceptorRegister Build()
        {
            return register;
        }
    }
}
