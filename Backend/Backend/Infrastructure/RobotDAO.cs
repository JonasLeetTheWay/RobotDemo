using Common.Domain;
using Backend.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Backend.Infrastructure;

public class RobotDAO
{
    private readonly IMongoCollection<Robot> _collection;
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;

    private readonly LocationDAO _locationDAO;

    public RobotDAO(IOptions<MongoDBSettings> settings, LocationDAO locationDAO)
    {

        _client = new MongoClient(settings.Value.ConnectionString);
        _database = _client.GetDatabase(settings.Value.DatabaseName);
        _collection = _database.GetCollection<Robot>(settings.Value.CollectionName_Robots);
        _locationDAO = locationDAO;
    }

    public Robot? VerifyExistance(Robot robot)
    {
        // check if there's already data that matches new data's name
        var filter = Builders<Robot>.Filter.Eq("name", robot.Name);
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

    public string InsertRobot(Robot robot)
    {
        var existing = VerifyExistance(robot);
        if (existing == null)
        {
            _collection.InsertOne(robot);
            return robot.Id;
        }
        else
        {
            UpdateRobot(existing.Id, robot);
            return existing.Id;
        }
    }

    public List<Robot> FindRobots(Expression<Func<Robot, bool>> filter)
    {
        return _collection.Find(filter).ToList();
    }
    public List<Robot> FindRobots()
    {
        return _collection.Find(new BsonDocument()).ToList();
    }

    public UpdateResult UpdateRobot(string id, Robot robot)
    {
        return _collection.UpdateOne(
            new BsonDocument("_id", id),
            new BsonDocument("$set", robot.ToBsonDocument()));
    }

    public DeleteResult DeleteRobot(string id)
    {
        return _collection.DeleteOne(new BsonDocument("_id", id));
    }

    // assume everytime update must exist CurrentLocation
    public UpdateResult UpdateRobotLocation(string robotId, Location location)
    {
        var existing = _locationDAO.VerifyExistance(location);
        string currLocationId;
        Location currLocation;
        if (existing == null)
        {
            currLocationId = location.Id;
            currLocation = location;
        }
        else
        {
            currLocationId = existing.Id;
            currLocation = existing;
        }

        // check every attribute of location whether they are null or not
        // if they are null, then ignore it
        // else, add them to updateFields
        // then update the document with UpdateRobotBasedOnFields function

        var updateFields = new BsonDocument("curr_location", currLocation.ToBsonDocument())
            .Add("$push", new BsonDocument("prev_locations_ids", currLocationId));

        return UpdateRobotBasedOnFields(robotId, updateFields);
    }

    
    public UpdateResult UpdateRobotStatus(string robotId, string status)
    {
        return _collection.UpdateOne(
            new BsonDocument("_id", robotId),
            new BsonDocument("$set", new BsonDocument("status", status)));
    }



    // RemoveFields is default to an empty list
    private UpdateResult UpdateRobotBasedOnFields(string id, BsonDocument updateFields, List<string>? removeFields = default)
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

        var filter = Builders<Robot>.Filter.Eq("_id", id);
        var update = Builders<Robot>.Update.Combine(updateDoc);
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
