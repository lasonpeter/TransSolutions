using TransSolutions.Shared.Enums.Vehicle;

namespace TransSolutions.Shared.Contracts.Driver;

 public record GetDriverResponse(Guid Id, string Name,string Surname, List<DrivingLicenseCategory> DrivingLicenseCategories);
 public record GetDriversResponse(IEnumerable<GetDriverResponse> Drivers, int PageNumber, int PageSize, int TotalCount);