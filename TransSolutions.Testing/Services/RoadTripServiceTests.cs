using Moq;
using TransSolutions.Domain.Interfaces.Repositories;
using TransSolutions.Domain.Models.BusinessLogic;
using TransSolutions.Infrastructure.Services;
using TransSolutions.Shared.Contracts.RoadTrip;
using TransSolutions.Shared.Enums.Vehicle;
using Xunit;

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
        // Arrange
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
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddHours(1),
            Distance = 100,
            AverageFuelConsumption = 8
        };

        _driverRepositoryMock.Setup(r => r.GetQueryable())
            .Returns(new List<Driver> { driver }.AsQueryable());
        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(vehicle.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act & Assert
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
        var request = new CreateRoadTripRequest { CarId = Guid.NewGuid() };

        _driverRepositoryMock.Setup(r => r.GetQueryable())
            .Returns(new List<Driver>().AsQueryable());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateTrip(request, userId, CancellationToken.None));
    }

    [Fact]
    public async Task CreateTrip_ShouldThrowIfVehicleNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var driver = new Driver { AppUserId = userId.ToString() };
        var request = new CreateRoadTripRequest { CarId = Guid.NewGuid() };

        _driverRepositoryMock.Setup(r => r.GetQueryable())
            .Returns(new List<Driver> { driver }.AsQueryable());
        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(request.CarId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle)null!);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateTrip(request, userId, CancellationToken.None));
    }
}