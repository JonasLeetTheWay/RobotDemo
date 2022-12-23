using Common.Domain;
using Backend.Infrastructure;
using Common.Protos.Location;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MongoDB.Bson;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
//using Common.Protos.Robot;
using MongoDB.Driver;
using System.Reflection;

namespace Backend.Services;

public class LocationService : LocationProto.LocationProtoBase
{
    private readonly LocationDAO locationDAO;
    private readonly ILogger _logger;

    public LocationService(LocationDAO locationDAO, ILogger logger)
    {
        this.locationDAO = locationDAO;
        _logger = logger;
    }

    ////////////////////////////////////// inner methods to avoid repetitive work //////////////////////////////////////
    private LocationResponse MakeLocationResponse(string id, IEnumerable<string> robotIds, string? name, double? x, double? y)
    {
        // check if there is an existingDoc that matches id 

        var existing = locationDAO.FindLocations(l => l.Id == id).SingleOrDefault();
        var response = new LocationResponse();

        if (existing != null)
        {
            response.Id = id;
            response.Name = existing.Name ?? "null";
            response.X = existing.X ?? double.MinValue;
            response.Y = existing.Y ?? double.MinValue;
            if (existing.RobotIds != null)
            {
                response.RobotIds.AddRange(existing.RobotIds);
            }
                
        }
        else
        {
            response.Id = id;
            response.Name = name ?? "null";
            response.X = x ?? double.MinValue;
            response.Y = y ?? double.MinValue;
        }

        response.RobotIds.AddRange(robotIds);

        return response;
    }

    private static UpdateLocationObj MakeUpdateLocationObj(IEnumerable<string> robotIds, string? name = default)
    {
        var response = new UpdateLocationObj();
        response.Name = name;
        response.RobotIds.AddRange(robotIds);
        return response;
    }
    private static Location MakeLocation(double? x, double? y, string? name, IEnumerable<string>? robotIds = default)
    {
        if(robotIds?.ToList().Count == null || robotIds == null)
        {
            return new Location
            {
                Name = name ?? "null",
                X = x ?? double.MinValue,
                Y = y ?? double.MinValue,
            };
        }
        return new Location
        {
            Name = name ?? "null",
            X = x ?? double.MinValue,
            Y = y ?? double.MinValue,
            RobotIds = robotIds.ToList() // because Google.Protobuf.Collections.RepeatedField is not a RECOGNIZED LIST
        };
    }
    private IEnumerable<string> makeFakeRobotIdsArray()
    {
        return new List<string> { "fake1", "fake2" };
    }

    ////////////////////////////////////// inner methods to avoid repetitive work //////////////////////////////////////

    // Implement the AddLocation method
    public override Task<LocationId> AddLocation(AddLocationRequest request, ServerCallContext context)
    {
        // Validate the request
        if (request == null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
        }
        // Add the location to the database
        var obj = MakeLocation(request.X, request.Y, request.Name);
        var insertedId = locationDAO.InsertLocation(obj);
        var response = new LocationId { Id = insertedId };

        return Task.FromResult(response);
    }


    // Implement the GetLocationById method
    public override Task<LocationResponse> GetLocationById(LocationId locationId, ServerCallContext context)
    {
        Console.WriteLine("server, getlocationbyid: "+ locationId.Id);
        // Validate the request
        if (string.IsNullOrEmpty(locationId.Id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
        }

        // Find the location in the database
        var location = locationDAO.FindLocations(l => l.Id == locationId.Id).SingleOrDefault(); // unique id for every object
        if (location == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Location not found"));
        }

        var response = MakeLocationResponse(location.Id, makeFakeRobotIdsArray(), location.Name, location.X,location.Y);

        return Task.FromResult(response);
    }
    
    
    // Implement the GetLocationByRobotId method
    public override Task<LocationResponse> GetLocationByRobotId(RobotId robotId, ServerCallContext context)
    {
        // Validate the request
        if (string.IsNullOrEmpty(robotId.Id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
        }

        // Find the location in the database
        var location = locationDAO.FindLocations(l => l.Id == robotId.Id).SingleOrDefault();
        if (location == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Location not found"));
        }

        var response = MakeLocationResponse(location.Id, makeFakeRobotIdsArray(), location.Name, location.X, location.Y);

        return Task.FromResult(response);
    }
    // Implement the GetLocationByName method
    public override Task<LocationResponse> GetLocationByName(Name request, ServerCallContext context)
    {
        // Validate the request
        if (string.IsNullOrEmpty(request.Value))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
        }
        // Find the location in the database
        var location = locationDAO.FindLocations(l => l.Id == request.Value).SingleOrDefault();
        if (location == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Location not found"));
        }
        var response = MakeLocationResponse(location.Id, makeFakeRobotIdsArray(), location.Name, location.X, location.Y);
        
        return Task.FromResult(response);

    }
    // Implement the GetLocationByName method
    public override Task<LocationResponse> GetLocationByCoordinate(Coordinate request, ServerCallContext context)
    {
        // Validate the request
        if (request.X == double.MinValue || request.Y == double.MinValue)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
        }
        // Find the location in the database
        var coordinateFilter = Builders<Location>.Filter.Eq("x", request.X) & Builders<Location>.Filter.Eq("y", request.Y);
        var location = locationDAO.FindLocations(coordinateFilter).SingleOrDefault();
        if (location == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Location not found"));
        }
        var response = MakeLocationResponse(location.Id, makeFakeRobotIdsArray(), location.Name, location.X, location.Y);
        
        return Task.FromResult(response);

    }

    // Implement the GetLocations method
    public override Task<LocationsResponse> GetLocations(Empty request, ServerCallContext context)
    {
        // Find all locations in the database
        var locations = locationDAO.GetLocations();

        // Return the found locations in the response
        LocationsResponse response = new LocationsResponse();
        List<LocationResponse> buffer = new();
        //await responseStream.WriteAsync(locations);
        var robotIds = new List<string> { "fake1", "fake2" };

        foreach (var loc in locations)
        {
            buffer.Add(MakeLocationResponse(loc.Id, makeFakeRobotIdsArray(), loc.Name, loc.X, loc.Y));
        }
        Console.WriteLine("buffer inside GetLocations: ", buffer);

        response.Locations.AddRange(buffer);
        //await responseStream.WriteAsync(response);
        return Task.FromResult(response);

    }

    // Implement the UpdateLocation method
    public override Task<Empty> UpdateLocation(LocationIdAndUpdate request, ServerCallContext context)
    {
        // Validate the request
        if (request == null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
        }

        // intepret each attribute of request.Update (a UpdateLocationObj)

        var update = MakeLocation(default,default,request.Update.Name, request.Update.RobotIds);

        // Update the location in the database
        var innerResult = locationDAO.UpdateLocation(request.Id, update);
        // logging innerResult with function name 
        _logger.LogInformation($"{MethodBase.GetCurrentMethod().Name} - {innerResult}");

        return Task.FromResult(new Empty());
    }

    // Implement the DeleteLocation method
    public override Task<Empty> DeleteLocation(LocationId request, ServerCallContext context)
    {
        // Validate the request
        if (string.IsNullOrEmpty(request.Id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
        }

        // Delete the location from the database
        var innerResult = locationDAO.DeleteLocation(request.Id);
        // logging innerResult with function name 
        _logger.LogInformation($"{MethodBase.GetCurrentMethod().Name} - {innerResult}");
        
        return Task.FromResult(new Empty());
    }


    // Implement the AddRobotToLocation method
    public override Task<Empty> AddRobotToLocation(LocationIdAndRobotId request, ServerCallContext context)
    {
        var locationId = request.LocationId;
        var robotId = request.RobotId;
        // Validate the request
        if (string.IsNullOrEmpty(locationId) || string.IsNullOrEmpty(robotId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
        }

        // Add the robot to the location in the database
        var innerResult = locationDAO.AddRobotToLocation(locationId, robotId);
        // logging innerResult with function name 
        _logger.LogInformation($"{MethodBase.GetCurrentMethod().Name} - {innerResult}");

        return Task.FromResult(new Empty());
    }

    // Implement the RemoveRobotFromLocation method
    public override Task<Empty> RemoveRobotFromLocation(LocationIdAndRobotId request, ServerCallContext context)
    {
        var locationId = request.LocationId;
        var robotId = request.RobotId;
        // Validate the request
        if (string.IsNullOrEmpty(locationId) || string.IsNullOrEmpty(robotId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
        }

        // Remove the robot from the location in the database
        var innerResult = locationDAO.RemoveRobotFromLocation(locationId, robotId);
        // logging innerResult with function name 
        _logger.LogInformation($"{MethodBase.GetCurrentMethod().Name} - {innerResult}");

        return Task.FromResult(new Empty());
    }


}
