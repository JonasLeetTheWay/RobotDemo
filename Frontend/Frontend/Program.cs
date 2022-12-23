using Grpc.Net.Client;
using Backend;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using Backend.Services;
using Backend.Protos;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
// Add services to the container.
services.AddControllersWithViews();

services.AddGrpcClient<RobotProto.RobotProtoClient>((serviceProvider, channel) => new RobotProto.RobotProtoClient(channel));
services.AddGrpcClient<LocationProto.LocationProtoClient>((serviceProvider, channel) => new LocationProto.LocationProtoClient(channel));


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
    endpoints.MapGrpcService<GreeterService>().RequireCors("AllowAll");
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
