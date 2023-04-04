using GsmLibrary;
using ListenerPoint;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ListenerOptions>(builder.Configuration.GetSection("Listener"));
builder.Services.AddHostedService<Listener>();
builder.Services.AddSingleton<LbsService>();
var app = builder.Build();

await app.RunAsync();