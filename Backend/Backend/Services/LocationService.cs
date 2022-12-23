using Backend.Domain;
using Backend.Infrastructure;
using Backend.Protos;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MongoDB.Bson;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Backend.Services;

public class LocationService : LocationProto.LocationProtoBase
{
    private readonly LocationDAO locationDAO;

    public LocationService(LocationDAO locationDAO)
    {
        this.locationDAO = locationDAO;
    }
    // inner method to avoid repetitive work
    private LocationResponse MakeLocationResponse(string id, RepeatedField<string> robotIds, string? name, double? x, double? y)
    {
        // check if there is an existingDoc that matches id 

        var existing = locationDAO.FindLocations(l => l.Id == id).SingleOrDefault();
        var response = new LocationResponse();

        if (existing != null)
        {
            response.Id = id;
            response.Name = existing.Name ?? "null";
            response.X = existing.X ?? double.NaN;
            response.Y = existing.Y ?? double.NaN;
            if (existing.RobotIds != null)
            {
                response.RobotIds.AddRange(existing.RobotIds);
            }
                
        }
        else
        {
            response.Id = id;
            response.Name = name ?? "null";
            response.X = x ?? double.NaN;
            response.Y = y ?? double.NaN;
        }

        response.RobotIds.AddRange(robotIds);

        return response;
    }
    private static UpdateLocationObj MakeUpdateLocationObj(RepeatedField<string> robotIds, string? name = default)
    {
        var response = new UpdateLocationObj();
        response.Name = name;
        response.RobotIds.AddRange(robotIds);
        return response;
    }
    private static Location MakeLocation(double? x, double? y, string? name, RepeatedField<string>? robotIds = default)
    {
        if(robotIds?.Count == null || robotIds == null)
        {
            return new Location
            {
                Name = name ?? "null",
                X = x ?? double.NaN,
                Y = y ?? double.NaN,
            };
        }
        return new Location
        {
            Name = name ?? "null",
            X = x ?? double.NaN,
            Y = y ?? double.NaN,
            RobotIds = robotIds.ToList() // because Google.Protobuf.Collections.RepeatedField is not a RECOGNIZED LIST
        };
    }
    private RepeatedField<string> makeFakeRobotIdsArray()
    {
        return new RepeatedField<string> { "fake1", "fake2" };
    }

    // Implement the AddLocation method
    public override Task<LocationId> AddLocation(AddLocationObj request, ServerCallContext context)
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
    public override Task<LocationResponse> GetLocationById(StringValue locationId, ServerCallContext context)
    {
        Console.WriteLine("server, getlocationbyid: "+ locationId.Value);
        // Validate the request
        if (string.IsNullOrEmpty(locationId.Value))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
        }

        // Find the location in the database
        var location = locationDAO.FindLocations(l => l.Id == locationId.Value).SingleOrDefault(); // unique id for every object
        if (location == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Location not found"));
        }

        var response = MakeLocationResponse(location.Id, makeFakeRobotIdsArray(), location.Name, location.X,location.Y);

        return Task.FromResult(response);
    }
    

    // Implement the GetLocationByRobotId method
    public override Task<LocationResponse> GetLocationByRobotId(StringValue robotId, ServerCallContext context)
    {
        // Validate the request
        if (string.IsNullOrEmpty(robotId.Value))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
        }

        // Find the location in the database
        var location = locationDAO.FindLocations(l => l.Id == robotId.Value).SingleOrDefault();
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
        var locations = locationDAO.FindLocations();

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
        locationDAO.UpdateLocation(request.Id, update);

        // Return the updated location in the response
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
        locationDAO.DeleteLocation(request.Id);

        // Return an empty response
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
        locationDAO.AddRobotToLocation(locationId, robotId);

        // Return an empty response
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
        locationDAO.RemoveRobotFromLocation(locationId, robotId);

        // Return an empty response
        return Task.FromResult(new Empty());
    }


}
