using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.Driver;

namespace TransSolutions.Endpoints.Driver;

public class CreateDriver : Endpoint<CreateDriverRequest, CreateDriverResponse>
{
    private readonly IDriverService _driverService;

    public CreateDriver(IDriverService driverService)
    {
        _driverService = driverService;
    }

    public override void Configure()
    {
        Post("/api/v1/driver/create-driver");
    }

    public override async Task HandleAsync(CreateDriverRequest req, CancellationToken ct)
    {
        var response = await _driverService.CreateDriver(req, ct);
        await Send.OkAsync(response, ct);
    }
}
