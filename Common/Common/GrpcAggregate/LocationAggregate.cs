using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.GrpcAggregate;

public class LocationAggregate
{
    public static AddLocationObj CreateAddLocationObj(Location locationData)
    {
        return new AddLocationObj
        {
            Name = locationData.Name,
            X = locationData.X,
            Y = locationData.Y
        };
    }

    public static LocationIdAndUpdate CreateLocationIdAndUpdate(string id, UpdateLocationObj update)
    {
        return new LocationIdAndUpdate
        {
            Id = id,
            Update = update
        };
    }

    public static UpdateLocationObj CreateUpdateLocationObj(string name, IEnumerable<string> robotIds)
    {
        return new UpdateLocationObj
        {
            Name = name,
            RobotIds = { robotIds }
        };
    }

    public static LocationIdAndRobotId CreateLocationIdAndRobotId(string locationId, string robotId)
    {
        return new LocationIdAndRobotId
        {
            LocationId = locationId,
            RobotId = robotId
        };
    }
}
