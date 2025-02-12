using KJSON;
using KLogger;
using System.Net.Sockets;

namespace KWeb
{
    public class WebServer
    {
        private static WebApplication? instance = null;
        public WebApplication CreateApplication()
        {
            if(instance!=null)
            {
                throw new Exception("只允许创造一个WebApplication对象");
            }
            WebApplication app = new WebApplication
            {
                Configuration = new KJson("application.json")
            };

            if (app.Configuration["Server"] == null)
            {
                app.Port = 8080;
                app.Name = "app";
            }
            else
            {
                string portKey = "Server:Port";
                string nameKey = "Server:Name";
                app.Port = app.Configuration.Get(portKey) == null ? 8080 : app.Configuration.Get<int>(portKey);
                app.Name = app.Configuration[nameKey] == null ? "app" : app.Configuration.Get<string>(nameKey);
            }
            
            app.Services.AddSingle<IKLogger, Logger>(() =>
            {
                string key = "Logger:Path";
                if (app.Configuration[key] == null)
                    return new Logger();
                else
                    return new Logger(app.Configuration.Get<string>(key));
            });

            instance = app;
            return app;
        }
    }
}
