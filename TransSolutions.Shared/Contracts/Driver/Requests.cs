using TransSolutions.Shared.Enums.Vehicle;

namespace TransSolutions.Shared.Contracts.Driver;

public record CreateDriverRequest(Guid UserId, List<DrivingLicenseCategory> DrivingLicenseCategories);
public record UpdateDriverRequest(Guid Id, List<DrivingLicenseCategory> DrivingLicenseCategories);
public record DeleteDriverRequest(Guid Id);
public record GetDriverRequest(Guid Id);
/// <summary>
/// In more complex scenario a "faucets" approach could be used
/// </summary>
public record GetDriversRequest(string? FullName = null, int PageNumber = 1, int PageSize = 10, bool? IsActive = true);
