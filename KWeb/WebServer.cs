using KJSON;
using KLogger;
using System.Net.Sockets;

namespace KWeb
{
    public class WebServer
    {
      
        public WebApplication CreateApplication()
        {
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
     
            return app;
        }
    }
}
