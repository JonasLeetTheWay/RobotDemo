using Backend.Infrastructure;
using Backend.Services;
using Backend.Settings;
using Common.Domain;
using Common.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using System.Linq.Expressions;
using Tests.Server.UnitTests.Helpers;
using Xunit;

namespace Backend.Tests;

public class LocationServiceTests
{
    private readonly Mock<ILocationDAO> mockLocationDAO = new Mock<ILocationDAO>();
    private readonly Mock<ILogger<LocationService>> mockLogger = new Mock<ILogger<LocationService>>();
    private readonly LocationService service;

    public LocationServiceTests()
    {
        service = new LocationService(mockLocationDAO.Object, mockLogger.Object);
    }

    // Test data class to store common test data
    public class TestData
    {
        public AddLocationRequest AddRequest { get; set; }
        public Location ExpectedLocation { get; set; }
        public LocationId GetByIdRequest { get; set; }
        public RobotId GetByRobotIdRequest { get; set; }
        public Coordinate GetByCoordinateRequest { get; set; }
        public LocationResponse ExpectedResponse { get; set; }
    }

    public static IEnumerable<object[]> GetTestData(string? name = default, double? x = default, double? y = default, string? id = default, string? robotId = default, double? coordX = default, double? coordY = default)
    {
        var random = new Random();

        name ??= $"Test location {random.Next()}";
        x ??= random.NextDouble();
        y ??= random.NextDouble();
        id ??= Guid.NewGuid().ToString();
        robotId ??= Guid.NewGuid().ToString();
        coordX ??= random.NextDouble();
        coordY ??= random.NextDouble();

        return new[]
        {
            new object[]
            {
                new TestData
                {
                    AddRequest = new AddLocationRequest
                    {
                        Name = name,
                        X = x.Value,
                        Y = y.Value
                    },
                    ExpectedLocation = new Location
                    {
                        Id = id,
                        Name = name,
                        X = x,
                        Y = y
                    },
                    GetByIdRequest = new LocationId
                    {
                        Id = id
                    },
                    GetByRobotIdRequest = new RobotId
                    {
                        Id = robotId
                    },
                    GetByCoordinateRequest = new Coordinate
                    {
                        X = coordX.Value,
                        Y = coordY.Value
                    },
                    ExpectedResponse = new LocationResponse
                    {
                        Id = id,
                        Name = name,
                        X = x.Value,
                        Y = y.Value
                    }
                }
            }
        };
    }


    // Test 1: AddLocation_ShouldReturnInsertedId_WhenThereIsnt_DuplicateDataInDatabase
    [Theory]
    [MemberData(nameof(GetTestData))]
    public void AddLocation_ShouldReturnInsertedId_WhenThereIsnt_DuplicateDataInDatabase(TestData testData)
    {
        // Arrange
        mockLocationDAO.Setup(dao => dao.FindLocations(It.IsAny<Expression<Func<Location, bool>>>())).Returns(new List<Location>());
        mockLocationDAO.Setup(dao => dao.InsertLocation(testData.ExpectedLocation)).Returns(testData.ExpectedLocation.Id);

        // Act
        var response = service.AddLocation(testData.AddRequest, TestServerCallContext.Create()).Result;

        // Assert
        Assert.Equal(testData.ExpectedLocation.Id, response.Id);
        mockLocationDAO.Verify(dao => dao.InsertLocation(testData.ExpectedLocation), Times.Once());
    }

    // Test 2: AddLocation_ShouldReturnExistingId_WhenThereIs_DuplicateDataInDatabase
    [Theory]
    [MemberData(nameof(GetTestData))]
    public void AddLocation_ShouldReturnExistingId_WhenThereIs_DuplicateDataInDatabase(TestData testData)
    {
        // Arrange
        mockLocationDAO.Setup(dao => dao.FindLocations(It.IsAny<Expression<Func<Location, bool>>>())).Returns(new List<Location> { testData.ExpectedLocation });

        // Act
        var response = service.AddLocation(testData.AddRequest, TestServerCallContext.Create()).Result;

        // Assert
        Assert.Equal(testData.ExpectedLocation.Id, response.Id);
        mockLocationDAO.Verify(dao => dao.VerifyExistance(testData.ExpectedLocation), Times.Once());
        mockLocationDAO.Verify(dao => dao.UpdateLocation(testData.ExpectedLocation.Id, testData.ExpectedLocation), Times.Once());
    }

    [Theory]
    [MemberData(nameof(GetTestData), parameters: 3)]
    public async Task GetLocations_ShouldReturnLocationsResponse_InDatabase(TestData testData)
    {
        // Arrange
        var locations = new List<Location>
        {
            testData.ExpectedLocation,
            testData.ExpectedLocation,
            testData.ExpectedLocation
        };
        var expectedResponse = new LocationsResponse
        {
            Locations = { testData.ExpectedResponse, testData.ExpectedResponse, testData.ExpectedResponse }
        };
        mockLocationDAO.Setup(dao => dao.GetLocations()).Returns(locations);
        
        // Act
        var response = await service.GetLocations(new Empty(), TestServerCallContext.Create());

        // Assert
        Assert.Equal(expectedResponse, response);
    }

    [Theory]
    [MemberData(nameof(GetTestData), parameters: 1)]
    public async Task GetLocationById_ShouldReturnLocationResponse_WhenThereIs_DataInDatabase(TestData testData)
    {
        // Arrange
        mockLocationDAO.Setup(dao => dao.FindLocations(It.IsAny<Expression<Func<Location, bool>>>())).Returns(new List<Location> { testData.ExpectedLocation });

        // Act
        var response = await service.GetLocationById(testData.GetByIdRequest, TestServerCallContext.Create());

        // Assert
        Assert.Equal(testData.ExpectedResponse, response);
    }
    
    ///////////////////////////////////////////////////////////////////////////////
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

}
