using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.Vehicle;

namespace TransSolutions.Endpoints.Vehicle;

public class GetVehicles : Endpoint<GetVehiclesRequest, GetVehiclesResponse>
{
    private readonly IVehicleService _vehicleService;

    public GetVehicles(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    public override void Configure()
    {
        Get("/api/v1/vehicle/get-vehicles");
    }

    public override async Task HandleAsync(GetVehiclesRequest req, CancellationToken ct)
    {
        var response = await _vehicleService.GetVehicles(req, ct);
        await Send.OkAsync(response, ct);
    }
}
