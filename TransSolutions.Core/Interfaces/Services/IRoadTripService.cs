using TransSolutions.Shared.Contracts.RoadTrip;

namespace TransSolutions.Domain.Interfaces.Services;

/// <summary>
/// Service for managing road trips.
/// </summary>
public interface IRoadTripService
{
    // Creates a new road trip
    Task<CreateRoadTripResponse> CreateTrip(CreateRoadTripRequest request, Guid userId, CancellationToken ct);
    
    // Deletes a specific road trip
    Task DeleteTrip(DeleteRoadTripRequest id, CancellationToken ct);
    
    // Gets detailed information for a specific road trip
    Task<GetRoadTripResponse> GetTrip(GetRoadTripRequest request, CancellationToken ct);
    
    // Gets a list of road trips
    Task<GetRoadTripsResponse> GetTrips(GetRoadTripsRequest request, CancellationToken ct);
}