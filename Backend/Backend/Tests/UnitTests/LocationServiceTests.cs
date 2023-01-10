using Backend.Services;
using Backend.Settings;
using Common.LoggerHelper;
using Common.Protos;
using Common.TestDataGenerator;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Tests.Server.UnitTests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Backend.UnitTests.LocationServiceTests;


public class LocationServiceTests
{

    private readonly LocationDAO dao;
    private readonly Mock<ILogger<LocationService>> mockLoggerLocationService = new Mock<ILogger<LocationService>>();
    private readonly Mock<ILogger<LocationDAO>> mockLoggerLocationDAO = new Mock<ILogger<LocationDAO>>();

    private readonly LocationService service;
    private readonly ITestOutputHelper _logger;
    private readonly LocationProtoTestDataGenerator td;




    public LocationServiceTests(ITestOutputHelper logger)
    {
        _logger = logger;

        // Initialize the LocationDAO instance with your desired connection string and database name
        var settings = Options.Create(new MongoDBSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "testDB",
            CollectionName_Locations = "testLocations"
        });

        dao = new LocationDAO(settings, mockLoggerLocationDAO.Object);
        dao.ClearCollection(); // Clear test collection before each test

        service = new LocationService(dao, mockLoggerLocationService.Object);

        td = new LocationProtoTestDataGenerator();

        td.SetCoordinateFilter(td.AddRequest.X, td.AddRequest.Y);
    }


    [Fact]
    public async Task AddLocation_ShouldInsertLocation_IntoDatabase()
    {
        // Arrange
        var location = td.AddRequest;

        // Act
        var inserted = await service.AddLocation(location, TestServerCallContext.Create());
        var locationId = inserted.Id;

        td.SetExpectedResponseId(locationId);
        var expected = td.ExpectedResponse;


        // Assert
        var result = await service.GetLocationById(new LocationId { Id = locationId }, TestServerCallContext.Create());

        _logger.WriteLine("inserted Id: " + locationId);
        _logger.WriteLine("retrieved Location with GetLocationById: " + LoggerHelper.GetString_LocationResponse(result));

        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.Name, result.Name);
        Assert.Equal(expected.X, result.X);
        Assert.Equal(expected.Y, result.Y);
    }

    [Fact]
    public async Task GetLocationById_ShouldReturnLocationResponse_ForValidId()
    {
        // Arrange
        var location = td.AddRequest;

        var inserted = await service.AddLocation(location, TestServerCallContext.Create());
        var locationId = inserted.Id;

        td.SetExpectedResponseId(locationId);
        var expected = td.ExpectedResponse;

        // Act
        var result = await service.GetLocationById(new LocationId { Id = locationId }, TestServerCallContext.Create());

        // Assert

        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.Name, result.Name);
        Assert.Equal(expected.X, result.X);
        Assert.Equal(expected.Y, result.Y);
    }

    [Fact]
    public async Task GetLocationByName_ShouldReturnLocationResponse_ForMatchingName()
    {
        // Arrange
        var location = td.AddRequest;

        var inserted = await service.AddLocation(location, TestServerCallContext.Create());
        var locationId = inserted.Id;

        td.SetExpectedResponseId(locationId);
        var expected = td.ExpectedResponse;

        // Act
        var result = await service.GetLocationByName(td.GetByNameRequest, TestServerCallContext.Create());

        // Assert

        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.Name, result.Name);
        Assert.Equal(expected.X, result.X);
        Assert.Equal(expected.Y, result.Y);
    }

    [Fact]
    public async Task GetLocationByCoordinate_ShouldReturnLocationResponse_ForMatchingCoordinate()
    {
        // Arrange
        var location = td.AddRequest;

        var inserted = await service.AddLocation(location, TestServerCallContext.Create());
        var locationId = inserted.Id;

        td.SetExpectedResponseId(locationId);
        var expected = td.ExpectedResponse;

        // Act
        var result = await service.GetLocationByCoordinate(td.GetByCoordinateRequest, TestServerCallContext.Create());

        // Assert

        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.Name, result.Name);
        Assert.Equal(expected.X, result.X);
        Assert.Equal(expected.Y, result.Y);
    }

    [Fact]
    public async Task GetLocationByRobotId_ShouldReturnLocationResponse_ForMatchingRobotId()
    {
        // Arrange
        var location = td.AddRequest;
        var inserted = await service.AddLocation(location, TestServerCallContext.Create());
        var locationId = inserted.Id;

        td.SetExpectedResponseId(locationId);

        var robotId = ObjectId.GenerateNewId().ToString();
        var robotId2 = ObjectId.GenerateNewId().ToString();
        var robotId3 = ObjectId.GenerateNewId().ToString();

        td.SetRobotData(robotId);
        td.SetAddRobotToLocationRequest(locationId, robotId);

        td.SetLocationUpdateRequestData(location.Name, new List<string> { robotId, robotId2, robotId3 });

        //td.SetLocationUpdateRequest(locationId, td.UpdateRequest);
        //await service.UpdateLocation(td.UpdateLocationRequest, TestServerCallContext.Create());

        await service.AddRobotToLocation(new LocationIdAndRobotId { RobotId = robotId, LocationId = locationId }, TestServerCallContext.Create());

        td.SetAddRobotToLocationRequest(locationId, robotId2);
        await service.AddRobotToLocation(new LocationIdAndRobotId { RobotId = robotId2, LocationId = locationId }, TestServerCallContext.Create());

        td.SetAddRobotToLocationRequest(locationId, robotId3);
        await service.AddRobotToLocation(new LocationIdAndRobotId { RobotId = robotId3, LocationId = locationId }, TestServerCallContext.Create());


        // Act
        var result = (await service.GetLocationByRobotId(new RobotId { Id = robotId }, TestServerCallContext.Create())).Locations.First();
        _logger.WriteLine(LoggerHelper.GetString_LocationResponse(result));

        var expected = td.ExpectedResponse;

        // Assert
        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.Name, result.Name);
        Assert.Equal(expected.X, result.X);
        Assert.Equal(expected.Y, result.Y);
        Assert.Contains(robotId, result.RobotIds);
    }

    [Fact]
    public async Task GetLocations_ShouldReturnLocationsResponse_InDatabase()
    {
        // Arrange
        td.SetLocationData("Test Location 1", 1.0, 2.0);
        var location1 = td.AddRequest;

        var inserted1 = await service.AddLocation(location1, TestServerCallContext.Create());
        var locationId1 = inserted1.Id;

        td.SetExpectedResponseId(locationId1);
        var expected1 = td.ExpectedResponse;



        td.SetLocationData("Test Location 2", 3.0, 4.0);
        var location2 = td.AddRequest;

        var inserted2 = await service.AddLocation(location2, TestServerCallContext.Create());
        var locationId2 = inserted2.Id;

        td.SetExpectedResponseId(locationId2);
        var expected2 = td.ExpectedResponse;

        _logger.WriteLine(LoggerHelper.GetString_LocationResponse(expected1));
        _logger.WriteLine(LoggerHelper.GetString_LocationResponse(expected2));
        _logger.WriteLine("\n----------------------\n");

        // Act
        var result = (await service.GetLocations(new Empty(), TestServerCallContext.Create())).Locations;
        // Assert
        var expected = new List<LocationResponse> { expected1, expected2 };
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, result[i].Id);
            Assert.Equal(expected[i].Name, result[i].Name);
            Assert.Equal(expected[i].X, result[i].X);
            Assert.Equal(expected[i].Y, result[i].Y);
            _logger.WriteLine(LoggerHelper.GetString_LocationResponse(expected[i]));
            _logger.WriteLine(LoggerHelper.GetString_LocationResponse(result[i]));
        }
    }
    [Fact]
    public async Task UpdateLocation_ShouldUpdateLocation_InDatabase()
    {
        // Arrange
        td.SetLocationData("Test Location 1", 1.0, 2.0);
        var init = td.AddRequest;
        var inserted = await service.AddLocation(init, TestServerCallContext.Create());
        var locationId = inserted.Id;

        td.SetExpectedResponseId(locationId);

        td.SetLocationUpdateRequestData("Test Location 2", new List<string> { "Robot 1", "Robot 2" });
        td.SetLocationUpdateRequest(locationId, td.UpdateRequest);

        // Act
        await service.UpdateLocation(td.UpdateLocationRequest, TestServerCallContext.Create());

        // Assert
        var result = await service.GetLocationById(new LocationId { Id = locationId }, TestServerCallContext.Create());

        var expected = td.ExpectedResponse;
        _logger.WriteLine(LoggerHelper.GetString_LocationResponse(expected));
        _logger.WriteLine(LoggerHelper.GetString_LocationResponse(result));

        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.Name, result.Name);
        Assert.Equal(expected.X, result.X);
        Assert.Equal(expected.Y, result.Y);
    }

    [Fact]
    public async Task AddRobotToLocation_ShouldAddRobot_ToLocation()
    {
        // Arrange
        td.SetLocationData("Test Location 1", 1.0, 2.0);
        var init = td.AddRequest;
        var inserted = await service.AddLocation(init, TestServerCallContext.Create());
        var locationId = inserted.Id;
        var robotId = ObjectId.GenerateNewId().ToString();

        td.SetExpectedResponseId(locationId);

        td.SetAddRobotToLocationRequest(locationId, robotId);

        // Act
        await service.AddRobotToLocation(new LocationIdAndRobotId { RobotId = robotId, LocationId = locationId }, TestServerCallContext.Create());
        var result = await service.GetLocationById(new LocationId { Id = locationId }, TestServerCallContext.Create());
        var expected = td.ExpectedResponse;

        // Assert
        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.Name, result.Name);
        Assert.Equal(expected.X, result.X);
        Assert.Equal(expected.Y, result.Y);
        Assert.Contains(robotId, result.RobotIds);
    }

    [Fact]
    public async Task DeleteLocation_ShouldDeleteLocation_FromDatabase()
    {
        // Arrange
        td.SetLocationData("Test Location 1", 1.0, 2.0);
        var init = td.AddRequest;
        var loc = (await service.AddLocation(init, TestServerCallContext.Create()));
        var locationId = loc.Id;

        // Act
        await service.DeleteLocation(new LocationId { Id = locationId }, TestServerCallContext.Create());
        _logger.WriteLine("deleted");
        _logger.WriteLine("locationId: " + locationId);

        var locResponses = (await service.GetLocations(new Empty(), TestServerCallContext.Create())).Locations;
        _logger.WriteLine("locResponses count: " + locResponses.Count);
        foreach (var locResponse in (await service.GetLocations(new Empty(), TestServerCallContext.Create())).Locations)
        {
            _logger.WriteLine("locResponse.Id: " + locResponse.Id);
        }


        // Assert
        var deletedLocation = dao.VerifyExistance(td.ExpectedLocation);
        _logger.WriteLine("deleted location:" + deletedLocation?.ToString());
        Assert.Null(deletedLocation);

        try
        {
            var result = await service.GetLocationById(new LocationId { Id = locationId }, TestServerCallContext.Create());
        }
        catch (RpcException e)
        {
            _logger.WriteLine("RpcException: " + e.Message);
            Assert.Equal(StatusCode.NotFound, e.StatusCode);
            Assert.Equal("Location not found", e.Status.Detail);

        }

    }


    [Fact]
    public async Task RemoveRobotFromLocation_ShouldRemoveRobotFromLocation_ForMatchingLocationAndRobotId()
    {
        //dao.ClearCollection();

        // Arrange
        var location = td.AddRequest;
        var robotId = ObjectId.GenerateNewId().ToString();
        var robotId2 = ObjectId.GenerateNewId().ToString();
        td.SetRobotData(robotId);

        var inserted = await service.AddLocation(location, TestServerCallContext.Create());
        var locationId = inserted.Id;

        td.SetExpectedResponseId(locationId);

        await service.AddRobotToLocation(new LocationIdAndRobotId { RobotId = robotId, LocationId = locationId }, TestServerCallContext.Create());
        await service.AddRobotToLocation(new LocationIdAndRobotId { RobotId = robotId2, LocationId = locationId }, TestServerCallContext.Create());

        // Act
        await service.RemoveRobotFromLocation(new LocationIdAndRobotId { RobotId = robotId, LocationId = locationId }, TestServerCallContext.Create());

        // Assert
        var result = await service.GetLocationById(new LocationId { Id = locationId }, TestServerCallContext.Create());
        var expected = td.ExpectedResponse;

        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.Name, result.Name);
        Assert.Equal(expected.X, result.X);
        Assert.Equal(expected.Y, result.Y);

        Assert.DoesNotContain(robotId, result.RobotIds);
        Assert.Contains(robotId2, result.RobotIds);


        _logger.WriteLine(LoggerHelper.GetString_LocationResponse(expected));
        _logger.WriteLine(LoggerHelper.GetString_LocationResponse(result));
    }


    ///////////////////////////////////////////////////////////////////////////////


}



//[TestCase(new LocationDAOStub1())]
//[TestCase(new LocationDAOStub2())]
//[TestCase(new LocationDAOStub3())]
//public void TestLocationServiceWithDifferentStubObjects(LocationDAO locationDAO)
//{
//    // Arrange
//    var service = new LocationService(locationDAO);

//    // Act and assert
//    TestAddLocation(service);
//    TestGetLocationById(service);
//    TestUpdateLocation(service);
//    TestDeleteLocation(service);
//    TestListLocations(service);
//}

//public class LocationServiceStub : LocationProto.LocationProtoBase
//{
//    private readonly LocationDAOStub locationDAO;

//    public LocationServiceStub(LocationDAOStub locationDAO)
//    {
//        this.locationDAO = locationDAO;
//    }

//    public override Task<LocationId> AddLocation(AddLocationRequest request, ServerCallContext context)
//    {
//        return Task.FromResult(new LocationId { Id = locationDAO.InsertLocation(new Location()) });
//    }

//    public override Task<LocationResponse> GetLocationById(LocationId locationId, ServerCallContext context)
//    {
//        var location = locationDAO.FindLocations(l => l.Id == locationId.Id).SingleOrDefault();
//        if (location == null)
//        {
//            throw new RpcException(new Status(StatusCode.NotFound, "Location not found"));
//        }
//        return Task.FromResult(location.ToLocationResponse());
//    }

//    public override Task<Empty> UpdateLocation(LocationIdAndUpdate request, ServerCallContext context)
//    {
//        locationDAO.UpdateLocation(request.Id, new Location());
//        return Task.FromResult(new Empty());
//    }

//    public override Task<Empty> DeleteLocation(LocationId locationId, ServerCallContext context)
//    {
//        locationDAO.DeleteLocation(locationId.Id);
//        return Task.FromResult(new Empty());
//    }
//}

//public class LocationDAOStub : LocationDAO
//{
//    public LocationDAOStub(IOptions<MongoDBSettings> settings, ILogger logger) : base(settings, logger)
//    {
//    }

//    public string InsertLocation(Location location)
//    {
//        return location.Id;
//    }

//    // not match the real return type
//    public bool UpdateLocation(string id, Location update)
//    {
//        return true;
//    }

//    // not match the real return type
//    public bool DeleteLocation(string id)
//    {
//        return true;
//    }

//    public IEnumerable<Location> FindLocations(Func<Location, bool> filter) // a filter delegate
//    {
//        return new List<Location> { new Location { Id = "1", Name = "Location 1", X = 1, Y = 2 } };
//    }
//}