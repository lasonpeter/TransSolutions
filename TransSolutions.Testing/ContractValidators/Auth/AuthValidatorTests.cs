using FluentValidation.TestHelper;
using TransSolutions.Shared.Contracts.Auth;
using TransSolutions.Shared.Enums.Auth;
using Xunit;

namespace TransSolutions.Testing.ContractValidators.Auth;

public class AuthValidatorTests
{
    private readonly RegisterValidator _registerValidator = new();
    private readonly LoginValidator _loginValidator = new();
    private readonly RefreshValidator _refreshValidator = new();

    [Fact]
    public void Register_ValidRequest_Passes()
    {
        var request = new RegisterRequest("test@example.com", "password123", "John", "Doe", UserRole.Driver);
        var result = _registerValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    public void Register_InvalidEmail_Fails(string email)
    {
        var request = new RegisterRequest(email, "password123", "John", "Doe", UserRole.Driver);
        var result = _registerValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Login_ValidRequest_Passes()
    {
        var request = new LoginRequest("test@example.com", "password123");
        var result = _loginValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Refresh_EmptyTokens_Fails()
    {
        var request = new RefreshRequest("", "");
        var result = _refreshValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AccessToken);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}
