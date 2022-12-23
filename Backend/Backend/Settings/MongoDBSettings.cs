namespace Backend.Settings;

public class MongoDBSettings
{
    public string ConnectionString { get; init; } = null!;

    public string DatabaseName { get; init; } = null!;

    public string CollectionName_Locations { get; init; } = null!;
    public string CollectionName_Robots { get; init; } = null!;
}