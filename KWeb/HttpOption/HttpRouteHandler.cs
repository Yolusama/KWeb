using DependencyInjection;
using KWeb.HttpOption.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace KWeb.HttpOption
{
    public static class HttpRouteHandler
    {
        private static readonly HashSet<HttpPath> paths = new HashSet<HttpPath>();
        public static void RegisterPaths()
        {
            var allTypes = Assembly.GetEntryAssembly().GetTypes();
            var types = allTypes.Where(t => t.IsSubclassOf(typeof(RestController)) && !t.IsAbstract).ToList();
            foreach (var type in types)
            {
                string? urlHead = type.GetCustomAttribute<Route>()?.Value;
                IEnumerable<FieldInfo> injectionFileds = type.GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)
                    .Where(f=>f.GetCustomAttribute<ServiceInjection>()!=null);
                ConstructorInfo? contruct = type.GetConstructors().SingleOrDefault(e=>e.GetCustomAttribute<
                    ServiceInjection>()!=null);
                object[]? values = null;
                if(contruct!=null)
                {
                    ParameterInfo[] constructParams = contruct.GetParameters();
                    values = new object[constructParams.Length];
                    for (int i = 0; i < constructParams.Length; i++)
                    {
                        ServiceInjection? attribute = constructParams[i].GetCustomAttribute<ServiceInjection>();
                        if (attribute != null)
                            values[i] = ServiceProvider.Get(constructParams[i].ParameterType, attribute.Name);
                        else
                            values[i] = ServiceProvider.Get(constructParams[i].ParameterType);
                    }
                }
                MethodInfo[] methods = type.GetMethods();
                foreach(MethodInfo method in methods)
                {
                    IEnumerable<Attribute> attributes = method.GetCustomAttributes();
                    if (attributes.Count(a => a.GetType().IsSubclassOf(typeof(Http1Request))) >= 2)
                        throw new MultiMethodSignedException();
                    bool isReqMethod = false;
                    foreach(Attribute attribute in attributes)
                    {
                        Type attributeType = attribute.GetType();
                        if(attributeType.IsSubclassOf(typeof(Http1Request))
                            ||attributeType.Equals(typeof(Http1Request)))
                        {
                            isReqMethod = true;
                            break;
                        }
                    }
                    if (!isReqMethod)
                        break;
                    StringBuilder builder = new StringBuilder();
                    HttpPath path = new HttpPath();
                    if(urlHead != null) 
                        builder.Append(urlHead);
                    Route? route = method.GetCustomAttribute<Route>();
                    if (route == null)
                        builder.Append($"/{method.Name}");
                    else
                    {
                        if (route.Value.Contains('{'))
                        {
                            int closeIndex = route.Value.IndexOf('}');
                            if (closeIndex < 0)
                                throw new PathVaribleNotClosedException();
                            int openIndex = route.Value.IndexOf('{');
                            string pathParamStr = route.Value.Substring(openIndex);
                            builder.Append(route.Value.Substring(0, openIndex - 1));
                            string[] routeStrs = pathParamStr.Split('/');
                            foreach(string param in routeStrs)
                            {
                                path.PathVarible.Add(param.Remove(param.Length-1).Remove(0,1));
                            }
                        }
                        else builder.Append(route.Value);
                    } 
                    path.Route = builder.ToString();
                    if(paths.SingleOrDefault(p=>p.Route==path.Route)!=null)
                    {
                        throw new Exception("该路径已被注册！");
                    }
                    ParameterInfo[] parameters = method.GetParameters();
                    path.Query = parameters.Where(p => p.GetCustomAttribute<QueryParam>() != null).
                        Select(p => p.Name).ToList();
                    IEnumerable<ParameterInfo> bodyParams = parameters.Where(p => p.GetCustomAttribute<RequestBody>() != null);
                    if (bodyParams.Count() > 1)
                        throw new MultiRequestBodyException();
                    else if(bodyParams.Count()!=0&&bodyParams.Count()==1)
                        path.RequestBodyType = bodyParams.First().ParameterType;
                    path.ControllerType = type;
                    path.ControlMethod = method;
                    paths.Add(path);   
                }
                ServiceProvider.GlobalServices.AddService(type,()=>contruct==null?
                Activator.CreateInstance(type):contruct.Invoke(values));
            }
        }

        public static HttpStatusCode PathExists(HttpPath aimPath)
        {
            HttpPath? path = paths.SingleOrDefault(p => p.Route == aimPath.Route);
            string url = aimPath.Route;
            if(path == null)
                return HttpStatusCode.NotFound;
            aimPath.ControllerType = path.ControllerType;
            aimPath.ControlMethod = path.ControlMethod;

            if(path.PathVarible.Count>0)
            {
                if (aimPath.PathVarible.Count != path.PathVarible.Count)
                    return HttpStatusCode.NotFound;
                for (int i = 0;i<aimPath.PathVarible.Count;i++)
                {
                    if (!path.PathVarible.Contains(aimPath.PathVarible[i]))
                        return HttpStatusCode.BadRequest;
                }
            }
            if(path.Query.Count>0)
            {
                if (aimPath.Query.Count != path.Query.Count)
                    return HttpStatusCode.BadRequest;
                foreach (string param in aimPath.Query)
                {
                    if (!path.Query.Contains(param))
                        return HttpStatusCode.BadRequest;
                }
            }
            if(path.RequestBodyType!=null)
                aimPath.RequestBodyType = path.RequestBodyType;
            return HttpStatusCode.OK;
        }

        public static void ModifyPath(HttpPath httpPath,HttpRequest request)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            foreach(HttpPath path in paths)
            {
               
                Regex regex = new Regex($"^{path.Route}(/.*)?$");
                bool res = regex.IsMatch(httpPath.Route);
                if(regex.IsMatch(httpPath.Route))
                {
                    if (path.PathVarible.Count == 0)
                    {
                        return;
                    } 
                    if (httpPath.Route.Count(c=>c=='/') - path.Route.Count(c => c == '/') > path.PathVarible.Count)
                        throw new ParamsNotMatchedException();
                    string[] strs = httpPath.Route.Split('/');
                  
                    for(int i = path.PathVarible.Count - 1,j = 0;i>=0;i--,j++)
                    {
                        values[path.PathVarible[j]]=strs[strs.Length - i - 1];
                    }
                    httpPath.Route = path.Route;
                    httpPath.PathVarible = path.PathVarible;
                    break;
                }
            }
            foreach(string key in values.Keys)
            {
                request.PathVarible[key] = HttpUtility.UrlDecode(values[key].ToString());
            }
        }

        public static bool VerifyRequest(HttpRequest request,HttpResponse response,HttpPath path)
        {
            if (request.Method == HttpMethod.Options)
                return false;
            if(request.Method != path.ControlMethod.GetCustomAttribute<Http1Request>().Method)
            {
                response.StatusCode = HttpStatusCode.NotAcceptable;
                return false;
            }
            else
            {
                response.StatusCode = HttpStatusCode.OK;
                return true;
            }
        }
    }
}
