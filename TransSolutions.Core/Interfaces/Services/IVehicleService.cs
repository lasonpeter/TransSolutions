using TransSolutions.Shared.Contracts.Driver;
using TransSolutions.Shared.Contracts.Vehicle;

namespace TransSolutions.Domain.Interfaces.Services;

public interface IVehicleService
{
    public Task<CreateVehicleResponse> CreateVehicle(CreateVehicleRequest request, CancellationToken ct);
    public Task UpdateVehicle(UpdateVehicleRequest request, CancellationToken ct);
    public Task DeleteVehicle(DeleteVehicleRequest request, CancellationToken ct);
    public Task<GetVehicleResponse> GetVehicle(GetVehicleRequest request, CancellationToken ct);
    public Task<GetVehiclesResponse> GetVehicles(GetVehiclesRequest request, CancellationToken ct);
}
