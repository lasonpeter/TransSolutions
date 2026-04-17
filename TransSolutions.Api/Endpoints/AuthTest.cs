using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using TransSolutions.Shared.CustomClaims;

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
        foreach (var VARIABLE in User.Claims)
        {
            Console.WriteLine(VARIABLE.Type + " : " + VARIABLE.Value);
        }
    }
}