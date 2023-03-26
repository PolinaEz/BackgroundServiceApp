using BackgroundServiceApp;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ListenerOptions>(builder.Configuration.GetSection("Listener"));
builder.Services.Configure<SenderOptions>(builder.Configuration.GetSection("Sender"));
builder.Services.AddHostedService<Sender>();
builder.Services.AddHostedService<Listener>();
builder.Services.AddSingleton<LbsService>();
var app = builder.Build();

await app.RunAsync();