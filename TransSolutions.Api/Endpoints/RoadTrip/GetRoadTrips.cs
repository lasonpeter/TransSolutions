using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.RoadTrip;

namespace TransSolutions.Endpoints.RoadTrip;

public class GetRoadTrips : Endpoint<GetRoadTripsRequest, GetRoadTripsResponse>
{
    private readonly IRoadTripService _roadTripService;

    public GetRoadTrips(IRoadTripService roadTripService)
    {
        _roadTripService = roadTripService;
    }

    public override void Configure()
    {
        Get("/api/v1/road-trip/get-road-trips");
    }

    public override async Task HandleAsync(GetRoadTripsRequest req, CancellationToken ct)
    {
        var response = await _roadTripService.GetTrips(req, ct);
        await Send.OkAsync(response, ct);
    }
}