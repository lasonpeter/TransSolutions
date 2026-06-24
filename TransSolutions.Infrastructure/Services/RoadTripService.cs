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
        var driver = await _driverRepository.GetByIdAsync(request.DriverId, track: false, ct);

        if (driver == null)
            throw new KeyNotFoundException("Driver not found");

        var vehicle = await _vehicleRepository.GetByIdAsync(request.CarId, track: false, ct: ct);

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

        var points = await _tripRepository.GetPointsAsync(request.Id, ct);

        return new GetRoadTripResponse
        {
            Id = trip.Id,
            DriverId = trip.DriverId,
            VehicleId = trip.VehicleId,
            StartDate = trip.StartTime,
            EndDate = trip.EndTime,
            Distance = trip.Distance,
            AverageFuelConsumption = trip.AverageFuelConsumption,
            Points = points.Select(p => new RoadTripPointDto
            {
                RoadTripId = p.RoadTripId,
                Timestamp = p.Timestamp,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Altitude = p.Altitude
            }).ToList()
        };
    }

    public async Task AddTripPoint(AddRoadTripPointRequest request, CancellationToken ct)
    {
        var trip = await _tripRepository.GetByIdAsync(request.RoadTripId, track: true, ct);
        if (trip == null) throw new KeyNotFoundException("Trip not found");

        var point = new RoadTripPoint
        {
            RoadTripId = request.RoadTripId,
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Altitude = request.Altitude
        };

        await _tripRepository.AddPointAsync(point, ct);

        // Fetch all points to compute cumulative distance
        var points = await _tripRepository.GetPointsAsync(request.RoadTripId, ct);
        double totalDistance = 0;
        for (int i = 0; i < points.Count - 1; i++)
        {
            totalDistance += CalculateDistance(
                points[i].Latitude, points[i].Longitude,
                points[i + 1].Latitude, points[i + 1].Longitude
            );
        }

        trip.Distance = (float)totalDistance;
        await _tripRepository.UpdateAsync(trip, ct);
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
                StartDate = x.StartTime,
                EndDate = x.EndTime,
                Distance = x.Distance,
                AverageFuelConsumption = x.AverageFuelConsumption
            })
        };
    }

    public async Task<GetRoadTripsResponse> GetTripsByDriverId(GetRoadTripsByDriverIdRequest request, CancellationToken ct)
    {
        var query = await _tripRepository.GetQueryable(ct);
        query = query.Where(x => x.DriverId == request.DriverId);

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
                StartDate = x.StartTime,
                EndDate = x.EndTime,
                Distance = x.Distance,
                AverageFuelConsumption = x.AverageFuelConsumption
            })
        };
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var r = 6371.0; // radius of Earth in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return r * c;
    }

    private static double ToRadians(double val)
    {
        return (Math.PI / 180.0) * val;
    }
}
