using TransSolutions.Application.DTOs.Auth;
using TransSolutions.Shared.Contracts.Auth;

namespace TransSolutions.Domain.Interfaces.Services;


//TODO ALSO ADD REGISTRATION IN HERE
public interface IIdentityService
{
    Task<LoginResponse?> LoginAsync(string email, string password);
    Task<RefreshResponse?> RefreshTokenAsync(string accessToken, string refreshToken);
    Task<bool> LogoutAsync(string userId);
}