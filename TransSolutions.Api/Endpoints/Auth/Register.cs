// WebApi/Endpoints/Auth/RegisterEndpoint.cs
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using TransSolutions.Domain.Models.Auth;
using TransSolutions.Shared.Contracts.Auth;


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
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var user = new AppUser() { UserName = req.Email, Email = req.Email, Name = req.Name, Surname = req.Surname };
        var result = await _userManager.CreateAsync(user, req.Password);
        
        if (result.Succeeded)
            await Send.OkAsync();
        else
            await Send.ErrorsAsync(400, ct);
    }
}