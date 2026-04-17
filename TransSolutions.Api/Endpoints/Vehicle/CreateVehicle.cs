using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.Vehicle;

namespace TransSolutions.Endpoints.Vehicle;

public class CreateVehicle : Endpoint<CreateVehicleRequest,CreateVehicleResponse>
{
    private readonly IVehicleService _vehicleService;

    public CreateVehicle(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    public override void Configure()
    {
        Post("/api/v1/vehicle/create-vehicle");
    }
    
    public override async Task HandleAsync(CreateVehicleRequest req, CancellationToken ct)
    {
        var response = await _vehicleService.CreateVehicle(req, ct);
        
        await Send.OkAsync(response,ct);
    }
}