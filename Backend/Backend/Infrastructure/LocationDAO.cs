using Common.Domain;
using Backend.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Backend.Infrastructure;

public class LocationDAO
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
    // id, name, x, y
    public string InsertLocation(Location location)
    {
        var existing = VerifyExistance(location);
        if (existing == null){
            _collection.InsertOne(location);
            return location.Id;
        }
        else
        {
            UpdateLocation(existing.Id,location);
            return existing.Id;
        }
    }
    public Location? VerifyExistance(Location location)
    {
        // check if there's already data that matches new data's x,y
        var idFilter = Builders<Location>.Filter.Eq("_id", location.Id);
        var coordinateFilter = Builders<Location>.Filter.Eq("x", location.X) & Builders<Location>.Filter.Eq("y", location.Y);
        var nameFilter = Builders<Location>.Filter.Eq("name", location.Name);

        Location? existing;

        // check if there's already data that matches new data's id
        try
        {
            existing = _collection.Find(idFilter).SingleOrDefault();
            if (existing == null)
            {
                // the name,coordinates are unique, so we must perform a check for both
                existing = _collection.Find(nameFilter).SingleOrDefault();

                if (existing == null)
                {
                    existing = _collection.Find(coordinateFilter).SingleOrDefault();
                    if (existing == null)
                    {
                        return null;
                    }
                    return existing;
                }
                else
                {
                    var check = _collection.Find(coordinateFilter).SingleOrDefault();
                    if (check != null)
                    {
                        try
                        {
                            if (check.Name == existing.Name)
                            {
                                throw new Exception("LocationDAO: VerifyExistance: Location name is not unique");
                            }
                            else if (check.X == existing.X && check.Y == existing.Y)
                            {
                                throw new Exception("LocationDAO: VerifyExistance: Location coordinates are not unique");
                            }
                            return existing;
                        }
                        catch (Exception e)
                        {
                            _logger.LogDebug(e.Message + "\n" +
                                "Name: " + location.Name + "\n" +
                                "X: " + location.X + "\n" +
                                "Y: " + location.Y + "\n" +
                                "Id: " + location.Id + "\n" +
                                "Existing Id: " + existing.Id + "\n" +
                                "Check Id: " + check.Id + "\n");
                            return existing;
                        }
                    }
                    return existing;
                }
            }
            else
            {
                throw new Exception("LocationDAO: VerifyExistance: Data with same id already exist");
                return existing;
                
            }
        }
        catch (Exception e)
        {
            _logger.LogDebug(e.Message + "\n" +
                "Name: " + location.Name + "\n" +
                "X: " + location.X + "\n" +
                "Y: " + location.Y + "\n" +
                "Id: " + location.Id + "\n" +
                "Existing Id: " + existing.Id + "\n" +
                "Check Id: " + check.Id + "\n");
            return existing;
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
        
        if (location.X.HasValue && location.Y.HasValue)
        {
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
        return UpdateLocationBasedOnFields(id, updateFields);
    }

    public DeleteResult DeleteLocation(string id)
    {
        return _collection.DeleteOne(new BsonDocument("_id", id));
    }

    public UpdateResult AddRobotToLocation(string locationId, string robotId)
    {
        return _collection.UpdateOne(
            new BsonDocument("_id", locationId),
        new BsonDocument("$addToSet", new BsonDocument("robots", robotId)));
    }

    public UpdateResult RemoveRobotFromLocation(string locationId, string robotId)
    {
        return _collection.UpdateOne(
        new BsonDocument("_id", locationId),
            new BsonDocument("$pull", new BsonDocument("robots", robotId)));
    }

    // RemoveFields is default to an empty list
    private UpdateResult UpdateLocationBasedOnFields(string id, BsonDocument updateFields, List<string>? removeFields = default)
    {
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

        var filter = Builders<Location>.Filter.Eq("_id", id);
        var update = Builders<Location>.Update.Combine(updateDoc);
        var result = _collection.UpdateOne(filter, update);
        return result;

        /*
        await UpdateFields(
           ObjectId.Parse("5f9d9d5e5c5b0a84b8c2cee2"),
       new BsonDocument { { "field1", "new value" }, { "field2", "new value" } }
        );
        */

    }





}