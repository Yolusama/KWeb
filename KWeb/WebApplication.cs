using DependencyInjection;
using KJSON;
using KLogger;
using KWeb.HttpOption;
using KWeb.HttpOption.RequestHandle;
using System.Data.Common;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace KWeb
{
    public class WebApplication : IDisposable
    {
        public Socket Server { get; set; }
        public string Name {  get; set; }
        public int Port { get; set; }
        private static WebApplication? instance = null;
        public KJson Configuration { get; init; }
        const int QueueSize = 1024;
        const int MB = 1024 * 1024;
        private CorsVerifier? usedCorsVerifer = null;
        private bool isRunning = false;
        public ServiceProvider Services { get; } = new ServiceProvider();
        private CorsVerifierRegister corsVerifierRegister;
        private RequestInterceptorRegister? interceptorManager = null;
        private RequestInterceptor? workedInterceptor = null;
        private ResourceHandler? resourceHandler = null;
        private IKLogger logger;
        public WebApplication()
        {
            if (instance == null)
                instance = this;
            else
                throw new Exception("只允许创造一个WebApplication对象");
        }
        public void AddCors(Action<CorsVerifierRegisterBuilder> registerFunc)
        {
            CorsVerifierRegisterBuilder builder = new CorsVerifierRegisterBuilder();
            registerFunc(builder);
            corsVerifierRegister = builder.Build();
        }

        public void AddInterceptors(Action<RequestInterceptorRegisterBuilder> registerFunc)
        {
            RequestInterceptorRegisterBuilder builder = new RequestInterceptorRegisterBuilder();
            registerFunc(builder);
            interceptorManager = builder.Build();
        }

        public void AddResourceHandler(Action<ResourceHandlerBuilder> builder)
        {
            ResourceHandlerBuilder handlerBuilder = new ResourceHandlerBuilder();
            builder(handlerBuilder);
            resourceHandler = handlerBuilder.Build();
        }

        public void UseCors(string name)
        {
            if (!corsVerifierRegister.CorsVerifiers.ContainsKey(name)) return;
            usedCorsVerifer = corsVerifierRegister.CorsVerifiers[name];
        }

        public void Run()
        {
            if(isRunning) return;
            isRunning = true;
            logger = (IKLogger)ServiceProvider.Get(typeof(IKLogger));
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Server.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port));
            logger.Fatal($"应用名称：{Name}，应用将启动...");
            logger.Info($"应用已启动，访问地址：http://{Server.LocalEndPoint}或者http://localhost:{Port}");
            Server.Listen(QueueSize);
            ThreadPool.SetMaxThreads(12, 12);
            Services.Register();
            HttpRouteHandler.RegisterPaths();
            int bufferSize = Configuration.Get("Request:MaxSize") == null ?
                                  100 * MB + 1: Configuration.Get<int>("Request:MaxSize");
            while (isRunning)
            {
                try
                { 
                    Socket client = Server.Accept();
                    if (client==null||!client.Connected||client.Available<0)
                    {
                        continue;
                    }
                    ThreadPool.QueueUserWorkItem(arg =>
                    {
                        try
                        {
                            byte[] buffer = new byte[bufferSize];
                            int recvBytes = client.Receive(buffer);
                            string res;
                            if (recvBytes > 0)
                                res = Encoding.UTF8.GetString(buffer, 0, recvBytes);
                            else res = "";
                            //Console.WriteLine(res.Trim());
                            if (res==""||res.Contains("/favicon.ico"))
                            {
                                /* response.StatusCode = HttpStatusCode.BadGateway;
                                 response.Result = "未知";*/
                                return;
                            }
                            HttpRequest request = new HttpRequest(buffer,recvBytes);
                            HttpResponse response = new HttpResponse();
                            HttpPath path = new HttpPath(request.NoQueryUrl());
                            path.Query = request.Query.Keys.ToList();
                            HttpRouteHandler.ModifyPath(path, request);
                            HttpStatusCode statusCode = HttpRouteHandler.PathExists(path);
                            response.StatusCode = statusCode;
                            if (usedCorsVerifer != null && request.Headers.ContainsKey("Origin"))
                                usedCorsVerifer.Verify(request,response);
                            if (statusCode == HttpStatusCode.OK &&
                            HttpRouteHandler.VerifyRequest(request, response, path)
                            &&(resourceHandler==null||!resourceHandler.RouteMatched(path.Route)))
                            {
                                if (InterceptorHandle(path, request, response))
                                    response.Result = path.InvokeControlMethod(request, response);
                                else
                                {
                                    response.StatusCode = HttpStatusCode.Unauthorized;
                                    response.Result = $"{(int)response.StatusCode} {response.StatusCode}";
                                }
                            }
                           else if(resourceHandler!=null && resourceHandler.RouteMatched(path.Route))
                            {
                                byte[] fileBytes = resourceHandler.Locate(path.Route, response);
                                if (fileBytes!=null) 
                                    response.Result= fileBytes;
                                else
                                {
                                    response.StatusCode = HttpStatusCode.NotFound;
                                    response.Result = $"{(int)response.StatusCode} {response.StatusCode}";
                                }
                            }
                            else
                            {
                                response.Result = $"{(int)response.StatusCode} {response.StatusCode}";
                            }
                            workedInterceptor?.AfterHandling(request, response);
                            if (response.Result.GetType() == typeof(byte[]))
                            {
                                using NetworkStream stream = new NetworkStream(client);
                                stream.Write(Encoding.UTF8.GetBytes(response.ResponseHeaders()));
                                stream.Write((byte[])response.Result);
                            }
                            else 
                                client.Send(Encoding.UTF8.GetBytes(response.Response()));
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();                     
                        }
                        catch(Exception ex) 
                        {
                            logger.Error(ex.Message + ":" + ex.StackTrace);
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                        }
                    });
                }
                catch 
                {
                    continue;
                }
                Thread.Sleep(10);
            }
        }
        private bool InterceptorHandle(HttpPath path,HttpRequest request,HttpResponse response)
        {
           if (interceptorManager == null)
                return true;
           foreach(var interceptor in interceptorManager.Interceptors.OrderByDescending(i=>i.Order))
               {
                    bool matchRes = false;
                    foreach(string pattern in interceptor.ExcludedPatterns)
                    {
                        Regex regex = new Regex(pattern);
                        if (regex.IsMatch(path.Route))
                        {
                            matchRes = true;
                            break;
                        }
                        else
                        {
                            matchRes = false;
                            break;
                        }
                    }
                   if (!matchRes)
                   {
                      workedInterceptor = interceptor;
                      return interceptor.PreHandle(request, response);
                   }
                }
            return true;
        }

        public void Dispose()
        {
            Server.Dispose();
            Configuration.Dispose();
        }
    }
}
