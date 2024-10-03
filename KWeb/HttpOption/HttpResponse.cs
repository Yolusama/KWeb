using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace KWeb.HttpOption
{
    public class HttpResponse
    {
        public Dictionary<string, string> Headers { get; init; }
        public HttpStatusCode StatusCode { get; set; }
        public string Version { get; set; }
        public object Result { get; set; }

        public HttpResponse()
        {
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/html;charset=utf-8" },
                { "Cache-Control" , "no-cache, no-store" }
            };
            Version = "HTTP/1.1";
        }

        public string Response()
        {
            StringBuilder builder = new StringBuilder();
            Type resultType = Result.GetType();
            string? result = null;
            if(IsBaseType(resultType))
            {
                result = Result.ToString();
            }
            else
            {
                Headers["Content-Type"] = "application/json";
                result = JsonSerializer.Serialize(Result, resultType,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            builder.Append(ResponseHeaders());
            builder.Append(result);
            return builder.ToString();
        }

        public string ResponseHeaders()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{Version} {(int)StatusCode} {StatusCode} \r\n");
            foreach (string key in Headers.Keys)
                builder.Append($"{key}: {Headers[key]}\r\n");
            builder.Append("\r\n");
            return builder.ToString();
        }

        private bool IsBaseType(Type type)
        {
            return type == typeof(string) ||
                type == typeof(int) || type == typeof(long)
                || type == typeof(float) || type == typeof(double)
                || type == typeof(uint) || type == typeof(ulong);
        }
    }
}
