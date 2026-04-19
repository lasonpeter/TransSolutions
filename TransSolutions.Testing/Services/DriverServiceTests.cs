using Moq;
using TransSolutions.Domain.Interfaces.Repositories;
using TransSolutions.Domain.Models.BusinessLogic;
using TransSolutions.Shared.Contracts.Driver;
using TransSolutions.Shared.Enums.Vehicle;
using Xunit;
using TransSolutions.Domain.Models.Auth;

namespace TransSolutions.Testing.Services;

public class DriverServiceTests
{
    private readonly Mock<IDriverRepository> _driverRepositoryMock;
    private readonly DriverService _sut;

    public DriverServiceTests()
    {
        _driverRepositoryMock = new Mock<IDriverRepository>();
        _sut = new DriverService(_driverRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateDriver_ShouldAddDriver()
    {
        // Arrange
        var request = new CreateDriverRequest(
            Guid.NewGuid(),
            new List<DrivingLicenseCategory> { DrivingLicenseCategory.B }
        );

        // Act
        var response = await _sut.CreateDriver(request, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, response.Id);
        _driverRepositoryMock.Verify(r => r.AddAsync(It.Is<Driver>(d => 
            d.AppUserId == request.UserId.ToString() && 
            d.DrivingLicenseCategories.SequenceEqual(request.DrivingLicenseCategories)), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDriver_ShouldUpdateWhenExists()
    {
        // Arrange
        var driver = new Driver { Id = Guid.NewGuid(), DrivingLicenseCategories = new List<DrivingLicenseCategory>() };
        var request = new UpdateDriverRequest(
            driver.Id,
            new List<DrivingLicenseCategory> { DrivingLicenseCategory.C }
        );

        _driverRepositoryMock.Setup(r => r.GetByIdAsync(driver.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);

        // Act
        await _sut.UpdateDriver(request, CancellationToken.None);

        // Assert
        Assert.Equal(request.DrivingLicenseCategories, driver.DrivingLicenseCategories);
        _driverRepositoryMock.Verify(r => r.UpdateAsync(driver, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDriver_ShouldThrowWhenNotFound()
    {
        // Arrange
        var request = new UpdateDriverRequest(Guid.NewGuid(), new List<DrivingLicenseCategory>());
        _driverRepositoryMock.Setup(r => r.GetByIdAsync(request.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Driver)null!);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateDriver(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetDriver_ShouldReturnDriverInfo()
    {
        // Arrange
        var driver = new Driver
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            User = new AppUser { Name = "John", Surname = "Doe" },
            DrivingLicenseCategories = new List<DrivingLicenseCategory> { DrivingLicenseCategory.B }
        };

        _driverRepositoryMock.Setup(r => r.GetByIdAsync(driver.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);

        // Act
        var response = await _sut.GetDriver(new GetDriverRequest(driver.Id), CancellationToken.None);

        // Assert
        Assert.Equal(driver.Id, response.Id);
        Assert.Equal("John", response.Name);
        Assert.Equal("Doe", response.Surname);
    }

    [Fact]
    public async Task GetDrivers_ShouldReturnPaginatedList()
    {
        // Arrange
        var drivers = new List<Driver>
        {
            new Driver { Id = Guid.NewGuid(), User = new AppUser { Name = "A", Surname = "A" }, DrivingLicenseCategories = new List<DrivingLicenseCategory>() },
            new Driver { Id = Guid.NewGuid(), User = new AppUser { Name = "B", Surname = "B" }, DrivingLicenseCategories = new List<DrivingLicenseCategory>() }
        }.AsQueryable().BuildMock();

        _driverRepositoryMock.Setup(r => r.GetQueryable())
            .Returns(drivers);

        var request = new GetDriversRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var response = await _sut.GetDrivers(request, CancellationToken.None);

        // Assert
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(2, response.Drivers.Count());
    }

    [Fact]
    public async Task DeleteDriver_ShouldMarkAsInactive()
    {
        // Arrange
        var driver = new Driver { Id = Guid.NewGuid(), IsActive = true };
        _driverRepositoryMock.Setup(r => r.GetByIdAsync(driver.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);

        // Act
        await _sut.DeleteDriver(new DeleteDriverRequest(driver.Id), CancellationToken.None);

        // Assert
        Assert.False(driver.IsActive);
        _driverRepositoryMock.Verify(r => r.UpdateAsync(driver, It.IsAny<CancellationToken>()), Times.Once);
    }
}
