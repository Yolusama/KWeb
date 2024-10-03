// See https://aka.ms/new-console-template for more information

using KWeb.HttpOption;

WebServer server = new WebServer();
var app = server.CreateApplication();

app.AddCors(builder =>
{
    builder.AddCorVerifier("default").
    AllowAnyHeaders().AllowAnyOrigins().AllowAnyMethods();
    builder.AddCorVerifier("OnlyGetPost").AllowAnyOrigins().
    AllowHeaders().AllowMethods("get", "post");
});

app.AddInterceptors(builder =>
{
    builder.AddInterceptor(new TestInterceptor()).Order(2)
                                .ExcludePatterns("/Test/Test1");
});

app.AddMySqlConnection(builder =>
{
     builder.Server = app.Configuration.Get<string>("MySql:Server");
     builder.Port = app.Configuration.Get<uint>("MySql:Port");
     builder.Database = app.Configuration.Get<string>("MySql:Database");
     builder.UserID = app.Configuration.Get<string>("MySql:UserId");
     builder.Password = app.Configuration.Get<string>("MySql:Password");
     builder.MaximumPoolSize = app.Configuration.Get<uint>("MySql:MaxConnection");
});

app.AddRedisConnetion(builder =>
{
    builder.SetHost("localhost").SetPort(6379);
});

app.AddFormCachingPath(() => app.Configuration.Get<string>("Request:Form:CachingPath"));


app.AddResourceHandler(builder =>
{
    builder.AddPatterns(app.Configuration.Get<string[]>("Resource:Patterns"))
                     .AddLocations(app.Configuration.Get<string[]>("Resource:Locations"));
});


app.Services.AddService(()=>new AService());

app.UseCors("default");

app.Run();

