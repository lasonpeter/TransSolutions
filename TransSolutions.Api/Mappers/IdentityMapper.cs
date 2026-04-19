using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using FluentValidation.Results;

namespace TransSolutions.Api.Mappers;

public static class IdentityMapper
{
    public static void ThrowIfInvalid(this IEndpoint ep, IdentityResult result)
    {
        if (result.Succeeded) return;

        foreach (var err in result.Errors)
        {
            var propertyName = err.Code switch
            {
                var c when c.Contains("Password") => "Password",
                var c when c.Contains("Email") => "Email",
                var c when c.Contains("UserName") => "UserName",
                _ => "General"
            };

            ep.ValidationFailures.Add(new ValidationFailure(propertyName, err.Description));
        }

        if (ep.ValidationFailures.Count > 0)
        {
            throw new ValidationFailureException();
        }
    }

    public static void ThrowIfNotFound<T>(this IEndpoint ep, T? result, string message = "Resource not found")
    {
        if (result is null)
        {
            ep.ValidationFailures.Add(new ValidationFailure("General", message));
            throw new ValidationFailureException();
        }
    }

    public static void ThrowIfFailure(this IEndpoint ep, bool success, string errorMessage = "Operation failed", int statusCode = 400)
    {
        if (!success)
        {
            ep.ValidationFailures.Add(new ValidationFailure("General", errorMessage));
            throw new ValidationFailureException();
        }
    }
}
