using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.RoadTrip;

namespace TransSolutions.Endpoints.RoadTrip;

public class DeleteRoadTrip : Endpoint<DeleteRoadTripRequest>
{
    private readonly IRoadTripService _roadTripService;

    public DeleteRoadTrip(IRoadTripService roadTripService)
    {
        _roadTripService = roadTripService;
    }

    public override void Configure()
    {
        Delete("/api/v1/road-trip/delete");
    }

    public override async Task HandleAsync(DeleteRoadTripRequest req, CancellationToken ct)
    {
        await _roadTripService.DeleteTrip(req, ct);
        await Send.NoContentAsync(ct);
    }
}