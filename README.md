# RobotDemo: Microservices for AMR
## Summary
This project is a system that allows users to track and manage a fleet of autonomous mobile robots (AMRs) in a warehouse or factory setting. It is built using microservices architecture, which allows for greater flexibility, scalability, and maintainability compared to a monolithic architecture. The system consists of a frontend client application and a backend server that communicate through gRPC. The frontend allows users to view the current locations and identities of all AMRs, as well as perform CRUD operations. The backend stores and retrieves AMR data from a MongoDB database, and backend shows real-time location updates using gRPC streaming from the backend.

## Definitions of each files:
- `RobotService.cs`: A gRPC service that implements the methods defined in the robot.proto file for managing robots.
- `LocationService.cs`: A gRPC service that implements the methods defined in the location.proto file for managing locations.
- `LocationDAO.cs`: A data access object (DAO) for interacting with the locations collection in a database.
- `RobotDAO.cs`: A data access object (DAO) for interacting with the robots collection in a database.
- `robot.proto`: A Protocol Buffers file that defines the gRPC service for managing robots.
- `location.proto`: A Protocol Buffers file that defines the gRPC service for managing locations.

## Concepts utilized:
- `Microservices`: a collection of small, independent services that communicate with APIs, enabling flexibility, scalability, and maintainability
- `gRPC`: a high-performance, open-source RPC framework that enables efficient communication between services
- `Protobuf`: a language- and platform-neutral data serialization format used to define gRPC service interfaces
- `MVC Frontend`: a common .NET framework for developing high-performant web application
- `DAO`: a data access object (DAO) for interacting with a specific collection in a database.
- `Dependency injection`: a software design pattern that allows for the separation of concerns and the management of dependencies between objects
- `MongoDB`: a NoSQL database that stores data in flexible, JSON-like documents


## Incoming:
- Testing (mock testing, doubles testing, microservices contract testing)

## Technology:
- ASP.NET Core
- .NET 6
- gRPC
- C#
- JavaScript
- HTML/CSS
- MongoDB