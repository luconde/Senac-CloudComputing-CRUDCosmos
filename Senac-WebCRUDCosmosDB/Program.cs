using Senac_WebCRUDCosmosDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddControllersWithViews();


var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);


var app = builder.Build();
startup.Configure(app, builder.Environment);

