using Common.Protos;
using Common.Domain;
using MongoDB.Bson;

namespace Common.TestDataGenerator;

public class LocationProtoTestDataGenerator
{
    public AddLocationRequest AddRequest { get; set; } = new();
    public Location ExpectedLocation { get; set; } = new();
    public LocationId GetByIdRequest { get; set; } = new();
    public RobotId GetByRobotIdRequest { get; set; } = new();
    public Name GetByNameRequest { get; set; } = new();
    /// snippet to write
    public UpdateLocationObj UpdateRequest { get; set; } = new();
    public LocationIdAndUpdate UpdateLocationRequest { get; set; } = new();
    public LocationIdAndRobotId AddRobotToLocationRequest { get; set; } = new();
    public LocationId DeleteLocationRequest { get; set; } = new();
    public Coordinate GetByCoordinateRequest { get; set; } = new();
    public LocationIdAndRobotId RemoveRobotFromLocationRequest { get; set; } = new();
    /// end of snippet to write
    public LocationResponse ExpectedResponse { get; set; } = new();
    public LocationsResponse ExpectedResponses { get; set; } = new();

    public LocationProtoTestDataGenerator(string? name = null)
    {
        SetLocationData(name);
    }

    ////////////////////////////////// MAJOR DATA SETTERS //////////////////////////////////

    public void SetLocationData(string name = null, double x = 1.0, double y = 2.0, string? locationId = null)
    {
        name ??= "Test Location";
        locationId ??= ObjectId.GenerateNewId().ToString();
        AddRequest = new AddLocationRequest
        {
            Name = name,
            X = x,
            Y = y
        };
        ExpectedLocation = new Location
        {
            Name = name,
            X = x,
            Y = y
        };
        GetByIdRequest = new LocationId
        {
            Id = locationId
        };

        GetByNameRequest = new Name { Value = name };

        ExpectedResponse = new LocationResponse
        {
            Id = locationId,
            Name = name,
            X = x,
            Y = y
        };
    }

    public void SetRobotData(string? robotId = null, double robotX = 3.0, double robotY = 4.0, IEnumerable<string>? robotIds = null)
    {
        robotId ??= ObjectId.GenerateNewId().ToString();
        GetByRobotIdRequest = new RobotId
        {
            Id = robotId
        };

        if (robotIds != null)
        {
            ExpectedResponse.RobotIds.AddRange(robotIds);
        }
    }

    ////////////////////////////////// FILTER SETTERS //////////////////////////////////
    public void SetCoordinateFilter(double coordX = 5.0, double coordY = 6.0)
    {
        GetByCoordinateRequest = new Coordinate
        {
            X = coordX,
            Y = coordY
        };
    }


    public void SetLocationIdFilter(string locationId)
    {
        GetByIdRequest.Id = locationId;
    }
    public void SetRobotIdFilter(string robotId)
    {
        GetByRobotIdRequest.Id = robotId;
    }
    public void SetExpectedResponseId(string locationId)
    {
        ExpectedResponse.Id = locationId;
    }
    public void SetExpectedLocationId(string locationId)
    {
        ExpectedLocation.Id = locationId;
    }

    ////////////////////////////////// LOCATION UPDATE & DELETE SETTERS //////////////////////////////////

    public void SetLocationUpdateRequestData(string locationName, IEnumerable<string> robotIds)
    {
        UpdateRequest = new UpdateLocationObj
        {
            Name = locationName,
            RobotIds = { robotIds }
        };

        ExpectedResponse.RobotIds.AddRange(robotIds);
        ExpectedLocation.RobotIds = robotIds.ToList();
        
        ExpectedResponse.Name = locationName;
        ExpectedLocation.Name = locationName;
        GetByNameRequest.Value = locationName;


    }
    public void SetLocationUpdateRequest(string locationId, UpdateLocationObj updateLocationObj)
    {
        UpdateLocationRequest = new LocationIdAndUpdate
        {
            Id = locationId,
            Update = updateLocationObj
        };
    }

    public void SetDeleteLocationRequest(string locationId)
    {
        DeleteLocationRequest = new LocationId
        {
            Id = locationId
        };
    }

    ////////////////////////////////// ROBOT TO LOCATION SETTERS //////////////////////////////////

    public void SetAddRobotToLocationRequest(string locationId, string robotId)
    {
        AddRobotToLocationRequest = new LocationIdAndRobotId
        {
            LocationId = locationId,
            RobotId = robotId
        };
        ExpectedResponse.RobotIds.Add(robotId);
    }
    public void SetRemoveRobotFromLocationRequest(string locationId, string robotId)
    {
        RemoveRobotFromLocationRequest = new LocationIdAndRobotId
        {
            LocationId = locationId,
            RobotId = robotId
        };
        ExpectedResponse.RobotIds.Remove(robotId);
    }

}