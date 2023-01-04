using Backend.Infrastructure;
using Backend.Services;
using Backend.Settings;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.
services.AddGrpc();

services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDB"));

services.AddScoped<LocationDAO>();
services.AddScoped<RobotDAO>();
services.AddScoped<ILogger,Logger<LocationDAO>>();


builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<LocationService>();
//app.MapGrpcService<RobotService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
