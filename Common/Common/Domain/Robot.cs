using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Domain;

public class Robot
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("type")]
    public string? Type { get; set; }
    [BsonElement("status")]
    public string? Status { get; set; }

    [BsonElement("curr_location")]
    public Location? CurrentLocation { get; set; }

    [BsonElement("prev_locations_ids")]
    public List<string>? PreviousLocationIds { get; set; }
}
