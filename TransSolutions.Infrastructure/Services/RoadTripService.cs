using Microsoft.EntityFrameworkCore;
using TransSolutions.Domain.Interfaces.Repositories;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Domain.Models.BusinessLogic;
using TransSolutions.Shared.Contracts.RoadTrip;
using TransSolutions.Shared.Enums.Vehicle;

namespace TransSolutions.Infrastructure.Services;

public class RoadTripService : IRoadTripService
{
    private readonly IDriverRepository _driverRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IRoadTripRepository _tripRepository;

    public RoadTripService(IDriverRepository driverRepository, IVehicleRepository vehicleRepository, IRoadTripRepository tripRepository)
    {
        _driverRepository = driverRepository;
        _vehicleRepository = vehicleRepository;
        _tripRepository = tripRepository;
    }

    public async Task<CreateRoadTripResponse> CreateTrip(CreateRoadTripRequest request, Guid userId, CancellationToken ct)
    {
        var driver = await _driverRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.AppUserId == userId.ToString(), ct);

        if (driver == null)
            throw new KeyNotFoundException("Driver not found");

        var vehicle = await _vehicleRepository.GetByIdAsync(request.CarId, track: false, ct);

        if (vehicle == null)
            throw new KeyNotFoundException("Car not found");

        bool isAllowed = false;
        switch (vehicle.VehicleType)
        {
            case VehicleType.Car:
                if (driver.DrivingLicenseCategories.Contains(DrivingLicenseCategory.B) || driver.DrivingLicenseCategories.Contains(DrivingLicenseCategory.C))
                    isAllowed = true;
                break;
            case VehicleType.Truck:
                if (driver.DrivingLicenseCategories.Contains(DrivingLicenseCategory.C))
                    isAllowed = true;
                break;
            case VehicleType.Motorcycle:
                if (driver.DrivingLicenseCategories.Contains(DrivingLicenseCategory.A))
                    isAllowed = true;
                break;
            case VehicleType.Bus:
                if (driver.DrivingLicenseCategories.Contains(DrivingLicenseCategory.D))
                    isAllowed = true;
                break;
            default:
                throw new InvalidDataException("Invalid vehicle type");
        }

        if (!isAllowed)
            throw new Exception("Invalid driving license category");

        var roadTrip = new RoadTrip()
        {
            Id = Guid.NewGuid(),
            DriverId = driver.Id,
            VehicleId = request.CarId,
            DeviceId = request.DeviceId,
            StartTime = request.StartDate,
            EndTime = request.EndDate,
            Distance = request.Distance,
            AverageFuelConsumption = request.AverageFuelConsumption
        };

        await _tripRepository.AddAsync(roadTrip, ct);

        return new CreateRoadTripResponse()
        {
            Id = roadTrip.Id
        };
    }

    public async Task DeleteTrip(DeleteRoadTripRequest request, CancellationToken ct)
    {
        var trip = await _tripRepository.GetByIdAsync(request.Id, track: true, ct);
        if (trip == null) throw new KeyNotFoundException("Trip not found");
        await _tripRepository.DeleteAsync(trip, ct);
    }

    public async Task<GetRoadTripResponse> GetTrip(GetRoadTripRequest request, CancellationToken ct)
    {
        var trip = await _tripRepository.GetByIdAsync(request.Id, track: false, ct);
        if (trip == null) throw new KeyNotFoundException("Trip not found");

        return new GetRoadTripResponse
        {
            Id = trip.Id,
            DriverId = trip.DriverId,
            VehicleId = trip.VehicleId,
            DeviceId = trip.DeviceId,
            StartDate = trip.StartTime,
            EndDate = trip.EndTime,
            Distance = trip.Distance,
            AverageFuelConsumption = trip.AverageFuelConsumption
        };
    }

    public async Task<GetRoadTripsResponse> GetTrips(GetRoadTripsRequest request, CancellationToken ct)
    {
        var query = await _tripRepository.GetQueryable(ct);

        if (!string.IsNullOrEmpty(request.DriverName))
        {
            query = query.Where(x => x.Driver.User.FullNameComputed.Contains(request.DriverName));
        }

        if (!string.IsNullOrEmpty(request.VehicleName))
        {
            query = query.Where(x => x.Vehicle.Name.Contains(request.VehicleName));
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.StartTime >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.EndTime <= request.EndDate.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var trips = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new GetRoadTripsResponse
        {
            TotalCount = totalCount,
            RoadTrips = trips.Select(x => new GetRoadTripResponse
            {
                Id = x.Id,
                DriverId = x.DriverId,
                VehicleId = x.VehicleId,
                DeviceId = x.DeviceId,
                StartDate = x.StartTime,
                EndDate = x.EndTime,
                Distance = x.Distance,
                AverageFuelConsumption = x.AverageFuelConsumption
            })
        };
    }

    public async Task<GetRoadTripsResponse> GetTripsByDeviceId(GetRoadTripsByDeviceIdRequest request, CancellationToken ct)
    {
        var query = await _tripRepository.GetQueryable(ct);
        query = query.Where(x => x.DeviceId == request.DeviceId);

        var totalCount = await query.CountAsync(ct);
        var trips = await query
            .OrderByDescending(x => x.StartTime)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new GetRoadTripsResponse
        {
            TotalCount = totalCount,
            RoadTrips = trips.Select(x => new GetRoadTripResponse
            {
                Id = x.Id,
                DriverId = x.DriverId,
                VehicleId = x.VehicleId,
                DeviceId = x.DeviceId,
                StartDate = x.StartTime,
                EndDate = x.EndTime,
                Distance = x.Distance,
                AverageFuelConsumption = x.AverageFuelConsumption
            })
        };
    }
}
