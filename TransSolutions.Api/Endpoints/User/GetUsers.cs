using FastEndpoints;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Shared.Contracts.Auth;
using GetUsersResponse = TransSolutions.Shared.Contracts.Auth.GetUsersResponse;

namespace TransSolutions.Endpoints.User;

public class GetUsers : Endpoint<GetUsersRequest, GetUsersResponse>
{
    private readonly IIdentityService _identityService;

    public GetUsers(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Get("/api/v1/user/get-users");
    }

    public override async Task HandleAsync(GetUsersRequest req, CancellationToken ct)
    {
        var response = await _identityService.GetUsers(req, ct);
        await Send.OkAsync(response, ct);
    }
}
