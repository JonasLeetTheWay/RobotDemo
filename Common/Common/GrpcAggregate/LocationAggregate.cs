using Common.Domain;
using Common.Protos;
using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Infrastructure;

namespace Common.GrpcAggregate;

public class LocationAggregate : LocationDAO
{

    // inner method to avoid repetitive work
    public LocationResponse MakeLocationResponse(string id, RepeatedField<string> robotIds, string? name, double? x, double? y)
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
    public static UpdateLocationObj MakeUpdateLocationObj(RepeatedField<string> robotIds, string? name = default)
    {
        var response = new UpdateLocationObj();
        response.Name = name;
        response.RobotIds.AddRange(robotIds);
        return response;
    }
    public static Location MakeLocation(double? x, double? y, string? name, RepeatedField<string>? robotIds = default)
    {
        if (robotIds?.Count == null || robotIds == null)
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
    public RepeatedField<string> makeFakeRobotIdsArray()
    {
        return new RepeatedField<string> { "fake1", "fake2" };
    }

    public static AddLocationObj CreateAddLocationObj(Location locationData)
    {
        return new AddLocationObj
        {
            Name = locationData.Name,
            X = locationData.X,
            Y = locationData.Y
        };
    }

    public static LocationIdAndUpdate CreateLocationIdAndUpdate(string id, UpdateLocationObj update)
    {
        return new LocationIdAndUpdate
        {
            Id = id,
            Update = update
        };
    }

    public static UpdateLocationObj CreateUpdateLocationObj(string name, IEnumerable<string> robotIds)
    {
        return new UpdateLocationObj
        {
            Name = name,
            RobotIds = { robotIds }
        };
    }

    public static LocationIdAndRobotId CreateLocationIdAndRobotId(string locationId, string robotId)
    {
        return new LocationIdAndRobotId
        {
            LocationId = locationId,
            RobotId = robotId
        };
    }
}

public class RobotAggregate
{
    public static AddRobotObj CreateAddRobotObj(RobotData robotData)
    {
        return new AddRobotObj
        {
            Name = robotData.Name,
            Type = robotData.Type,
            Status = robotData.Status,
            CurrentLocation = robotData.CurrentLocation
        };
    }

    public static RobotIdAndUpdate CreateRobotIdAndUpdate(string id, LocationObj currentLocation)
    {
        return new RobotIdAndUpdate
        {
            Id = id,
            CurrentLocation = currentLocation
        };
    }

    public static RobotList CreateRobotList(List<RobotObj> robots)
    {
        return new RobotList
        {
            Robots = { robots }
        };
    }

    public static RobotObj CreateRobotObj(RobotData robotData)
    {
        return new RobotObj
        {
            Name = robotData.Name,
            Type = robotData.Type,
            Status = robotData.Status,
            CurrentLocation = robotData.CurrentLocation,
            PreviousLocationIds = { robotData.PreviousLocationIds }
        };
    }

    public static class CreateRobotData(RobotObj robot)
    {
        return new RobotData
        {
            Id = robot.Id,
            Name = robot.Name,
            Type = robot.Type,
            Status = robot.Status,
            CurrentLocation = robot.CurrentLocation,
            PreviousLocationIds = { robot.PreviousLocationIds }
        };
    }
}
