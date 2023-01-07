using Common.Domain;
using Backend.Infrastructure;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
//using Common.Protos;

using MongoDB.Bson;
using Common.Protos;
using Google.Protobuf.Collections;
using Common.Protos;

namespace Backend.Services;

// double.MinValue meaning the valuehasnt been assigned

//public class RobotService : RobotProto.RobotProtoBase
//{
//    private readonly RobotDAO robotDAO;
//    private readonly LocationDAO locationDAO;

//    ////////////////////////////////////// inner methods to avoid repetitive work //////////////////////////////////////
//    private LocationObjFull MakeLocationObjFull(string id, double x = double.MinValue, double y = double.MinValue, string name = "null", IEnumerable<string>? robotIds = default)
//    {
//        // check matches with same id or name
//        var existing = locationDAO.VerifyExistance(new Location { Id = id, Name = name });
//        var response = new LocationObjFull();

//        if (existing != null)
//        {
//            response.Id = id;
//            response.Name = existing.Name ?? "null";
//            response.X = existing.X ?? double.MinValue;
//            response.Y = existing.Y ?? double.MinValue;
//            if (existing.RobotIds != null)
//            {
//                response.RobotIds.AddRange(existing.RobotIds);
//            }

//        }
//        else
//        {
//            response.Id = id;
//            response.Name = name;
//            response.X = x;
//            response.Y = y;
//        }

//        response.RobotIds.AddRange(robotIds);

//        return response;
//    }

//    private RobotResponse MakeRobotResponse(string id, string name, string type, string status, LocationObjFull obj, IEnumerable<string>? locationIds = default)
//    {
//        // check matches with same id or name
//        var existing = robotDAO.VerifyExistance(new Robot { Id = id, Name = name });
//        var response = new RobotResponse();

//        if (existing != null)
//        {
//            response.Name = existing.Name ?? "null";
//            LocationObjFull locationObjFull = new LocationObjFull();
//            response.CurrentLocation = existing.CurrentLocation ?? locationObjFull;
//        }
//        else
//        {
//            response.Name = name ?? "null";
//            response.CurrentLocation = locationId ?? "null";
//        }
            
//        response.PreviousLocationIds.AddRange(locationIds);


//        return response;
        

//        response.Name = name;
//        response.Type = type;
//        response.Status = status;
//        response.CurrentLocation = obj;
//        return response;
//    }
    

//    private static UpdateRobotRequest MakeUpdateRobotRequest(string id, LocationObjFull? newLocation = default)
//    {
//        var response = new UpdateRobotRequest();
//        response.Id = id;
//        response.NewLocation = newLocation;
//        return response;
//    }
//    private static Location MakeLocation(double? x, double? y, string? name, IEnumerable<string>? robotIds = default)
//    {
//        if (robotIds?.ToList().Count == null || robotIds == null)
//        {
//            return new Location
//            {
//                Name = name ?? "null",
//                X = x ?? double.MinValue,
//                Y = y ?? double.MinValue,
//            };
//        }
//        return new Location
//        {
//            Name = name ?? "null",
//            X = x ?? double.MinValue,
//            Y = y ?? double.MinValue,
//            RobotIds = robotIds.ToList() // because Google.Protobuf.Collections.RepeatedField is not a RECOGNIZED LIST
//        };
//    }
//    private IEnumerable<string> makeFakeRobotIdsArray()
//    {
//        return new List<string> { "fake1", "fake2" };
//    }

//    ////////////////////////////////////// inner methods to avoid repetitive work //////////////////////////////////////

//    public RobotService(RobotDAO robotDAO, LocationDAO locationDAO)
//    {
//        this.robotDAO = robotDAO;
//        this.locationDAO = locationDAO;
//    }

//    // Implement the AddRobot method
//    public override Task<Robot> AddRobot(Robot request, ServerCallContext context)
//    {
//        // Validate the request
//        if (request == null || request.CurrentLocation == null)
//        {
//            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
//        }

//        // Add the robot to the database
//        string robotId = robotDAO.InsertRobot(request);

//        // Add the robot to the location
//        locationDAO.AddRobotToLocation(request.CurrentLocation, robotId);

//        // Return the added robot in the response
//        return Task.FromResult(request);
//    }

//    // Implement the GetRobot method
//    public override Task<Robot> GetRobot(StringValue request, ServerCallContext context)
//    {
//        // Validate the request
//        if (string.IsNullOrEmpty(request.Value))
//        {
//            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
//        }
//        // Find the robot in the database
//        var robot = robotDAO.FindRobots().Find(r => r.Id == request.Value);
//        if (robot == null)
//        {
//            throw new RpcException(new Status(StatusCode.NotFound, "Robot not found"));
//        }

//        // Return the found robot in the response
//        return Task.FromResult(robot);
//    }

//    // Implement the UpdateRobot method
//    public override Task<Robot> UpdateRobot(Robot request, ServerCallContext context)
//    {
//        // Validate the request
//        if (request == null || request.CurrentLocation == null)
//        {
//            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
//        }

//        // Find the robot in the database
//        var robot = robotDAO.FindRobots().Find(r => r.Id == request.Id);
//        if (robot == null)
//        {
//            throw new RpcException(new Status(StatusCode.NotFound, "Robot not found"));
//        }

//        // Update the robot in the database
//        robotDAO.UpdateRobot(request.Id, request);

//        // Update the location of the robot
//        robotDAO.UpdateRobotLocation(request.Id, request.CurrentLocation);

//        // Return the updated robot in the response
//        return Task.FromResult(request);
//    }

//    // Implement the DeleteRobot method
//    public override Task<Empty> DeleteRobot(StringValue request, ServerCallContext context)
//    {
//        // Validate the request
//        if (string.IsNullOrEmpty(request.Value))
//        {
//            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request"));
//        }

//        // Find the robot in the database
//        var robot = robotDAO.FindRobots().Find(r => r.Id == request.Value);
//        if (robot == null)
//        {
//            throw new RpcException(new Status(StatusCode.NotFound, "Robot not found"));
//        }

//        // Delete the robot from the database
//        robotDAO.DeleteRobot(request.Value);
//        locationDAO.RemoveRobotFromLocation(robot.CurrentLocation, robot.Id);

//        // Return an empty response
//        return Task.FromResult(new Empty());
//    }
//}
