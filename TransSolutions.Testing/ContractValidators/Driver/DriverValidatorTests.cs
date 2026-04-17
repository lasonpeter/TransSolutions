using FluentValidation.TestHelper;
using TransSolutions.Shared.Contracts.Driver;
using TransSolutions.Shared.Enums.Vehicle;
using Xunit;

namespace TransSolutions.Testing.ContractValidators.Driver;

public class DriverValidatorTests
{
    private readonly CreateDriverValidator _createValidator = new();
    private readonly UpdateDriverValidator _updateValidator = new();
    private readonly DeleteDriverValidator _deleteValidator = new();
    private readonly GetDriverValidator _getValidator = new();
    private readonly GetDriversValidator _getListValidator = new();

    [Fact]
    public void CreateDriver_ValidRequest_Passes()
    {
        var request = new CreateDriverRequest(
            Guid.NewGuid(),
            new List<DrivingLicenseCategory> { DrivingLicenseCategory.B }
        );

        var result = _createValidator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateDriver_EmptyLicense_Fails()
    {
        var request = new CreateDriverRequest(Guid.NewGuid(), new List<DrivingLicenseCategory>());
        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DrivingLicenseCategories);
    }

    [Fact]
    public void CreateDriver_EmptyUserId_Fails()
    {
        var request = new CreateDriverRequest(Guid.Empty, new List<DrivingLicenseCategory> { DrivingLicenseCategory.B });
        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void UpdateDriver_ValidRequest_Passes()
    {
        var request = new UpdateDriverRequest(
            Guid.NewGuid(),
            new List<DrivingLicenseCategory> { DrivingLicenseCategory.C }
        );

        var result = _updateValidator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DeleteDriver_EmptyId_Fails()
    {
        var request = new DeleteDriverRequest(Guid.Empty);
        var result = _deleteValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void GetDrivers_InvalidPageSize_Fails()
    {
        var request = new GetDriversRequest(PageSize: 101);
        var result = _getListValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}
