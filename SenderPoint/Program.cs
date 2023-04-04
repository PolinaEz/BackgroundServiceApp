using GsmLibrary;
using SenderPoint;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SenderOptions>(builder.Configuration.GetSection("Sender"));
builder.Services.AddHostedService<Sender>();
builder.Services.AddSingleton<LbsService>();
var app = builder.Build();

await app.RunAsync();