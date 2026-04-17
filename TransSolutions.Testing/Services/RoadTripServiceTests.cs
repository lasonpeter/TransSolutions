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

    [Fact]
    public async Task GetTrip_ShouldReturnDriverIdAndVehicleId()
    {
        // Arrange
        var trip = new RoadTrip
        {
            Id = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(2),
            Distance = 150,
            AverageFuelConsumption = 7.5f
        };
        _tripRepositoryMock.Setup(r => r.GetByIdAsync(trip.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        // Act
        var response = await _sut.GetTrip(new GetRoadTripRequest { Id = trip.Id }, CancellationToken.None);

        // Assert
        Assert.Equal(trip.DriverId, response.DriverId);
        Assert.Equal(trip.VehicleId, response.VehicleId);
    }

    [Fact]
    public async Task GetTrips_ShouldReturnDriverIdAndVehicleId()
    {
        // Arrange
        var trip = new RoadTrip
        {
            Id = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(2),
            Distance = 150,
            AverageFuelConsumption = 7.5f
        };
        
        var queryable = new List<RoadTrip> { trip }.AsQueryable().BuildMock();

        _tripRepositoryMock.Setup(r => r.GetQueryable(It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryable);

        // Act
        var response = await _sut.GetTrips(new GetRoadTripsRequest(), CancellationToken.None);

        // Assert
        var tripResponse = response.RoadTrips.First();
        Assert.Equal(trip.DriverId, tripResponse.DriverId);
        Assert.Equal(trip.VehicleId, tripResponse.VehicleId);
    }
}

// Simple MockQueryable implementation for tests without extra dependencies
public static class QueryableExtensions
{
    public static IQueryable<T> BuildMock<T>(this IQueryable<T> source)
    {
        var mock = new Mock<IQueryable<T>>();
        var enumerable = source.AsEnumerable();

        mock.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(enumerable.GetEnumerator()));

        mock.Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(source.Provider));

        mock.Setup(m => m.Expression).Returns(source.Expression);
        mock.Setup(m => m.ElementType).Returns(source.ElementType);
        mock.Setup(m => m.GetEnumerator()).Returns(enumerable.GetEnumerator());

        return mock.Object;
    }
}

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression)!;
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = ((IQueryProvider)this).Execute(expression);

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }
}
