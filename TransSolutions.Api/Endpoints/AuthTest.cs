using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TransSolutions.Endpoints;

public class AuthTest: EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/auth/test");
        
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync();
    }
}