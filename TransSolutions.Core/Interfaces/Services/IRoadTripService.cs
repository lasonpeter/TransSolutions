using TransSolutions.Shared.Contracts.RoadTrip;

namespace TransSolutions.Domain.Interfaces.Services;

public interface IRoadTripService
{
    Task<CreateRoadTripResponse> CreateTrip(CreateRoadTripRequest request,Guid userId, CancellationToken ct);
    Task DeleteTrip(DeleteRoadTripRequest id, CancellationToken ct);
    
    Task<GetRoadTripResponse> GetTrip(GetRoadTripRequest request, CancellationToken ct);
    Task<GetRoadTripsResponse> GetTrips(GetRoadTripsRequest request, CancellationToken ct);
}