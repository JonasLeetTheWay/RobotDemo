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
    public LocationDAO(IOptions<MongoDBSettings> settings)
    {

        _client = new MongoClient(settings.Value.ConnectionString);
        _database = _client.GetDatabase(settings.Value.DatabaseName);
        _collection = _database.GetCollection<Location>(settings.Value.CollectionName_Locations);
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
        var filter = Builders<Location>.Filter.Eq("x", location.X) & Builders<Location>.Filter.Eq("y", location.Y);
        var existing = _collection.Find(filter).SingleOrDefault();
        if (existing == null)
        {
            return null;
        }
        else
        {
            return existing;
        }
    }

    public List<Location> FindLocations()
    {
        return _collection.Find(new BsonDocument()).ToList();
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

        var updateFields = new BsonDocument
        {
            // x,y attribute will always be given
            { "x", location.X },
            { "y", location.Y }
        };

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