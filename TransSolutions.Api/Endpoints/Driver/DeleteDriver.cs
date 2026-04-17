using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.Driver;

namespace TransSolutions.Endpoints.Driver;

public class DeleteDriver : Endpoint<DeleteDriverRequest>
{
    private readonly IDriverService _driverService;

    public DeleteDriver(IDriverService driverService)
    {
        _driverService = driverService;
    }

    public override void Configure()
    {
        Delete("/api/v1/driver/delete-driver");
    }

    public override async Task HandleAsync(DeleteDriverRequest req, CancellationToken ct)
    {
        await _driverService.DeleteDriver(req, ct);
        await Send.OkAsync(ct);
    }
}
