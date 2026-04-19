using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.RoadTrip;

namespace TransSolutions.Endpoints.RoadTrip;

public class GetRoadTripsByDeviceId : Endpoint<GetRoadTripsByDeviceIdRequest, GetRoadTripsResponse>
{
    private readonly IRoadTripService _roadTripService;

    public GetRoadTripsByDeviceId(IRoadTripService roadTripService)
    {
        _roadTripService = roadTripService;
    }

    public override void Configure()
    {
        Get("/api/v1/road-trip/get-road-trips-by-device/{DeviceId}");
    }

    public override async Task HandleAsync(GetRoadTripsByDeviceIdRequest req, CancellationToken ct)
    {
        var response = await _roadTripService.GetTripsByDeviceId(req, ct);
        await Send.OkAsync(response, ct);
    }
}
