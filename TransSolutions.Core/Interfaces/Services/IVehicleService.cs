using TransSolutions.Shared.Contracts.Driver;
using TransSolutions.Shared.Contracts.Vehicle;

namespace TransSolutions.Domain.Interfaces.Services;

/// <summary>
/// Service for managing vehicles.
/// </summary>
public interface IVehicleService
{
    // Adds a new vehicle to the system
    public Task<CreateVehicleResponse> CreateVehicle(CreateVehicleRequest request, CancellationToken ct);
    
    // Updates existing vehicle details
    public Task UpdateVehicle(UpdateVehicleRequest request, CancellationToken ct);
    
    // Marks a vehicle as inactive
    public Task DeleteVehicle(DeleteVehicleRequest request, CancellationToken ct);
    
    // Gets information for a specific vehicle
    public Task<GetVehicleResponse> GetVehicle(GetVehicleRequest request, CancellationToken ct);
    
    // Gets a paginated list of vehicles
    public Task<GetVehiclesResponse> GetVehicles(GetVehiclesRequest request, CancellationToken ct);
}