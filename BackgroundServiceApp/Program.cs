using BackgroundServiceApp;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<Sender>();
builder.Services.AddHostedService<Listener>();
builder.Services.AddSingleton<UdpClient>(provider => new UdpClient(22220));
var app = builder.Build();

await app.RunAsync();