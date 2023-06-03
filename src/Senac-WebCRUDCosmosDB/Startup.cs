using Microsoft.Extensions.Configuration;
using System.Net.WebSockets;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Remote;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure.IO.GraphSON;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Senac_WebCRUDCosmosDB.TinkerpopCustom;
using Microsoft.AspNetCore.Routing.Template;

namespace Senac_WebCRUDCosmosDB
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {

            string strHostname = "";
            int intPort = 0;
            bool blnEnableSSL = false;
            string strUserName = "";
            string strPassword = "";

            // Configura a conexão dos Singleton de Conexao e Trasveral do Gremilin conforme Producao ou Desenvolvimento
            // Veja o arquivo de appsettings.json para saber os ambientes

            // Captura os dados do appsettings.{environment}.json
            strHostname = Configuration.GetValue<string>("CosmosDBConnectionString:hostname");
            intPort = Configuration.GetValue<int>("CosmosDBConnectionString:port");
            blnEnableSSL = Configuration.GetValue<bool>("CosmosDBConnectionString:enablessl");
            strUserName = Configuration.GetValue<string>("CosmosDBConnectionString:username");
            strPassword = Configuration.GetValue<string>("CosmosDBConnectionString:password");

            Console.WriteLine($"Connecting to: host: {strHostname}, port: {intPort.ToString()}, container: {strUserName}, ssl: {blnEnableSSL.ToString()}");
            // Configura os serviços
            services.AddSingleton<GremlinClient>(
                (serviceProvider) =>
                {
                    var gremlinServer = new GremlinServer(
                        hostname: strHostname,
                        port: intPort,
                        enableSsl: blnEnableSSL,
                        username: strUserName,
                        password: strPassword
                    );

                    var connectionPoolSettings = new ConnectionPoolSettings
                    {
                        MaxInProcessPerConnection = 10,
                        PoolSize = 30,
                        ReconnectionAttempts = 3,
                        ReconnectionBaseDelay = TimeSpan.FromMilliseconds(500)
                    };

                    var webSocketConfiguration = new Action<ClientWebSocketOptions>(options =>
                    {
                        options.KeepAliveInterval = TimeSpan.FromSeconds(10);
                    });

                    return new GremlinClient(
                        gremlinServer, 
                        new CustomGraphSON2Reader(), 
                        new GraphSON2Writer(), 
                        Gremlin.Net.Structure.IO.SerializationTokens.GraphSON2MimeType,
                        connectionPoolSettings, 
                        webSocketConfiguration
                        ); ;

                }
            );

            services.AddSingleton<GraphTraversalSource>(
                (serviceProvider) =>
                {
                    GremlinClient gremlinClient = serviceProvider.GetService<GremlinClient>();
                    var driverRemoteConnection = new DriverRemoteConnection(gremlinClient, "g");
                    return AnonymousTraversalSource.Traversal().WithRemote(driverRemoteConnection);
                }
            );

            services.AddRazorPages();
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsProduction())
            {
                app.UseExceptionHandler("/Home/Error");

                // Obtem o servidor de destino
                var strTarget = Configuration.GetValue<string>("Deploy:Target");
                Console.WriteLine($"Target : {Configuration.GetValue<string>("Deploy:Target")}");
                if (strTarget == "NGINX")
                {
                    // Para funcionar em sub-diretorio
                    Console.WriteLine($"UsePathBase: {Configuration.GetValue<string>("Deploy:UsePathBase")}");
                    app.UsePathBase(Configuration.GetValue<string>("Deploy:UsePathBase"));
                }
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
