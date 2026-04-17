using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.Driver;

namespace TransSolutions.Endpoints.Driver;

public class UpdateDriver : Endpoint<UpdateDriverRequest>
{
    private readonly IDriverService _driverService;

    public UpdateDriver(IDriverService driverService)
    {
        _driverService = driverService;
    }

    public override void Configure()
    {
        Put("/api/v1/driver/update-driver");
    }

    public override async Task HandleAsync(UpdateDriverRequest req, CancellationToken ct)
    {
        await _driverService.UpdateDriver(req, ct);
        await Send.OkAsync();
    }
}
