using TransSolutions.Shared.Contracts.Driver;

namespace TransSolutions.Domain.Interfaces.Services;

public interface IDriverService
{
    Task<CreateDriverResponse> CreateDriver(CreateDriverRequest request, CancellationToken ct);
    Task UpdateDriver(UpdateDriverRequest request, CancellationToken ct);
    Task DeleteDriver(DeleteDriverRequest id, CancellationToken ct);
    
    Task<GetDriverResponse> GetDriver(GetDriverRequest request, CancellationToken ct);
    Task<GetDriversResponse> GetDrivers(GetDriversRequest request, CancellationToken ct);
}