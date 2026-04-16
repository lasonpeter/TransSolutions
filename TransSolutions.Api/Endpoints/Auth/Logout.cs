using FastEndpoints;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using TransSolutions.Domain.Interfaces.Services;

namespace TransSolutions.Endpoints.Auth;

public class LogoutEndpoint : EndpointWithoutRequest
{
    private readonly IIdentityService _identityService;

    public LogoutEndpoint(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Post("/api/v1/auth/logout");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        if (await _identityService.LogoutAsync(userId))
            await Send.OkAsync();
        else
            await Send.ErrorsAsync(400, ct);
        
    }
}