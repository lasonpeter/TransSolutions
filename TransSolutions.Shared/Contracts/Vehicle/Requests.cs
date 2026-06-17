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

public record GetVehiclesRequest(string? Name = null, string? RegistrationPlateNumber = null, int PageNumber = 1, int PageSize = 10, bool? IsActive = true);
