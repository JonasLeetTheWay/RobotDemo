using Common.Domain;
using Backend.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Backend.Infrastructure;

public class LocationDAO : ILocationDAO
{
    private readonly IMongoCollection<Location> _collection;
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly ILogger _logger;

    public LocationDAO(IOptions<MongoDBSettings> settings, ILogger logger)
    {

        _client = new MongoClient(settings.Value.ConnectionString);
        _database = _client.GetDatabase(settings.Value.DatabaseName);
        _collection = _database.GetCollection<Location>(settings.Value.CollectionName_Locations);
        _logger = logger;
    }

    public void ClearCollection()
    {
        _collection.DeleteMany(Builders<Location>.Filter.Empty);
    }
    // id, name, x, y
    public string InsertLocation(Location location)
    {
        Console.WriteLine("parameter id" + location.Id);
        var existing = VerifyExistance(location);
        if (existing == null)
        {
            _collection.InsertOne(location);
            Console.WriteLine("existing==null, _collection.InsertOne id: " + location.Id);
            return location.Id;
        }
        else
        {
            UpdateLocation(existing.Id, location);
            Console.WriteLine("existing!=null, existing data id: " + existing.Id);
            return existing.Id;
        }
    }



    public Location? VerifyExistance(Location location)
    {
        // check if there's already data that matches new data's x,y

        var coordinateFilter = Builders<Location>.Filter.Eq("x", location.X) & Builders<Location>.Filter.Eq("y", location.Y);
        var nameFilter = Builders<Location>.Filter.Eq("name", location.Name);

        var e1 = _collection.Find(l => l.Id == location.Id).FirstOrDefault();
        var e2 = _collection.Find(coordinateFilter).FirstOrDefault();
        var e3 = _collection.Find(nameFilter).FirstOrDefault();

        // check if there's already data that matches new data's id
        try
        {

            if (e1 == null)
            {
                if (e2 == null)
                {
                    if (e3 == null)
                    {
                        return null;
                    }
                    if (location.Name == e3.Name && (location.X != e3.X || location.Y != e3.Y))
                    {
                        throw new Exception("LocationDAO: VerifyExistance: Location coordinates are not unique");
                    }
                    return e3;
                }
                else
                {
                    if (e3 != null)
                    {
                        if (e2.Name != e3.Name)
                        {
                            throw new Exception("LocationDAO: VerifyExistance: Location name is not unique");
                        }
                        return e2;
                    }
                    return e2;
                }
            }
            else
            {
                throw new Exception("LocationDAO: VerifyExistance: Data with same id already exist");
            }
        }
        catch (Exception e)
        {
            _logger.LogDebug(e.Message + "\n" +
                "Name: " + location.Name + "\n" +
                "X: " + location.X + "\n" +
                "Y: " + location.Y + "\n" +
                "Id: " + location.Id + "\n");
            return e1;
        }
    }


    public List<Location> GetLocations()
    {
        return _collection.Find(_ => true).ToList();
    }
    public List<Location> FindLocations(FilterDefinition<Location> coordinateFilter)
    {
        return _collection.Find(coordinateFilter).ToList();
    }
    public List<Location> FindLocations(Expression<Func<Location, bool>> filter)
    {
        return _collection.Find(filter).ToList();
    }
    public UpdateResult UpdateLocation(string id, Location location)
    {
        // check every attribute of location whether they are null or not
        // if they are null, then ignore it
        // else, add them to updateFields
        // then update the document with UpdateLocationBasedOnFields function

        var updateFields = new BsonDocument();
        Console.WriteLine($"{MethodBase.GetCurrentMethod().DeclaringType.Name} {MethodBase.GetCurrentMethod().Name} - ", location);
        Console.WriteLine($"{MethodBase.GetCurrentMethod().DeclaringType.Name} {MethodBase.GetCurrentMethod().Name}, x,y expanded - ", location.Id, location.X, location.Y, location.Name, location.RobotIds);

        if (location.X != double.MinValue && location.Y != double.MinValue)
        {
            Console.WriteLine($"{MethodBase.GetCurrentMethod().DeclaringType.Name} {MethodBase.GetCurrentMethod().Name}, x,y not uninitialized - ", location);
            updateFields.Add("x", location.X);
            updateFields.Add("y", location.Y);
        }

        if (location.Name != null)
        {
            updateFields.Add("name", location.Name);
        }

        if (location.RobotIds != null)
        {
            // note that this is "$set", not "$addToSet" nor "$push"
            // hence it will replace the whole array
            // so be careful to Add RobotIds when using this function
            updateFields.Add("robots", new BsonArray(location.RobotIds));
        }
        Console.WriteLine($"{MethodBase.GetCurrentMethod().DeclaringType.Name} {MethodBase.GetCurrentMethod().Name} - {updateFields}");

        return UpdateLocationBasedOnFields(id, location.Id, updateFields);
    }

    public DeleteResult DeleteLocation(string id)
    {
        return _collection.DeleteOne(new BsonDocument("Id", id));
    }

    public UpdateResult AddRobotToLocation(string locationId, string robotId)
    {
        return _collection.UpdateOne(
            new BsonDocument("Id", locationId),
        new BsonDocument("$addToSet", new BsonDocument("robots", robotId)));
    }

    public UpdateResult RemoveRobotFromLocation(string locationId, string robotId)
    {
        return _collection.UpdateOne(
        new BsonDocument("Id", locationId),
            new BsonDocument("$pull", new BsonDocument("robots", robotId)));
    }

    // RemoveFields is default to an empty list
    private UpdateResult UpdateLocationBasedOnFields(string id, string id_new, BsonDocument updateFields, List<string>? removeFields = default)
    {
        // Find the document with the matching id
        var doc = _collection.Find(l => l.Id == id).FirstOrDefault();
        if (doc == null)
        {
            throw new Exception("No document with matching id found");
        }

        Console.WriteLine($"{MethodBase.GetCurrentMethod().DeclaringType.Name} {MethodBase.GetCurrentMethod().Name} - updateFields: {updateFields}");
        var updateDoc = new BsonDocument();

        if (updateFields != null)
        {
            updateDoc.Add("$set", updateFields);
        }
        if (removeFields != null)
        {
            var unsetFields = new BsonDocument();
            removeFields.ForEach(field => unsetFields.Add(field, 1));
            updateDoc.Add("$unset", unsetFields);
        }
        Console.WriteLine($"{MethodBase.GetCurrentMethod().DeclaringType.Name} {MethodBase.GetCurrentMethod().Name} - updateDoc: {updateDoc}");

        var update = Builders<Location>.Update.Combine(updateDoc);
        var result = _collection.UpdateOne(l => l.Id == id, update);
        Console.WriteLine($"{MethodBase.GetCurrentMethod().DeclaringType.Name} {MethodBase.GetCurrentMethod().Name} - result: {result}");

        return result;

    }
}