using System.Net;

namespace KWeb.HttpOption
{
    public class ActionResult
    {
        public string message { get; }
        public HttpStatusCode code { get;  }

        protected ActionResult() { }
        public ActionResult(string message, HttpStatusCode code)
        {
            this.message = message;
            this.code = code;
        }
    }

    public class ActionResult<T> : ActionResult
    {
        public T data { get;  }
        public ActionResult(string message,HttpStatusCode code,T data) : base(message,code)
        {
           this.data = data;
        }
    }
}
