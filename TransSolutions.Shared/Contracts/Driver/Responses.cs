using TransSolutions.Shared.Enums.Vehicle;

namespace TransSolutions.Shared.Contracts.Driver;

 public record GetDriverResponse
 {
     public Guid Id { get; init; }
     public string Name { get; init; }
     public string Surname { get; init; }
     public IEnumerable<DrivingLicenseCategory> DrivingLicenseCategories { get; init; }
 }

 public record GetDriversResponse
 {
     public IEnumerable<GetDriverResponse> Drivers { get; init; }
     public int TotalCount { get; init; }
 }

 public record CreateDriverResponse
 {
     public Guid Id { get; init; }
 }