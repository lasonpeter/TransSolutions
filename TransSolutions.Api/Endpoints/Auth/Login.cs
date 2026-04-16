using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.Auth;

namespace TransSolutions.Endpoints.Auth;

public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly IIdentityService _identityService;

    public LoginEndpoint(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Post("/api/v1/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var result = await _identityService.LoginAsync(req.Email, req.Password);
        
        if (result == null)
        {
            await Send.UnauthorizedAsync();
            return;
        }

        await Send.OkAsync(result, cancellation: ct);
    }
}