using TransSolutions.Application.DTOs.Auth;
using TransSolutions.Shared.Contracts.Auth;

namespace TransSolutions.Domain.Interfaces.Services;

/// <summary>
/// Service for user authentication and identity management.
/// </summary>
public interface IIdentityService
{
    // Authenticates a user and returns access and refresh tokens
    Task<LoginResponse?> LoginAsync(string email, string password);
    
    // Refreshes an expired access token using a valid refresh token
    Task<RefreshResponse?> RefreshTokenAsync(string accessToken, string refreshToken);
    
    // Revokes current user refresh tokens
    Task<bool> LogoutAsync(string userId);
    
    // Gets a paginated list of users with search by full name
    Task<GetUsersResponse> GetUsers(GetUsersRequest request, CancellationToken ct);
}