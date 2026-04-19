using Moq;
using TransSolutions.Domain.Interfaces.Repositories;
using TransSolutions.Domain.Models.BusinessLogic;
using TransSolutions.Infrastructure.Services;
using TransSolutions.Shared.Contracts.Vehicle;
using TransSolutions.Shared.Enums.Vehicle;

namespace TransSolutions.Testing.Services;

public class VehicleServiceTests
{
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly VehicleService _sut;

    public VehicleServiceTests()
    {
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _sut = new VehicleService(_vehicleRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateVehicle_ShouldAddVehicle()
    {
        var request = new CreateVehicleRequest
        {
            Name = "Scania R500",
            RegistrationPlateNumber = "XYZ123",
            VehicleType = VehicleType.Truck
        };

        var response = await _sut.CreateVehicle(request, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, response.Id);
        _vehicleRepositoryMock.Verify(r => r.AddAsync(It.Is<Vehicle>(v => 
            v.Name == request.Name && 
            v.RegistrationPlateNumber == request.RegistrationPlateNumber &&
            v.VehicleType == request.VehicleType &&
            v.IsActive), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateVehicle_ShouldUpdateWhenExistsAndActive()
    { 
        var vehicle = new Vehicle { Id = Guid.NewGuid(), IsActive = true };
        var request = new UpdateVehicleRequest
        {
            Id = vehicle.Id,
            Name = "New Name",
            RegistrationPlateNumber = "NEW123",
            VehicleType = VehicleType.Bus
        };

        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(vehicle.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        await _sut.UpdateVehicle(request, CancellationToken.None);

        Assert.Equal(request.Name, vehicle.Name);
        Assert.Equal(request.RegistrationPlateNumber, vehicle.RegistrationPlateNumber);
        Assert.Equal(request.VehicleType, vehicle.VehicleType);
        _vehicleRepositoryMock.Verify(r => r.UpdateAsync(vehicle, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateVehicle_ShouldThrowWhenNotFound()
    {
        var request = new UpdateVehicleRequest { Id = Guid.NewGuid() };
        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(request.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateVehicle(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetVehicle_ShouldReturnVehicleInfo()
    { 
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            IsActive = true,
            VehicleType = VehicleType.Car,
            RegistrationPlateNumber = "ABC"
        };

        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(vehicle.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var response = await _sut.GetVehicle(new GetVehicleRequest { Id = vehicle.Id }, CancellationToken.None);

        Assert.Equal(vehicle.Id, response.Id);
        Assert.Equal(vehicle.Name, response.Name);
    }

    [Fact]
    public async Task GetVehicles_ShouldReturnPaginatedList()
    {
        var vehicles = new List<Vehicle>
        {
            new Vehicle { Id = Guid.NewGuid(), Name = "A", CreatedAt = DateTime.UtcNow },
            new Vehicle { Id = Guid.NewGuid(), Name = "B", CreatedAt = DateTime.UtcNow.AddMinutes(1) }
        }.AsQueryable().BuildMock();

        _vehicleRepositoryMock.Setup(r => r.GetQueryable())
            .Returns(vehicles);

        var request = new GetVehiclesRequest { PageNumber = 1, PageSize = 10 };

        var response = await _sut.GetVehicles(request, CancellationToken.None);

        Assert.Equal(2, response.TotalCount);
        Assert.Equal(2, response.Vehicles.Count());
    }

    [Fact]
    public async Task DeleteVehicle_ShouldMarkAsInactive()
    {
        var vehicle = new Vehicle { Id = Guid.NewGuid(), IsActive = true };
        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(vehicle.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);


        await _sut.DeleteVehicle(new DeleteVehicleRequest { Id = vehicle.Id }, CancellationToken.None);

        Assert.False(vehicle.IsActive);
        _vehicleRepositoryMock.Verify(r => r.UpdateAsync(vehicle, It.IsAny<CancellationToken>()), Times.Once);
    }
}
