using Microsoft.EntityFrameworkCore;
using TransSolutions.Domain.Interfaces.Repositories;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Domain.Models.BusinessLogic;
using TransSolutions.Shared.Contracts.Vehicle;

namespace TransSolutions.Infrastructure.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleService(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<CreateVehicleResponse> CreateVehicle(CreateVehicleRequest request, CancellationToken ct)
    {
        var vehicle = new Vehicle()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Name = request.Name,
            RegistrationPlateNumber = request.RegistrationPlateNumber,
            VehicleType = request.VehicleType,
            IsActive = true
        };
        await _vehicleRepository.AddAsync(vehicle, ct);
        return new CreateVehicleResponse { Id = vehicle.Id };
    }

    public async Task UpdateVehicle(UpdateVehicleRequest request, CancellationToken ct)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.Id, track: true, ct);

        if (vehicle is null || !vehicle.IsActive)
            throw new KeyNotFoundException($"Vehicle with ID {request.Id} not found.");

        vehicle.Name = request.Name;
        vehicle.RegistrationPlateNumber = request.RegistrationPlateNumber;
        vehicle.VehicleType = request.VehicleType;
        
        await _vehicleRepository.UpdateAsync(vehicle, ct);
    }

    public async Task DeleteVehicle(DeleteVehicleRequest request, CancellationToken ct)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.Id, track: true, ct);

        if (vehicle is null)
            throw new KeyNotFoundException("Vehicle not found.");

        // Logical delete
        vehicle.IsActive = false;
        await _vehicleRepository.UpdateAsync(vehicle, ct);
    }

    public async Task<GetVehicleResponse> GetVehicle(GetVehicleRequest request, CancellationToken ct)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.Id, track: false, ct);

        if (vehicle is null || !vehicle.IsActive)
            throw new KeyNotFoundException("Vehicle not found.");

        return new GetVehicleResponse
        {
            Id = vehicle.Id,
            Name = vehicle.Name,
            RegistrationPlateNumber = vehicle.RegistrationPlateNumber,
            VehicleType = vehicle.VehicleType,
            CreatedAt = vehicle.CreatedAt,
            IsActive = vehicle.IsActive
        };
    }

    public async Task<GetVehiclesResponse> GetVehicles(GetVehiclesRequest request, CancellationToken ct)
    {
        var query = _vehicleRepository.GetQueryable();

        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{request.Name}%"));

        if (!string.IsNullOrWhiteSpace(request.RegistrationPlateNumber))
            query = query.Where(x => EF.Functions.ILike(x.RegistrationPlateNumber, $"%{request.RegistrationPlateNumber}%"));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(request.PageSize * (request.PageNumber - 1))
            .Take(request.PageSize)
            .Select(v => new GetVehicleResponse
            {
                Id = v.Id,
                Name = v.Name,
                RegistrationPlateNumber = v.RegistrationPlateNumber,
                VehicleType = v.VehicleType,
                CreatedAt = v.CreatedAt,
                IsActive = v.IsActive
            })
            .ToListAsync(ct);

        return new GetVehiclesResponse
        {
            Vehicles = items,
            TotalCount = totalCount
        };
    }
}