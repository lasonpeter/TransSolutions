using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.Driver;

namespace TransSolutions.Endpoints.Driver;

public class GetDrivers : Endpoint<GetDriversRequest, GetDriversResponse>
{
    private readonly IDriverService _driverService;

    public GetDrivers(IDriverService driverService)
    {
        _driverService = driverService;
    }

    public override void Configure()
    {
        Get("/api/v1/driver/get-drivers");
    }

    public override async Task HandleAsync(GetDriversRequest req, CancellationToken ct)
    {
        var response = await _driverService.GetDrivers(req, ct);
        await Send.OkAsync(response, ct);
    }
}
