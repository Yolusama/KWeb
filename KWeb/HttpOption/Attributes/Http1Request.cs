using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWeb.HttpOption.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Http1Request : Attribute
    {
        public Http1Request() { }
        public Http1Request(HttpMethod method) {  Method = method; }
        public HttpMethod Method {  get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HttpGet : Http1Request
    {
       public HttpGet() { Method = HttpMethod.Get; }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPost : Http1Request
    {
        public HttpPost() { Method = HttpMethod.Post; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPut : Http1Request
    {
        public HttpPut() { Method = HttpMethod.Put; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPatch : Http1Request
    {
        public HttpPatch() { Method = HttpMethod.Patch; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HttpDelete : Http1Request
    {
        public HttpDelete() { Method = HttpMethod.Delete; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HttpHead : Http1Request
    {
        public HttpHead() { Method = HttpMethod.Head; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class QueryParam : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class RouteParam : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class RequestBody : Attribute
    {

    }

    public class PathVaribleNotClosedException : Exception
    {
        public PathVaribleNotClosedException() : base("定义路径参数时未闭合")
        {
        }
    }

    public class MultiMethodSignedException : Exception
    {
        public MultiMethodSignedException() : base("请求方法重定义") { }
    }

    public class ParamsNotMatchedException : Exception
    {
        public ParamsNotMatchedException() : base("参数不匹配"){}
    }

    public class MultiRequestBodyException : Exception
    {
        public MultiRequestBodyException() : base("请求体只能定义一个！") { }
    }

}
