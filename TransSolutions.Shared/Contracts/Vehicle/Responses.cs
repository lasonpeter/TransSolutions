using TransSolutions.Shared.Enums.Vehicle;

namespace TransSolutions.Shared.Contracts.Vehicle;

public record GetVehicleResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string RegistrationPlateNumber { get; init; } = default!;
    public VehicleType VehicleType { get; init; } 
    public DateTime CreatedAt { get; init; }
    public bool IsActive { get; init; }
}

public record GetVehiclesResponse
{
    public IEnumerable<GetVehicleResponse> Vehicles { get; init; }
    public int TotalCount { get; init; }
    
}

public record CreateVehicleResponse
{
    public Guid Id { get; init; }
}