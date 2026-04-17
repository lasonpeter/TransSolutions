using TransSolutions.Shared.Enums.Vehicle;

namespace TransSolutions.Shared.Contracts.Vehicle;

public record CreateVehicleRequest
{
    public string Name { get; init; }
    public string RegistrationPlateNumber { get; init; }
    public VehicleType VehicleType { get; init; }
}

public record DeleteVehicleRequest
{
    public Guid Id { get; init; }
}

public record UpdateVehicleRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string RegistrationPlateNumber { get; init; }
    public VehicleType VehicleType { get; init; } 
}

public record GetVehicleRequest
{
    public Guid Id { get; init; }
    public bool? IsActive { get; init; } = true;
}

public record GetVehiclesRequest
{
    public string? Name { get; init; }
    public string? RegistrationPlateNumber { get; init; }
    public int PageNumber { get; init; }= 1;
    public int PageSize { get; init; } = 10;
    public bool? IsActive { get; init; } = true;
}
