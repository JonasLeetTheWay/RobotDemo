using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Domain;

//Name,X,Y,RobotIds
public class Location
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("x")]
    public double? X { get; set; }

    [BsonElement("y")]
    public double? Y { get; set; }

    [BsonElement("robots")]
    public List<string>? RobotIds { get; set; } = new();

    // override ToString()
    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, X: {X}, Y: {Y}, RobotIds: {string.Join(", ", RobotIds)}";
    }
}
