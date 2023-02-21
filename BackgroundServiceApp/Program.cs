using BackgroundServiceApp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<MyBackgroundService>(); 
var app = builder.Build();

await app.RunAsync();