using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.Vehicle;

namespace TransSolutions.Endpoints.Vehicle;

public class DeleteVehicle : Endpoint<DeleteVehicleRequest>
{
    private readonly IVehicleService _vehicleService;

    public DeleteVehicle(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    public override void Configure()
    {
        Delete("/api/v1/vehicle/delete-vehicle");
    }

    public override async Task HandleAsync(DeleteVehicleRequest req, CancellationToken ct)
    {
        await _vehicleService.DeleteVehicle(req, ct);
        await Send.OkAsync();
    }
}