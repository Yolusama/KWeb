using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KWeb.HttpOption
{
    public abstract class RestController
    {
        public HttpRequest Request { get; set; }
        public HttpResponse Response { get; set; }
        public RestController() { }
        public ActionResult OK()
        {
            HttpStatusCode code = HttpStatusCode.OK;
            return new ActionResult($"{(int)code} {code}", code);
        }
        public ActionResult OK(string message)
        {
            return new ActionResult(message, HttpStatusCode.OK);
        }

        public ActionResult<T> OK<T>(T data)
        {
            HttpStatusCode code = HttpStatusCode.OK;
            return new ActionResult<T>($"{(int)code} {code}", code, data);
        }

        public ActionResult<T> OK<T>(string message,T data) 
        {
           return new ActionResult<T>(message, HttpStatusCode.OK, data);
        }
        public ActionResult Fail(HttpStatusCode statusCode)
        {
            return new ActionResult($"{(int)statusCode} {statusCode}", statusCode);
        }
        public ActionResult Fail(string message,HttpStatusCode statusCode)
        {
           return new ActionResult(message,statusCode);
        }
        public ActionResult<T> Fail<T>(HttpStatusCode statusCode)
        {
            return new ActionResult<T>($"{(int)statusCode} {statusCode}", statusCode,default);
        }
        public ActionResult<T> Fail<T>(string message, HttpStatusCode statusCode)
        {
            return new ActionResult<T>(message, statusCode,default);
        }
        public byte[] FileResult(string fileName, byte[] data)
        {
            Response.Headers["Content-Type"] = "octet-stream";
            Response.Headers["Content-Disposition"] = "attachment; filename=\"" + fileName + "\"";
            return data;
        }
    }
}
