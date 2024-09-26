using DependencyInjection;
using KJSON;
using KWeb.HttpOption.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KWeb.HttpOption
{
    public class HttpPath
    {
        public string Route { get; set; }
        public List<string> Query { get; set; }
        public List<string> PathVarible { get; set; }

        public Type ControllerType { get; set; }
        public MethodInfo ControlMethod { get; set; }
        public Type? RequestBodyType { get; set; }
        public HttpPath() {
            Query = new List<string>();
            PathVarible = new List<string>();
            RequestBodyType = null;
        }
        public HttpPath(string route) : this() 
        {
            Route = route;  
        }

        public object? GetContoller()
        {
            return ServiceProvider.Get(ControllerType);
        }

        public object? InvokeControlMethod(HttpRequest request,HttpResponse response)
        {
            object? controller = GetContoller();
            ParameterInfo[] paramNames = ControlMethod.GetParameters();
            ControllerType.GetProperty("Request").SetValue(controller, request);
            ControllerType.GetProperty("Response").SetValue(controller,response);
            if(Query.Count==0&&PathVarible.Count==0&&RequestBodyType==null)
                return ControlMethod.Invoke(controller,parameters: null);
            List<object> values = new List<object>();
            foreach(ParameterInfo param in paramNames)
            {
                if (param.GetCustomAttribute<RouteParam>() != null)
                    values.Add(TransformValue(param.ParameterType, request.PathVarible[param.Name].ToString()));
                else if (param.GetCustomAttribute<QueryParam>() != null)
                    values.Add(TransformValue(param.ParameterType, request.Query[param.Name].ToString()));
                else if(param.GetCustomAttribute<RequestBody>() != null)
                {
                    if (request.Body == null)
                        values.Add(null);
                    else
                    {
                        JsonParser parser = new JsonParser(request.Body);
                        values.Add(parser.Parse(RequestBodyType));
                    }
                }
            }
            return ControlMethod.Invoke(controller, values.ToArray());
        }

        private object TransformValue(Type paramType,string paramValue)
        {
            if(paramType.Equals(typeof(int)))
                return int.Parse(paramValue);
            if(paramType.Equals(typeof(float)))
                return float.Parse(paramValue);
            if(paramType.Equals (typeof(double)))
                return double.Parse(paramValue);
            if(paramType.Equals(typeof(long)))
                return long.Parse(paramValue);
            return paramValue;
        }

        public override bool Equals(object? obj)
        {
            return Route.Equals(((HttpPath)obj).Route);
        }

        public override int GetHashCode()
        {
            return Route.GetHashCode();
        }
    }
}
