using Moq;
using TransSolutions.Domain.Interfaces.Repositories;
using TransSolutions.Domain.Models.BusinessLogic;
using TransSolutions.Infrastructure.Services;
using TransSolutions.Shared.Contracts.RoadTrip;
using TransSolutions.Shared.Enums.Vehicle;
using Xunit;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace TransSolutions.Testing.Services;

public class RoadTripServiceTests
{
    private readonly Mock<IDriverRepository> _driverRepositoryMock;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly Mock<IRoadTripRepository> _tripRepositoryMock;
    private readonly RoadTripService _sut;

    public RoadTripServiceTests()
    {
        _driverRepositoryMock = new Mock<IDriverRepository>();
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _tripRepositoryMock = new Mock<IRoadTripRepository>();
        _sut = new RoadTripService(
            _driverRepositoryMock.Object,
            _vehicleRepositoryMock.Object,
            _tripRepositoryMock.Object);
    }

    [Theory]
    [InlineData(VehicleType.Car, DrivingLicenseCategory.B, true)]
    [InlineData(VehicleType.Car, DrivingLicenseCategory.C, true)]
    [InlineData(VehicleType.Truck, DrivingLicenseCategory.C, true)]
    [InlineData(VehicleType.Motorcycle, DrivingLicenseCategory.A, true)]
    [InlineData(VehicleType.Bus, DrivingLicenseCategory.D, true)]
    [InlineData(VehicleType.Car, DrivingLicenseCategory.A, false)]
    [InlineData(VehicleType.Truck, DrivingLicenseCategory.B, false)]
    [InlineData(VehicleType.Motorcycle, DrivingLicenseCategory.C, false)]
    [InlineData(VehicleType.Bus, DrivingLicenseCategory.B, false)]
    public async Task CreateTrip_ShouldValidateDrivingLicense(VehicleType vehicleType, DrivingLicenseCategory licenseCategory, bool expectedSuccess)
    {
        var userId = Guid.NewGuid();
        var driver = new Driver
        {
            Id = Guid.NewGuid(),
            AppUserId = userId.ToString(),
            DrivingLicenseCategories = new List<DrivingLicenseCategory> { licenseCategory }
        };
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            VehicleType = vehicleType
        };
        var request = new CreateRoadTripRequest
        {
            CarId = vehicle.Id,
            DeviceId = Guid.NewGuid(),
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddHours(1),
            Distance = 100,
            AverageFuelConsumption = 8
        };

        _driverRepositoryMock.Setup(r => r.GetQueryable())
            .Returns(new List<Driver> { driver }.AsQueryable().BuildMock());
        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(vehicle.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);
        
        if (expectedSuccess)
        {
            var response = await _sut.CreateTrip(request, userId, CancellationToken.None);
            
            Assert.NotNull(response);
            _tripRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RoadTrip>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        else
        {
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.CreateTrip(request, userId, CancellationToken.None));
            Assert.Equal("Invalid driving license category", exception.Message);
        }
    }

    [Fact]
    public async Task CreateTrip_ShouldThrowIfDriverNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateRoadTripRequest { CarId = Guid.NewGuid(), DeviceId = Guid.NewGuid() };

        _driverRepositoryMock.Setup(r => r.GetQueryable())
            .Returns(new List<Driver>().AsQueryable().BuildMock());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateTrip(request, userId, CancellationToken.None));
    }

    [Fact]
    public async Task CreateTrip_ShouldThrowIfVehicleNotFound()
    {
        var userId = Guid.NewGuid();
        var driver = new Driver { AppUserId = userId.ToString() };
        var request = new CreateRoadTripRequest { CarId = Guid.NewGuid(), DeviceId = Guid.NewGuid() };

        _driverRepositoryMock.Setup(r => r.GetQueryable())
            .Returns(new List<Driver> { driver }.AsQueryable().BuildMock());
        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(request.CarId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle)null!);

        
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateTrip(request, userId, CancellationToken.None));
    }

    [Fact]
    public async Task GetTrip_ShouldReturnDeviceId()
    {
        var trip = new RoadTrip
        {
            Id = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(2),
            Distance = 150,
            AverageFuelConsumption = 7.5f
        };
        _tripRepositoryMock.Setup(r => r.GetByIdAsync(trip.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        var response = await _sut.GetTrip(new GetRoadTripRequest { Id = trip.Id }, CancellationToken.None);

        Assert.Equal(trip.DriverId, response.DriverId);
        Assert.Equal(trip.VehicleId, response.VehicleId);
        Assert.Equal(trip.DeviceId, response.DeviceId);
    }

    [Fact]
    public async Task GetTrips_ShouldReturnDeviceId()
    {
        var trip = new RoadTrip
        {
            Id = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(2),
            Distance = 150,
            AverageFuelConsumption = 7.5f
        };
        
        var queryable = new List<RoadTrip> { trip }.AsQueryable().BuildMock();

        _tripRepositoryMock.Setup(r => r.GetQueryable(It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryable);

        var response = await _sut.GetTrips(new GetRoadTripsRequest(), CancellationToken.None);

        var tripResponse = response.RoadTrips.First();
        Assert.Equal(trip.DeviceId, tripResponse.DeviceId);
    }

    [Fact]
    public async Task GetTripsByDeviceId_ShouldReturnCorrectTrips()
    {
        var deviceId = Guid.NewGuid();
        var trip1 = new RoadTrip { DeviceId = deviceId, StartTime = DateTime.Now };
        var trip2 = new RoadTrip { DeviceId = Guid.NewGuid(), StartTime = DateTime.Now };
        
        var queryable = new List<RoadTrip> { trip1, trip2 }.AsQueryable().BuildMock();

        _tripRepositoryMock.Setup(r => r.GetQueryable(It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryable);

        var response = await _sut.GetTripsByDeviceId(new GetRoadTripsByDeviceIdRequest { DeviceId = deviceId }, CancellationToken.None);

        Assert.Single(response.RoadTrips);
        Assert.Equal(deviceId, response.RoadTrips.First().DeviceId);
    }
}
