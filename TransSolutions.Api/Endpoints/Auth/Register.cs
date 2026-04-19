// WebApi/Endpoints/Auth/RegisterEndpoint.cs
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TransSolutions.Domain.Models.Auth;
using TransSolutions.Shared.Contracts.Auth;
using TransSolutions.Shared.CustomClaims;
using TransSolutions.Shared.Enums.Auth;
using TransSolutions.Api.Mappers;


public class Register : Endpoint<RegisterRequest>
{
    private readonly UserManager<AppUser> _userManager;

    public Register(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public override void Configure()
    {
        Post("api/v1/auth/register");
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var isAdmin = User.HasClaim(c => c.Type == CustomClaims.AdminClaim && c.Value == "true");
        var isManager = User.HasClaim(c => c.Type == CustomClaims.ManagerClaim && c.Value == "true");

        bool canCreate = isAdmin || (isManager && req.Role == UserRole.Driver);

        if (!canCreate)
        {
            await Send.ErrorsAsync(403, ct);
            return;
        }

        var user = new AppUser() { UserName = req.Email, Email = req.Email, Name = req.Name, Surname = req.Surname };
        var result = await _userManager.CreateAsync(user, req.Password);
        
        this.ThrowIfInvalid(result);

        var claimType = req.Role switch
        {
            UserRole.Admin => CustomClaims.AdminClaim,
            UserRole.Manager => CustomClaims.ManagerClaim,
            UserRole.Driver => CustomClaims.DriverClaim,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var claimResult = await _userManager.AddClaimAsync(user, new Claim(claimType, "true"));
        this.ThrowIfInvalid(claimResult);
        
        await Send.OkAsync();
    }
}