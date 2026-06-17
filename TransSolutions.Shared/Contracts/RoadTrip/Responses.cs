namespace TransSolutions.Shared.Contracts.RoadTrip;

public record GetRoadTripResponse
{
    public Guid Id { get; init; }
    public Guid DriverId { get; init; }
    public Guid VehicleId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public float Distance { get; init; }
    public float AverageFuelConsumption { get; init; }
}

public record GetRoadTripsResponse
{
    public IEnumerable<GetRoadTripResponse> RoadTrips { get; init; }
    public int TotalCount { get; init; }
}

public record CreateRoadTripResponse
{
    public Guid Id { get; init; }
}
