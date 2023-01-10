// See https://aka.ms/new-console-template for more information
using Common.Protos;
using Common.TestDataGenerator;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using MongoDB.Bson;
using Newtonsoft.Json;

Console.WriteLine("Hello, World!");
var channel = GrpcChannel.ForAddress("https://localhost:7081");
var robotClient = new RobotProto.RobotProtoClient(channel);
var locationClient = new LocationProto.LocationProtoClient(channel);


while (Console.ReadKey().Key == ConsoleKey.Enter)
{

    var data_all = locationClient.GetLocations(new Empty());
    string json = JsonConvert.SerializeObject(data_all, Formatting.Indented);
    Console.WriteLine("all locations before starting: " + json);
    var td = new LocationProtoTestDataGenerator(); // td : testdata

    var init = locationClient.AddLocation(td.AddRequest);
    var locationId = init.Id;
    var robotId = ObjectId.GenerateNewId().ToString();
    td.SetLocationUpdateRequestData("ddd", new List<string> { robotId, "b", "c" });

    Console.WriteLine("added location id: " + locationId);
    Console.WriteLine("generated robot id: " + robotId);

    td.SetRobotData(robotId);
    td.SetAddRobotToLocationRequest(locationId, robotId);

    td.SetLocationIdFilter(locationId);
    td.SetRobotIdFilter(robotId);


    Console.WriteLine("\n");

    var data = locationClient.GetLocationById(td.GetByIdRequest);
    Console.WriteLine("get location by id: " + data);
    Console.WriteLine("\n");

    td.SetLocationUpdateRequest(locationId, td.UpdateRequest);
    locationClient.UpdateLocation(td.UpdateLocationRequest);


    var data2 = locationClient.GetLocationById(td.GetByIdRequest);
    Console.WriteLine("update location by id: " + data2);
    Console.WriteLine("\n");

    locationClient.AddRobotToLocation(td.AddRobotToLocationRequest);
    var data3 = locationClient.GetLocationByRobotId(td.GetByRobotIdRequest);
    Console.WriteLine("get location by robot id: " + data3);
    Console.WriteLine("\n");


    data_all = locationClient.GetLocations(new Empty());
    json = JsonConvert.SerializeObject(data_all, Formatting.Indented);
    Console.WriteLine("all locations after updating location with RobotIds data: " + json);



    // after adding location data & robot data, now GetLocationById shall return Location with both data aspect
    var data4 = locationClient.GetLocationById(new LocationId { Id = init.Id });
    Console.WriteLine("after adding location data & robot data,\nget location by id: " + data4);
    Console.WriteLine("\n");

    // GetLocationByRobotId shall return the same object
    var data5 = locationClient.GetLocationByRobotId(td.GetByRobotIdRequest);
    Console.WriteLine("GetLocationByRobotId shall return the same object: " + data5);
    Console.WriteLine("\n");

    td.SetCoordinateFilter(td.AddRequest.X, td.AddRequest.Y);

    // GetLocationByCoordinate shall return the same object
    var data6 = locationClient.GetLocationByCoordinate(td.GetByCoordinateRequest);
    Console.WriteLine("GetLocationByCoordinate shall return the same object: " + data6);
    Console.WriteLine("\n");

    // GetLocationByName shall return the same object
    var data7 = locationClient.GetLocationByName(td.GetByNameRequest);
    Console.WriteLine("GetLocationByName shall return the same object: " + data7);
    Console.WriteLine("\n");

    // after removing robot from location, show GetLocationByRobotId
    locationClient.RemoveRobotFromLocation(new LocationIdAndRobotId { LocationId = init.Id, RobotId = robotId });
    var datae = locationClient.GetLocationByRobotId(td.GetByRobotIdRequest);
    Console.WriteLine("get location by robot id after removing robot from location: " + datae);
    Console.WriteLine("\n");

    data_all = locationClient.GetLocations(new Empty());
    json = JsonConvert.SerializeObject(data_all, Formatting.Indented);
    Console.WriteLine("all locations after removing robotId: " + json);
    Console.WriteLine("\n");

    // delete location
    locationClient.DeleteLocation(new LocationId { Id = init.Id });
    data_all = locationClient.GetLocations(new Empty());
    json = JsonConvert.SerializeObject(data_all, Formatting.Indented);
    Console.WriteLine("all locations after deleting: " + json);

}

