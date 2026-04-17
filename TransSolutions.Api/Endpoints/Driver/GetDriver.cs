using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.Driver;

namespace TransSolutions.Endpoints.Driver;

public class GetDriver : Endpoint<GetDriverRequest, GetDriverResponse>
{
    private readonly IDriverService _driverService;

    public GetDriver(IDriverService driverService)
    {
        _driverService = driverService;
    }

    public override void Configure()
    {
        Get("/api/v1/driver/get-driver");
    }

    public override async Task HandleAsync(GetDriverRequest req, CancellationToken ct)
    {
        var response = await _driverService.GetDriver(req, ct);
        await Send.OkAsync(response, ct);
    }
}
