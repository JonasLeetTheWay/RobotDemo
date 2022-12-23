using Backend.Protos;
using Frontend.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class RobotController : Controller
    {
        private readonly RobotProto.RobotProtoClient _robotClient;
        private readonly LocationProto.LocationProtoClient _locationClient;

        public RobotController(RobotProto.RobotProtoClient robotClient, LocationProto.LocationProtoClient locationClient)
        {
            _robotClient = robotClient;
            _locationClient = locationClient;
        }

        public async Task<IActionResult> Index(string robotId)
        {
            // Get the robot by id
            var robotResponse = await _robotClient.GetRobotAsync(new StringValue { Value = robotId });
            var robot = robotResponse.Robot;

            // Get the list of previous location ids for the robot
            var previousLocationIds = robot.PreviousLocationIds;

            // Get the locations for each previous location id
            var previousLocations = new List<LocationObjFull>();
            foreach (var locationId in previousLocationIds)
            {
                var locationResponse = await _locationClient.GetLocationByIdAsync(new StringValue { Value = locationId });
                var location = locationResponse.Location;
                previousLocations.Add(location);
            }

            // Pass the list of previous locations to the view
            return View(previousLocations);
        }
    }

}