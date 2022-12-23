using Backend.Domain;
using Backend.Infrastructure;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
//using Backend.Protos;

using MongoDB.Bson;

namespace Backend.Services;

//public class RobotService : RobotProto.RobotProtoBase
//{
//    private readonly RobotDAO robotDAO;
//    private readonly _locationDAO locationDAO;

//    public RobotService(RobotDAO robotDAO, _locationDAO locationDAO)
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
