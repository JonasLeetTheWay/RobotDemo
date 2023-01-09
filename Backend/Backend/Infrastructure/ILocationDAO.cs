using Common.Domain;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Backend.Infrastructure
{
    public interface ILocationDAO
    {
        UpdateResult AddRobotToLocation(string locationId, string robotId);
        void ClearCollection();
        DeleteResult DeleteLocation(string id);
        List<Location> FindLocations(Expression<Func<Location, bool>> filter);
        List<Location> FindLocations(FilterDefinition<Location> coordinateFilter);
        List<Location> GetLocations();
        string InsertLocation(Location location);
        UpdateResult RemoveRobotFromLocation(string locationId, string robotId);
        UpdateResult UpdateLocation(string id, Location location);
        Location? VerifyExistance(Location location);
    }
}