using Common.Protos.Location;
using Common.Protos.Robot;

using Frontend.Models;
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
            var robotResponse = await _robotClient.GetRobotByIdAsync(new Common.Protos.Robot.RobotId { Id = robotId });
            var robot = robotResponse.Id;

            // Get the list of previous location ids for the robot
            var previousLocationIds = robotResponse.PreviousLocationIds;

            // Get the locations for each previous location id
            var previousLocations = new List<LocationObjFull>();
            foreach (var locationId in previousLocationIds)
            {
                var locationResponse = await _locationClient.GetLocationByIdAsync(new Common.Protos.Location.LocationId { Id = locationId });
                var localobjfull = new LocationObjFull
                {
                    Id = locationResponse.Id,
                    Name = locationResponse.Name,
                    X = locationResponse.X,
                    Y = locationResponse.Y,
                };
                localobjfull.RobotIds.AddRange(locationResponse.RobotIds);
                previousLocations.Add(localobjfull);
            }

            // Pass the list of previous locations to the view
            return View(previousLocations);
        }
    }

}