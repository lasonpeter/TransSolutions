using FastEndpoints;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.Auth;
using TransSolutions.Api.Mappers;
using TransSolutions.Shared.CustomClaims;

namespace TransSolutions.Endpoints.Auth;

public class UpdateUser : Endpoint<UpdateUserRequest>
{
    private readonly IIdentityService _identityService;

    public UpdateUser(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Post("/api/v1/user/update/{UserId}");
        Claims(CustomClaims.AdminClaim, CustomClaims.ManagerClaim);
    }

    public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
    {
        var targetUserId = Route<string>("UserId");
        if (!Guid.TryParse(targetUserId, out _))
        {
            ThrowError("Userid is required");
            return;
        }

        var result = await _identityService.UpdateUserAsync(targetUserId, req, User);
        
        this.ThrowIfInvalid(result);

        await Send.NoContentAsync(ct);
    }
}
