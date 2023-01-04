using Grpc.Net.Client;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using Backend.Services;
using Common.Protos.Location;
using Common.Protos.Robot;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
// Add services to the container.
services.AddControllersWithViews();

// Add gRPC client services
services.AddGrpcClient<LocationProto.LocationProtoClient>(o => o.Address = new Uri("https://localhost:5001"));
services.AddGrpcClient<RobotProto.RobotProtoClient>(o => o.Address = new Uri("https://localhost:5001"));

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 7081, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
        //listenOptions.UseHttps("<path to .pfx file>",
        //    "<certificate password>");
    });
});


var app = builder.Build();

//// The port number must match the port of the gRPC server.
//using var channel = GrpcChannel.ForAddress("https://localhost:7042");

//var locationClient = new LocationProto.LocationProtoClient(channel);
//var robotClient = new RobotProto.RobotProtoClient(channel);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.UseCors();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<LocationService>().RequireCors("AllowAll");
    //endpoints.MapGrpcService<RobotService>().RequireCors("AllowAll");
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
