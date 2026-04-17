using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.Vehicle;

namespace TransSolutions.Endpoints.Vehicle;

public class GetVehicle : Endpoint<GetVehicleRequest,GetVehicleResponse>
{
    private readonly IVehicleService _vehicleService;

    public GetVehicle(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    public override void Configure()
    {
        ResponseCache(5);
        Get("/api/v1/vehicle/get-vehicle");
    }

    public override async Task HandleAsync(GetVehicleRequest req, CancellationToken ct)
    {
        var response = await _vehicleService.GetVehicle(req, ct);
        ThrowIfAnyErrors();
        await Send.OkAsync(response,ct);
    }
}