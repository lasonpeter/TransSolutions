using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.Vehicle;

namespace TransSolutions.Endpoints.Vehicle;

public class UpdateVehicle : Endpoint<UpdateVehicleRequest>
{
    private readonly IVehicleService _vehicleService;

    public UpdateVehicle(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    public override void Configure()
    {
        Post("/api/v1/vehicle/update-vehicle");
    }

    public override async Task HandleAsync(UpdateVehicleRequest req, CancellationToken ct)
    {
        await _vehicleService.UpdateVehicle(req, ct);
        await Send.OkAsync();
    }
}