using TransSolutions.Shared.Contracts.Driver;

namespace TransSolutions.Domain.Interfaces.Services;

/// <summary>
/// Service for managing driver business logic.
/// </summary>
public interface IDriverService
{
    // Creates a new driver
    Task<CreateDriverResponse> CreateDriver(CreateDriverRequest request, CancellationToken ct);
    
    // Updates existing driver information
    Task UpdateDriver(UpdateDriverRequest request, CancellationToken ct);
    
    // Soft-deletes a driver
    Task DeleteDriver(DeleteDriverRequest id, CancellationToken ct);
    
    // Gets detailed information for a specific driver
    Task<GetDriverResponse> GetDriver(GetDriverRequest request, CancellationToken ct);
    
    // Gets a paginated list of drivers with optional filtering
    Task<GetDriversResponse> GetDrivers(GetDriversRequest request, CancellationToken ct);
}