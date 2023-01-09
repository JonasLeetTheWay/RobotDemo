using Common.Domain;
using Backend.Settings;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Backend.Infrastructure;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit.Abstractions;

namespace Backend.Tests.UnitTests;

public static class TestDataGenerator
{
    public static Location GenerateLocation(string name, double x, double y, string? id = default)
    {
        return new Location
        {
            Id = id ?? ObjectId.GenerateNewId().ToString(),
            Name = name,
            X = x,
            Y = y
        };
    }
}


public class LocationDAOTests
{
    private readonly Mock<ILogger<LocationDAO>> mockLogger = new Mock<ILogger<LocationDAO>>();
    private readonly LocationDAO dao;
    private readonly ITestOutputHelper _logger;

    public LocationDAOTests(ITestOutputHelper logger)
    {
        _logger = logger;
        var settings = Options.Create(new MongoDBSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "testDB",
            CollectionName_Locations = "testLocations"
        });

        dao = new LocationDAO(settings, mockLogger.Object);
        dao.ClearCollection(); // Clear test collection before each test
    }

    
    ////////////////////// InsertLocation ///////////////////////
    
    [Fact]
    public void InsertLocation_ShouldInsertNewLocation_WhenLocationDoesNotExist()
    {
        // Arrange
        var location = TestDataGenerator.GenerateLocation("Test Location", 1.0, 2.0);
        var expectedId = location.Id;

        // Act
        var result = dao.InsertLocation(location);

        // Assert
        Assert.Equal(expectedId, result);
    }
    
    
    
    ////////////////////// VerifyExistance ///////////////////////
    
    [Fact]
    public void VerifyExistance_ShouldReturnNull_WhenLocationDoesNotExist()
    {
        // Arrange
        var location = TestDataGenerator.GenerateLocation("Test Location", 1.0, 2.0);
        // Act
        var result = dao.VerifyExistance(location);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void VerifyExistance_ShouldReturnLocation_WhenLocationExistsWithSameId()
    {
        // Arrange
        var location = TestDataGenerator.GenerateLocation("Test Location", 1.0, 2.0);
        dao.InsertLocation(location);

        // Act
        var result = dao.VerifyExistance(location);

        // Assert
        Assert.Equal(location.Id, result.Id);
    }

    [Fact]
    public void VerifyExistance_ShouldReturnLocation_WhenLocationExistsWithSameCoordinates()
    {
        // Arrange
        var location = TestDataGenerator.GenerateLocation("Test Location", 1.0, 2.0);
        dao.InsertLocation(location);
        var expectedLocation = location;
        // Act
        var result = dao.VerifyExistance(location);

        // Assert
        Assert.Equal(expectedLocation.X, result.X);
        Assert.Equal(expectedLocation.Y, result.Y);
        
    }

    [Fact]
    public void VerifyExistance_ShouldThrowException_WhenLocationExistsWithSameNameAndDifferentCoordinates()
    {
        // Arrange
        var location = TestDataGenerator.GenerateLocation("Test Location", 1.0, 2.0);
        var existingLocation = TestDataGenerator.GenerateLocation(location.Name, 3.0, 4.0);
        dao.InsertLocation(existingLocation);
        _logger.WriteLine(existingLocation.Id);
        _logger.WriteLine(existingLocation.X.ToString());
        _logger.WriteLine(existingLocation.Y.ToString());
        _logger.WriteLine(existingLocation.Name);
        bool exceptionThrown = false;

        // Act
        try
        {
            var res = dao.VerifyExistance(location);
                _logger.WriteLine("hi");
                _logger.WriteLine(res.Id);
                _logger.WriteLine(res.X.ToString());
                _logger.WriteLine(res.Y.ToString());
                _logger.WriteLine(res.Name);
        }
        catch (Exception)
        {
            exceptionThrown = true;
        }

        // Assert
        Assert.True(exceptionThrown);
    }
    
    
    
    
    ////////////////////// GetLocations ///////////////////////
     
    [Fact]
    public void GetLocations_ShouldReturnAllLocations_WhenCalled()
    {
        // Arrange
        var location1 = TestDataGenerator.GenerateLocation("Test Location 1", 1.0, 2.0);
        var location2 = TestDataGenerator.GenerateLocation("Test Location 2", 3.0, 4.0);
        dao.InsertLocation(location1);
        dao.InsertLocation(location2);

        // Act
        var locations = dao.GetLocations();
        foreach (var res in locations)
        {
            _logger.WriteLine("hi");
            _logger.WriteLine(res.Id);
            _logger.WriteLine(res.X.ToString());
            _logger.WriteLine(res.Y.ToString());
            _logger.WriteLine(res.Name);
        }
        // Assert
        Assert.Contains(location1, locations, new LocationComparer());
        Assert.Contains(location2, locations, new LocationComparer());

    }

    [Fact]
    public void FindLocations_ShouldReturnLocation_WhenFilterMatchesSingleLocation()
    {
        // Arrange
        var location = TestDataGenerator.GenerateLocation("Test Location", 1.0, 2.0);
        _logger.WriteLine(location.Id);
        
        dao.InsertLocation(location);
        var filter = Builders<Location>.Filter.Eq("name", location.Name);

        // Act
        var result = dao.FindLocations(filter);
        foreach (var res in result)
        {
            _logger.WriteLine("hi");
            _logger.WriteLine(res.Id);
            _logger.WriteLine(res.X.ToString());
            _logger.WriteLine(res.Y.ToString());
            _logger.WriteLine(res.Name);
        }
        // Assert
        Assert.Single(result);
        Assert.Equal(location.Id, result[0].Id);
        Assert.Equal(location.Name, result[0].Name);
        Assert.Equal(location.X, result[0].X);
        Assert.Equal(location.Y, result[0].Y);
    }

    [Fact]
    public void FindLocations_ShouldReturnLocations_WhenFilterMatchesMultipleLocations_1()
    {
        // Arrange
        var location1 = TestDataGenerator.GenerateLocation("Test Location 1", 1.0, 2.0);
        dao.InsertLocation(location1);
        _logger.WriteLine(location1.Id);
        
        var location2 = TestDataGenerator.GenerateLocation("Test Location 2", 3.0, 4.0);
        dao.InsertLocation(location2);
        _logger.WriteLine(location2.Id);

        var location3 = TestDataGenerator.GenerateLocation("Test Location 3", 5.0, 6.0);
        dao.InsertLocation(location3);
        _logger.WriteLine(location3.Id);
        

        var filter = Builders<Location>.Filter.Gte("x", 1.0) & Builders<Location>.Filter.Lte("x", 5.0);

        // Act
        var result = dao.FindLocations(filter);
        foreach (var res in result)
        {
            _logger.WriteLine("hi");
            _logger.WriteLine(res.Id);
            _logger.WriteLine(res.X.ToString());
            _logger.WriteLine(res.Y.ToString());
            _logger.WriteLine(res.Name);
        }


        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(location1, result, new LocationComparer());
        Assert.Contains(location2, result, new LocationComparer());
        Assert.Contains(location3, result, new LocationComparer());
    }

    public class LocationComparer : IEqualityComparer<Location>
    {
        public bool Equals(Location x, Location y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.Id == y.Id && x.Name == y.Name && x.X == y.X && x.Y == y.Y;
        }

        public int GetHashCode(Location obj)
        {
            return obj.Id.GetHashCode();
        }
    }

    [Fact]
    public void FindLocations_ShouldReturnLocations_WhenFilterMatchesMultipleLocations_2()
    {
        // Arrange
        // location1 & location 2 have same coordinate, therefore only the last inserted document shall persist, but with the initial id!
        // so 
        var location1 = TestDataGenerator.GenerateLocation("Test Location 1", 1.0, 2.0);
        var location2 = TestDataGenerator.GenerateLocation("Test Location 2", 1.0, 2.0);
        var location3 = TestDataGenerator.GenerateLocation("Test Location 3", 3.0, 4.0);
        dao.InsertLocation(location1);
        _logger.WriteLine(location1.Id);
        
        dao.InsertLocation(location2);
        _logger.WriteLine(location2.Id);
        
        dao.InsertLocation(location3);
        _logger.WriteLine(location3.Id);
        
        var expectedLocations = new List<Location> { location2 };
        var filter = Builders<Location>.Filter.Eq("x", 1.0) & Builders<Location>.Filter.Eq("y", 2.0);
        // Act
        var result = dao.FindLocations(filter);
        foreach (var res in result)
        {
            _logger.WriteLine("hi");
            _logger.WriteLine(res.Id);
            _logger.WriteLine(res.X.ToString());
            _logger.WriteLine(res.Y.ToString());
            _logger.WriteLine(res.Name);
        }

        // Assert
        Assert.Equal(expectedLocations, result, new LocationComparer());
    }


    [Fact]
    public void FindLocations_ShouldReturnEmptyList_WhenFilterDoesNotMatchAnyLocations()
    {
        // Arrange
        var location1 = TestDataGenerator.GenerateLocation("Test Location 1", 1.0, 2.0);
        var location2 = TestDataGenerator.GenerateLocation("Test Location 2", 3.0, 4.0);
        dao.InsertLocation(location1);
        dao.InsertLocation(location2);
        var expectedLocations = new List<Location>(); // empty list
        var filter = Builders<Location>.Filter.Eq("x", 2.0) & Builders<Location>.Filter.Eq("y", 3.0);
        // Act
        var result = dao.FindLocations(filter);

        // Assert
        Assert.Equal(expectedLocations, result);
    }

    

    ////////////////////// UpdateLocation ///////////////////////
    
    [Fact]
    public void UpdateLocation_ShouldUpdateLocation_WhenLocationExists()
    {
        // Arrange
        var location = TestDataGenerator.GenerateLocation("Test Location", 1.0, 2.0);
        dao.InsertLocation(location);

        var updatedLocation = TestDataGenerator.GenerateLocation("Updated Test Location", 2.0, 3.0, location.Id);

        // Act
        dao.UpdateLocation(location.Id, updatedLocation);

        // Assert
        var result = dao.GetLocations().FirstOrDefault(l => l.Id == location.Id);
        Assert.Equal(updatedLocation.Name, result.Name);
        Assert.Equal(updatedLocation.X, result.X);
        Assert.Equal(updatedLocation.Y, result.Y);
    }

    [Fact]
    public void UpdateLocation_ShouldThrowException_WhenLocationDoesNotExist()
    {
        // Arrange
        var location = TestDataGenerator.GenerateLocation("Test Location", 1.0, 2.0);
        var invalidId = ObjectId.GenerateNewId().ToString();

        // Act and Assert
        Assert.Throws<Exception>(() => dao.UpdateLocation(invalidId, location));
    }
    

    
    ////////////////////// DeleteLocation ///////////////////////
    
    
    [Fact]
    public void DeleteLocation_ShouldDeleteLocation_WhenLocationExists()
    {
        // Arrange
        var location = TestDataGenerator.GenerateLocation("Test Location", 1.0, 2.0);
        dao.InsertLocation(location);

        // Act
        dao.DeleteLocation(location.Id);

        // Assert
        var locations = dao.GetLocations();
        Assert.DoesNotContain(location, locations);
    }

    

}
