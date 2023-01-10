using Backend.Infrastructure;
using Backend.Settings;
using Common.Domain;
using Common.LoggerHelper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;

public class LocationDAO : ILocationDAO
{
    private readonly IMongoCollection<Location> _collection;
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly ILogger? _logger;

    public LocationDAO(IOptions<MongoDBSettings> settings, ILogger? logger)
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
        _logger.LogInformation("parameter id: " + location.Id);
        var existing = VerifyExistance(location);
        if (existing == null)
        {
            _collection.InsertOne(location);
            _logger.LogInformation("existing==null, _collection.InsertOne id: " + location.Id);
            return location.Id;
        }
        else
        {
            UpdateLocation(existing.Id, location);
            _logger.LogInformation("existing!=null, existing data id: " + existing.Id);
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
            _logger.LogInformation("Catched error:\n" +
                e.Message +
                "Name: " + location.Name +
                "X: " + location.X +
                "Y: " + location.Y +
                "Id: " + location.Id);
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
        _logger.LogInformation(LoggerHelper.GetMethodData(MethodBase.GetCurrentMethod()) + location);

        if (location.X != double.MinValue && location.Y != double.MinValue)
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
        _logger.LogInformation(LoggerHelper.GetMethodData(MethodBase.GetCurrentMethod()) + $"updated fields: {updateFields}");

        return UpdateLocationBasedOnFields(id, location.Id, updateFields);
    }

    public DeleteResult DeleteLocation(string locationId)
    {
        var filter = Builders<Location>.Filter.Eq("Id", locationId);
        return _collection.DeleteOne(filter);
    }

    public UpdateResult AddRobotToLocation(string locationId, string robotId)
    {
        var filter = Builders<Location>.Filter.Eq("Id", locationId);
        var update = Builders<Location>.Update.AddToSet("RobotIds", robotId);
        return _collection.UpdateOne(filter, update);

    }

    public UpdateResult RemoveRobotFromLocation(string locationId, string robotId)
    {
        var filter = Builders<Location>.Filter.Eq("Id", locationId);
        var update = Builders<Location>.Update.Pull("RobotIds", robotId);

        return _collection.UpdateOne(filter, update);
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

        _logger.LogInformation(LoggerHelper.GetMethodData(MethodBase.GetCurrentMethod()) + $"updateFields: {updateFields}");
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
        _logger.LogInformation(LoggerHelper.GetMethodData(MethodBase.GetCurrentMethod()) + $"updateDoc: {updateDoc}");

        var update = Builders<Location>.Update.Combine(updateDoc);
        var result = _collection.UpdateOne(l => l.Id == id, update);
        _logger.LogInformation(LoggerHelper.GetMethodData(MethodBase.GetCurrentMethod()) + $"result: {result}");

        return result;

    }
}