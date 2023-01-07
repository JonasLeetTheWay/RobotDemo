// See https://aka.ms/new-console-template for more information
using Grpc.Net.Client;
using Common.Protos;

using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;

Console.WriteLine("Hello, World!");
var channel = GrpcChannel.ForAddress("https://localhost:7081");
var robotClient = new RobotProto.RobotProtoClient(channel);
var locationClient = new LocationProto.LocationProtoClient(channel);

/*
Robot {
  string id = 1;
  string name = 2;
  string type = 3;
  string current_location = 4;
  repeated string previous_locations = 5;
}
 */
//var r = new Robot
//{
//    Name = "Robot1",
//    Type = "hard",
//    CurrentLocation = "1",
//};
while(Console.ReadKey().Key == ConsoleKey.Enter)
{
    var l = new AddLocationRequest
    {
        Name = "a",
        X = 10,
        Y = -10,
    };
    var l2 = new UpdateLocationObj();
    l2.Name = "dd";
    List<string> fakeIds = new() { "1", "1", "1" };
    l2.RobotIds.AddRange(fakeIds);

    var res = locationClient.AddLocation(l);
    Console.WriteLine("added location id: ", res.Id);
    Console.WriteLine(locationClient.GetLocationById(new Common.Protos.LocationId { Id = res.Id }));


    locationClient.UpdateLocation(new LocationIdAndUpdate { Id = "63a5c1d2870279a77e77499a", Update = l2 });
    Console.WriteLine(locationClient.GetLocationById(new Common.Protos.LocationId { Id = res.Id }));

    var response = locationClient.GetLocations(new Empty());

    string json = JsonConvert.SerializeObject(response, Formatting.Indented);
    Console.WriteLine(json);
}

