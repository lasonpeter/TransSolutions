using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.RoadTrip;

namespace TransSolutions.Endpoints.RoadTrip;

public class GetRoadTrip : Endpoint<GetRoadTripRequest, GetRoadTripResponse>
{
    private readonly IRoadTripService _roadTripService;

    public GetRoadTrip(IRoadTripService roadTripService)
    {
        _roadTripService = roadTripService;
    }

    public override void Configure()
    {
        Get("/api/v1/road-trip/get");
    }

    public override async Task HandleAsync(GetRoadTripRequest req, CancellationToken ct)
    {
        var response = await _roadTripService.GetTrip(req, ct);
        await Send.OkAsync(response, ct);
    }
}