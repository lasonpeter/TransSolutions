using System.IdentityModel.Tokens.Jwt;
using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.RoadTrip;
using TransSolutions.Shared.CustomClaims;

namespace TransSolutions.Endpoints.RoadTrip;

public class CreateRoadTrip : Endpoint<CreateRoadTripRequest,CreateRoadTripResponse>
{
    private readonly IRoadTripService _roadTripService;

    public CreateRoadTrip(IRoadTripService roadTripService)
    {
        _roadTripService = roadTripService;
    }

    public override void Configure()
    {
        Post("/api/v1/road-trip/create");
        Claims(CustomClaims.DriverClaim);
        Claims(CustomClaims.AdminClaim, CustomClaims.ManagerClaim);
    }

    public override async Task HandleAsync(CreateRoadTripRequest req, CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!Guid.TryParse(userId, out var id))
                await Send.UnauthorizedAsync(ct);
            var response = await _roadTripService.CreateTrip(req, id, ct);
            await Send.OkAsync(response, ct);
        }
        catch (Exception e)
        {
            AddError(e.Message);
            ThrowIfAnyErrors();
        }
    }
}