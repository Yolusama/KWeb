// See https://aka.ms/new-console-template for more information

using Entity;
using Koober;

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
    builder.RegisterFromService();
    /*builder.AddInterceptor(new TestInterceptor()).Order(2)
                                .ExcludePatterns("/Test/Test1");*/
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

/*app.CreateTables(configurator =>
{
    DbTableBuilder<C> cBuilder = configurator.TableBuilder<C>();
    cBuilder.Column().WithProperty(c => new { c.Id }).IsPrimaryKey().IsAutoIncresing();
    cBuilder.Column().WithProperty(c => new { c.Name }).Required().DbType("varchar(10)");
    cBuilder.Column().WithProperty(c => new { c.Content }).DbType("varchar(25)");
    cBuilder.Column().WithProperty(c => new { c.AId }).HasComment("a表主键");
    cBuilder.Column().WithProperty(c => new { c.Zone }).DbType("varchar(25)");
    cBuilder.Index().WithProperty(c => new { c.AId });
    cBuilder.Build();
});*/

app.CreateTables(typeof(D));

app.Services.AddConfiguration<ExampleConfig1>(() => app.Configuration.Get<ExampleConfig1>("Example:exam1"));
app.Services.AddConfiguration<ExampleConfig2>(() => app.Configuration.Get<ExampleConfig2>("Example:exam2"));

app.Services.AddService(()=>new AService());

app.UseCors("default");

app.Run();

