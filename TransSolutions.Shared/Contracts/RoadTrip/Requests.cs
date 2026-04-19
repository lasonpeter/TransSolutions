namespace TransSolutions.Shared.Contracts.RoadTrip;

public record CreateRoadTripRequest
{
    public Guid CarId { get; init; }
    public Guid DeviceId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public float Distance { get; init; }
    public float AverageFuelConsumption { get; init; }
}

public record UpdateRoadTripRequest
{
    public Guid Id { get; init; }
    public Guid? CarId { get; init; }
    public Guid? DeviceId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public float? Distance { get; init; }
    public float? AverageFuelConsumption { get; init; }
}

public record GetRoadTripRequest
{
    public Guid Id { get; init; }
}

public record GetRoadTripsRequest
{
    public string? DriverName { get; init; }
    
    public string? VehicleName { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int PageNumber { get; init; }= 1;
    public int PageSize { get; init; } = 10;
}

public record GetRoadTripsByDeviceIdRequest
{
    public Guid DeviceId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record DeleteRoadTripRequest
{
    public Guid Id { get; init; }
}
