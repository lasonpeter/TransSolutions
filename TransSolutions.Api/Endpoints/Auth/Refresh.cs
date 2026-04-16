using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.Auth;

namespace TransSolutions.Endpoints.Auth;

public class Refresh : Endpoint<RefreshRequest, RefreshResponse>
{
    private readonly IIdentityService _identityService;
    public Refresh(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Post("/api/v1/auth/refresh");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RefreshRequest req, CancellationToken ct)
    {
        var result = await _identityService.RefreshTokenAsync(req.AccessToken, req.RefreshToken);

        if (result == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        await Send.OkAsync(result, cancellation: ct);
    }
}