using FluentValidation.TestHelper;
using TransSolutions.Shared.Contracts.Vehicle;
using TransSolutions.Shared.Enums.Vehicle;
using Xunit;

namespace TransSolutions.Testing.ContractValidators.Vehicle;

public class VehicleValidatorTests
{
    private readonly CreateVehicleValidator _createValidator = new();
    private readonly UpdateVehicleValidator _updateValidator = new();
    private readonly DeleteVehicleValidator _deleteValidator = new();
    private readonly GetVehicleValidator _getValidator = new();
    private readonly GetVehiclesValidator _getListValidator = new();

    [Fact]
    public void CreateVehicle_ValidRequest_Passes()
    {
        var request = new CreateVehicleRequest
        {
            Name = "Test Truck",
            RegistrationPlateNumber = "ABC-123",
            VehicleType = VehicleType.Truck
        };

        var result = _createValidator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void CreateVehicle_InvalidName_Fails(string name)
    {
        var request = new CreateVehicleRequest { Name = name };
        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void UpdateVehicle_ValidRequest_Passes()
    {
        var request = new UpdateVehicleRequest
        {
            Id = Guid.NewGuid(),
            Name = "Updated Truck",
            RegistrationPlateNumber = "DEF-456",
            VehicleType = VehicleType.Truck
        };

        var result = _updateValidator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DeleteVehicle_EmptyId_Fails()
    {
        var request = new DeleteVehicleRequest { Id = Guid.Empty };
        var result = _deleteValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void GetVehicles_InvalidPage_Fails()
    {
        var request = new GetVehiclesRequest { PageNumber = 0 };
        var result = _getListValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }
}
